// <copyright file="ClaimsContainerBindings.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System.Collections.Generic;
    using System.Linq;
    using Corvus.Azure.Cosmos.Tenancy;
    using Corvus.Azure.GremlinExtensions.Tenancy;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Sql.Tenancy;
    using Corvus.Tenancy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;

    /// <summary>
    ///     Container related bindings to configure the service provider for features.
    /// </summary>
    [Binding]
    public static class ClaimsContainerBindings
    {
        /// <summary>
        /// Initializes the container before each feature's tests are run.
        /// </summary>
        /// <param name="featureContext">The SpecFlow test context.</param>
        [BeforeFeature("@setupContainer", Order = ContainerBeforeFeatureOrder.PopulateServiceCollection)]
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

                    if (featureContext.FeatureInfo.Tags.Any(t => t == "withBlobStorageTenantProvider"))
                    {
                        serviceCollection.AddTenantProviderBlobStore();
                    }
                    else
                    {
                        serviceCollection.AddSingleton<ITenantProvider, FakeTenantProvider>();
                    }

                    var blobOptions = new TenantCloudBlobContainerFactoryOptions
                    {
                        AzureServicesAuthConnectionString = config["AzureServicesAuthConnectionString"]
                    };

                    config.Bind("ROOTTENANTBLOBSTORAGECONFIGURATIONOPTIONS", blobOptions.RootTenantBlobStorageConfiguration);

                    serviceCollection.AddTenantCloudBlobContainerFactory(blobOptions);

                    var cosmosOptions = new TenantCosmosContainerFactoryOptions
                    {
                        AzureServicesAuthConnectionString = config["AzureServicesAuthConnectionString"]
                    };

                    config.Bind("ROOTTENANTCOSMOSCONFIGURATIONOPTIONS", cosmosOptions.RootTenantCosmosConfiguration);

                    serviceCollection.AddTenantCosmosContainerFactory(cosmosOptions);

                    var gremlinOptions = new TenantGremlinContainerFactoryOptions
                    {
                        AzureServicesAuthConnectionString = config["AzureServicesAuthConnectionString"]
                    };

                    config.Bind("ROOTTENANTGREMLINCONFIGURATIONOPTIONS", gremlinOptions.RootTenantGremlinConfiguration);

                    serviceCollection.AddTenantGremlinContainerFactory(gremlinOptions);

                    var sqlOptions = new TenantSqlConnectionFactoryOptions
                    {
                        AzureServicesAuthConnectionString = config["AzureServicesAuthConnectionString"]
                    };

                    config.Bind("ROOTTENANTSQLCONFIGURATIONOPTIONS", sqlOptions.RootTenantSqlConfiguration);

                    serviceCollection.AddTenantSqlConnectionFactory(sqlOptions);
                });
        }
    }
}