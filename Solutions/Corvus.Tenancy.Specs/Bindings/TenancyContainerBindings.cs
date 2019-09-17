// <copyright file="ClaimsContainerBindings.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System.Collections.Generic;
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
                        .AddJsonFile("local.settings.json", true, true)
                        .AddEnvironmentVariables()
                        .Build();
                    serviceCollection.AddSingleton(config);
                    serviceCollection.AddSingleton<ITenantProvider, FakeTenantProvider>();
                    serviceCollection.AddTenantCloudBlobContainerFactory(config);
                });
        }
    }
}