// <copyright file="TenancyContainerBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.Azure.Cosmos.Tenancy;
    using Corvus.Azure.GremlinExtensions.Tenancy;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Sql.Tenancy;
    using Corvus.Testing.SpecFlow;
    using Corvus.Tenancy;
    using Corvus.Tenancy.Internal;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;
    using Corvus.Storage.Azure.BlobStorage.Tenancy;

    /// <summary>
    ///     Container related bindings to configure the service provider for features.
    /// </summary>
    [Binding]
    public static class TenancyContainerBindings
    {
        /// <summary>
        /// Initializes the container before each feature's tests are run.
        /// </summary>
        /// <param name="featureContext">The SpecFlow test context.</param>
        [BeforeFeature("@perFeatureContainer", Order = ContainerBeforeFeatureOrder.PopulateServiceCollection)]
        public static void InitializeContainer(FeatureContext featureContext)
        {
            ContainerBindings.ConfigureServices(
                featureContext,
                serviceCollection =>
                {
                    var configData = new Dictionary<string, string>
                    {
                        //// Add configuration value pairs here
                        ////{ "STORAGEACCOUNTCONNECTIONSTRING", "UseDevelopmentStorage=true" },
                    };
                    IConfigurationRoot config = new ConfigurationBuilder()
                        .AddInMemoryCollection(configData)
                        .AddEnvironmentVariables()
                        .AddJsonFile("local.settings.json", true, true)
                        .Build();

                    serviceCollection.AddSingleton(config);
                    serviceCollection.AddSingleton(config.GetSection("TestSettings").Get<TestSettings>());

                    serviceCollection.AddRequiredTenancyServices();

                    if (featureContext.FeatureInfo.Tags.Any(t => t == "withBlobStorageTenantProvider"))
                    {
                        serviceCollection.AddTenantProviderBlobStore(_ =>
                        {
                            var blobStorageConfiguration = new BlobStorageConfiguration();
                            config.Bind("TENANCYBLOBSTORAGECONFIGURATIONOPTIONS", blobStorageConfiguration);
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

                    // TODO: is this the right place to do this?
                    serviceCollection.AddAzureBlobStorageClient();
                    serviceCollection.AddBlobContainerV2ToV3Transition();

                    var blobOptions = new TenantCloudBlobContainerFactoryOptions
                    {
                        AzureServicesAuthConnectionString = config["AzureServicesAuthConnectionString"],
                    };

                    serviceCollection.AddTenantCloudBlobContainerFactory(blobOptions);

                    var tableOptions = new TenantCloudTableFactoryOptions
                    {
                        AzureServicesAuthConnectionString = config["AzureServicesAuthConnectionString"],
                    };

                    serviceCollection.AddTenantCloudTableFactory(tableOptions);

                    var cosmosOptions = new TenantCosmosContainerFactoryOptions
                    {
                        AzureServicesAuthConnectionString = config["AzureServicesAuthConnectionString"],
                    };

                    serviceCollection.AddTenantCosmosContainerFactory(cosmosOptions);

                    var gremlinOptions = new TenantGremlinContainerFactoryOptions
                    {
                        AzureServicesAuthConnectionString = config["AzureServicesAuthConnectionString"],
                    };

                    serviceCollection.AddTenantGremlinContainerFactory(gremlinOptions);

                    var sqlOptions = new TenantSqlConnectionFactoryOptions
                    {
                        AzureServicesAuthConnectionString = config["AzureServicesAuthConnectionString"],
                    };

                    serviceCollection.AddTenantSqlConnectionFactory(sqlOptions);
                });
        }

        /// <summary>
        /// Cleans up the tenant provider blob store by removing all the tenants created during the test run.
        /// </summary>
        /// <param name="featureContext">The SpecFlow test context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [AfterFeature("@withBlobStorageTenantProvider")]
        public static Task CleanUpTenantStore(FeatureContext featureContext)
        {
            return featureContext.RunAndStoreExceptionsAsync(async () =>
            {
                IServiceProvider sp = ContainerBindings.GetServiceProvider(featureContext);
                TenantTrackingTenantProviderDecorator tenantTrackingProvider = sp.GetRequiredService<TenantTrackingTenantProviderDecorator>();
                List<ITenant> tenants = tenantTrackingProvider.CreatedTenants;
                tenants.Add(tenantTrackingProvider.Root);
                ITenantCloudBlobContainerFactory blobContainerFactory = sp.GetRequiredService<ITenantCloudBlobContainerFactory>();

                CloudBlobContainer[] blobContainers = await Task.WhenAll(
                    tenants.Select(tenant => blobContainerFactory.GetBlobContainerForTenantAsync(
                        tenant,
                        TenantProviderBlobStore.ContainerDefinition))).ConfigureAwait(false);

                foreach (CloudBlobContainer container in blobContainers.Distinct(x => x.Name))
                {
                    await container.DeleteIfExistsAsync().ConfigureAwait(false);
                }
            });
        }
    }
}