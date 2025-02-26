// <copyright file="TenancyContainerScenarioBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.Tenancy.Internal;
    using Corvus.Testing.ReqnRoll;

    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Reqnroll;

    [Binding]
    public class TenancyContainerScenarioBindings
    {
        private readonly FeatureContext featureContext;
        private readonly ScenarioContext scenarioContext;
        private RootTenant? rootTenant;

        static TenancyContainerScenarioBindings()
        {
            var configData = new Dictionary<string, string?>
            {
                //// Add configuration value pairs here
                { "TestSettings:AzureStorageConnectionString", "DefaultEndpointsProtocol=https;AccountName=endteststorage;EndpointSuffix=core.windows.net" },
            };
            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", true, true)
                .Build();
        }

        public TenancyContainerScenarioBindings(
            FeatureContext featureContext,
            ScenarioContext scenarioContext)
        {
            this.featureContext = featureContext;
            this.scenarioContext = scenarioContext;
        }

        public static IConfiguration Configuration { get; }

        public RootTenant RootTenant => this.rootTenant ?? throw new InvalidOperationException("Tenant has not been set up yet");

        // These bindings are used partly by tests that use per-feature and partly per-scenario setup.
        // (We need the tenancy store ones to be per-feature because the tear down runs into Azure Storage
        // delete-then-recreate-too-quickly problems).
        private IServiceProvider ServiceProvider => this.featureContext.FeatureInfo.Tags.Any(t => t == "perFeatureContainer")
            ? ContainerBindings.GetServiceProvider(this.featureContext)
            : ContainerBindings.GetServiceProvider(this.scenarioContext);

        [BeforeScenario("@perScenarioContainer", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection)]
        public void InitializeCommonTenancyServices()
        {
            ContainerBindings.ConfigureServices(
                   this.scenarioContext,
                   serviceCollection => CommonServiceInit(this.featureContext, serviceCollection));
        }

        [BeforeFeature("@perFeatureContainer", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection)]
        public static void InitializeCommonTenancyServices(FeatureContext featureContext)
        {
            ContainerBindings.ConfigureServices(
                featureContext,
                serviceCollection => CommonServiceInit(featureContext, serviceCollection));
        }

        [BeforeScenario("@perScenarioContainer", Order = ContainerBeforeScenarioOrder.ServiceProviderAvailable)]
        public void GetServicesForScenarioLevelContainer()
        {
            ITenantProvider tenantProvider = this.ServiceProvider.GetRequiredService<ITenantProvider>();

            this.rootTenant = tenantProvider.Root;
        }

        [BeforeScenario("@perFeatureContainer", Order = ContainerBeforeScenarioOrder.ServiceProviderAvailable)]
        public void GetServicesForFeatureLevelContainer()
        {
            ITenantProvider tenantProvider = this.ServiceProvider.GetRequiredService<ITenantProvider>();

            this.rootTenant = tenantProvider.Root;
        }

        /// <summary>
        /// Cleans up the tenant provider blob store by removing all the tenants created during the test run.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [AfterScenario("@withBlobStorageTenantProvider")]
        public async Task CleanUpTenantStore()
        {
            if (!this.featureContext.FeatureInfo.Tags.Any(t => t == "perFeatureContainer"))
            {
                await this.scenarioContext.RunAndStoreExceptionsAsync(() => CoreCleanupAsync(this.ServiceProvider, this.RootTenant))
                    .ConfigureAwait(false);
            }
        }

        [AfterFeature("@withBlobStorageTenantProvider")]
        public static async Task CleanUpTenantStore(FeatureContext featureContext)
        {
            if (featureContext.FeatureInfo.Tags.Any(t => t == "perFeatureContainer"))
            {
                IServiceProvider sp = ContainerBindings.GetServiceProvider(featureContext);
                ITenantProvider p = sp.GetRequiredService<ITenantProvider>();
                await featureContext.RunAndStoreExceptionsAsync(() => CoreCleanupAsync(sp, p.Root))
                    .ConfigureAwait(false);
            }
        }

        private static async Task CoreCleanupAsync(IServiceProvider sp, ITenant rootTenant)
        {
            TenantTrackingTenantProviderDecorator tenantTrackingProvider = sp.GetRequiredService<TenantTrackingTenantProviderDecorator>();
            List<ITenant> tenants = tenantTrackingProvider.CreatedTenants;
            tenants.Add(tenantTrackingProvider.Root);
            ITenantCloudBlobContainerFactory blobContainerFactory = sp.GetRequiredService<ITenantCloudBlobContainerFactory>();

            CloudBlobContainer rootContainer = await blobContainerFactory.GetBlobContainerForTenantAsync(
                rootTenant, TenantProviderBlobStore.ContainerDefinition).ConfigureAwait(false);

            IEnumerable<CloudBlobContainer> blobContainers =
                tenants.Select(tenant =>
                {
                    // It would be easier just to ask blobContainerFactory.GetBlobContainerForTenantAsync to give
                    // us the container, but that will attempt to create it if it doesn't exist, and in tests
                    // where we happen already to have deleted it, that quick recreation test will then fail
                    // because Azure doesn't like it if you do taht.
                    string tenantedContainerName = $"{tenant.Id.ToLowerInvariant()}-{TenantProviderBlobStore.ContainerDefinition.ContainerName}";
                    string containerName = AzureStorageNameHelper.HashAndEncodeBlobContainerName(tenantedContainerName);
                    return rootContainer.ServiceClient.GetContainerReference(containerName);
                });

            foreach (CloudBlobContainer container in blobContainers.Distinct(x => x.Name))
            {
                await container.DeleteIfExistsAsync().ConfigureAwait(false);
            }
        }

        private static void CommonServiceInit(
            FeatureContext featureContext,
            IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(Configuration);
            serviceCollection.AddSingleton(Configuration.GetSection("TestSettings").Get<TestSettings>() ?? new TestSettings());

            serviceCollection.AddRequiredTenancyServices();

            if (featureContext.FeatureInfo.Tags.Any(t => t == "withBlobStorageTenantProvider"))
            {
                serviceCollection.AddTenantProviderBlobStore(_ =>
                {
                    var blobStorageConfiguration = new BlobStorageConfiguration();
                    Configuration.Bind("TENANCYBLOBSTORAGECONFIGURATIONOPTIONS", blobStorageConfiguration);
                    return blobStorageConfiguration;
                });

                // Now replace the service for ITenantStore and ITenantProvider with a decorated TenantProviderBlobStore.
                serviceCollection.AddSingleton(sp => new TenantTrackingTenantProviderDecorator(
                        sp.GetRequiredService<TenantProviderBlobStore>()));
                serviceCollection.Remove(serviceCollection.First(x => x.ServiceType == typeof(ITenantProvider)));
                serviceCollection.AddSingleton<ITenantProvider>(
                    sp => sp.GetRequiredService<TenantTrackingTenantProviderDecorator>());
                serviceCollection.AddSingleton<ITenantStore>(
                    sp => sp.GetRequiredService<TenantTrackingTenantProviderDecorator>());
            }
            else
            {
                serviceCollection.AddSingleton<FakeTenantProvider>();
                serviceCollection.AddSingleton<ITenantProvider>(sp => sp.GetRequiredService<FakeTenantProvider>());
            }

            ////serviceCollection.AddServiceIdentityAzureTokenCredentialSourceFromLegacyConnectionString(Configuration["AzureServicesAuthConnectionString"] ?? throw new InvalidOperationException("AzureServicesAuthConnectionString configuration setting required"));
        }
    }
}