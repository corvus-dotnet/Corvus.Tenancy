// <copyright file="LegacyTenancyCosmosContainerBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Corvus.Azure.Cosmos.Tenancy;
    using Corvus.Testing.ReqnRoll;

    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.DependencyInjection;

    using Reqnroll;

    /// <summary>
    /// Reqnroll bindings to support a tenanted cloud blob container.
    /// </summary>
    [Binding]
    public class LegacyTenancyCosmosContainerBindings
    {
        private readonly ScenarioContext scenarioContext;
        private readonly List<Container> containersToRemoveAtTeardown = [];
        private ITenantCosmosContainerFactory? tenantCosmosContainerFactory;

        public LegacyTenancyCosmosContainerBindings(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        public ITenantCosmosContainerFactory TenantCosmosContainerFactory => this.tenantCosmosContainerFactory ?? throw new InvalidOperationException("Factory has not been set up yet");

        public void RemoveThisContainerOnTestTeardown(Container container)
        {
            this.containersToRemoveAtTeardown.Add(container);
        }

        /// <summary>
        /// Initializes the container before each scenario runs.
        /// </summary>
        [BeforeScenario("@setupLegacyTenantedCosmosContainer", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection + 1)]
        public void InitializeContainer()
        {
            ContainerBindings.ConfigureServices(
                   this.scenarioContext,
                   serviceCollection =>
                   {
                       var cosmosOptions = new TenantCosmosContainerFactoryOptions
                       {
                           AzureServicesAuthConnectionString = TenancyContainerScenarioBindings.Configuration["AzureServicesAuthConnectionString"],
                       };

                       serviceCollection.AddJsonNetSerializerSettingsProvider();
                       serviceCollection.AddTenantCosmosContainerFactory(cosmosOptions);
                       serviceCollection.AddCosmosClientBuilderWithNewtonsoftJsonIntegration();
                   });
        }

        /// <summary>
        /// Gets services from DI required during testing..
        /// </summary>
        [BeforeScenario("@setupLegacyTenantedCosmosContainer", Order = ContainerBeforeScenarioOrder.ServiceProviderAvailable)]
        public void GetServices()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.scenarioContext);
            this.tenantCosmosContainerFactory = serviceProvider.GetRequiredService<ITenantCosmosContainerFactory>();
        }

        /// <summary>
        /// Tear down the tenanted Cloud Blob Container for the scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterScenario("@setupLegacyTenantedCosmosContainer", Order = 100000)]
        public Task TeardownCosmosDB()
        {
            return this.scenarioContext.RunAndStoreExceptionsAsync(
                async () =>
                {
                    foreach (Container container in this.containersToRemoveAtTeardown)
                    {
                        await container.DeleteContainerAsync().ConfigureAwait(false);
                    }
                });
        }
    }
}