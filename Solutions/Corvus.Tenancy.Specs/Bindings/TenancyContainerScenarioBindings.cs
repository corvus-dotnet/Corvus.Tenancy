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
    using Corvus.Testing.SpecFlow;

    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using TechTalk.SpecFlow;

    [Binding]
    public class TenancyContainerScenarioBindings
    {
        private readonly FeatureContext featureContext;
        private readonly ScenarioContext scenarioContext;
        private RootTenant? rootTenant;

        public TenancyContainerScenarioBindings(
            FeatureContext featureContext,
            ScenarioContext scenarioContext)
        {
            this.featureContext = featureContext;
            this.scenarioContext = scenarioContext;

            var configData = new Dictionary<string, string>
            {
                //// Add configuration value pairs here
                ////{ "STORAGEACCOUNTCONNECTIONSTRING", "UseDevelopmentStorage=true" },
            };
            this.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", true, true)
                .Build();
        }

        public IConfiguration Configuration { get; }

        public RootTenant RootTenant => this.rootTenant ?? throw new InvalidOperationException("Tenant has not been set up yet");

        [BeforeScenario("@perScenarioContainer", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection)]
        public void InitializeCommonTenancyServices()
        {
            ContainerBindings.ConfigureServices(
                   this.scenarioContext,
                   serviceCollection =>
                   {
                       serviceCollection.AddSingleton(this.Configuration);
                       serviceCollection.AddSingleton(this.Configuration.GetSection("TestSettings").Get<TestSettings>());

                       serviceCollection.AddRequiredTenancyServices();

                       if (this.featureContext.FeatureInfo.Tags.Any(t => t == "withBlobStorageTenantProvider"))
                       {
                           serviceCollection.AddTenantProviderBlobStore(_ =>
                           {
                               var blobStorageConfiguration = new BlobStorageConfiguration();
                               this.Configuration.Bind("TENANCYBLOBSTORAGECONFIGURATIONOPTIONS", blobStorageConfiguration);
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

                       serviceCollection.AddServiceIdentityAzureTokenCredentialSourceFromLegacyConnectionString(this.Configuration["AzureServicesAuthConnectionString"]);
                   });
        }

        [BeforeScenario("@perScenarioContainer", Order = ContainerBeforeScenarioOrder.ServiceProviderAvailable)]
        public void GetServices()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.scenarioContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();

            this.rootTenant = tenantProvider.Root;
        }

        /// <summary>
        /// Cleans up the tenant provider blob store by removing all the tenants created during the test run.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [AfterScenario("@withBlobStorageTenantProvider")]
        public Task CleanUpTenantStore()
        {
            return this.scenarioContext.RunAndStoreExceptionsAsync(async () =>
            {
                IServiceProvider sp = ContainerBindings.GetServiceProvider(this.scenarioContext);
                TenantTrackingTenantProviderDecorator tenantTrackingProvider = sp.GetRequiredService<TenantTrackingTenantProviderDecorator>();
                List<ITenant> tenants = tenantTrackingProvider.CreatedTenants;
                tenants.Add(tenantTrackingProvider.Root);
                ITenantCloudBlobContainerFactory blobContainerFactory = sp.GetRequiredService<ITenantCloudBlobContainerFactory>();

                CloudBlobContainer rootContainer = await blobContainerFactory.GetBlobContainerForTenantAsync(
                    this.RootTenant, TenantProviderBlobStore.ContainerDefinition).ConfigureAwait(false);

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
            });
        }
    }
}