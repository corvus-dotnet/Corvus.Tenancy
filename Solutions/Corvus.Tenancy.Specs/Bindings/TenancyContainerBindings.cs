// <copyright file="ClaimsContainerBindings.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System.Collections.Generic;
    using System.Linq;
    using Corvus.SpecFlow.Extensions;
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

                    serviceCollection.AddTenantCloudBlobContainerFactory(config);
                    serviceCollection.AddTenantCosmosContainerFactory(config);
                    serviceCollection.AddTenantSqlConnectionFactory(config);
                });
        }
    }
}