// <copyright file="TenancyCosmosContainerBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Corvus.Storage.Azure.Cosmos;
    using Corvus.Storage.Azure.Cosmos.Tenancy;
    using Corvus.Testing.SpecFlow;

    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using TechTalk.SpecFlow;

    [Binding]
    public class TenancyCosmosContainerBindings
    {
        private readonly ScenarioContext scenarioContext;
        private readonly List<Database> databasesToRemoveAtTeardown = new ();
        private ICosmosContainerSourceFromDynamicConfiguration? containerSource;

        public TenancyCosmosContainerBindings(
            ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;

            IConfiguration configuration = TenancyContainerScenarioBindings.Configuration;
            this.TestCosmosConfiguration = configuration
                .GetSection("TestCosmosConfigurationOptions")
                .Get<CosmosContainerConfiguration>();
            this.TestLegacyCosmosConfiguration = configuration
                .GetSection("TestLegacyCosmosConfigurationOptions")
                .Get<LegacyV2CosmosContainerConfiguration>();
        }

        public CosmosContainerConfiguration TestCosmosConfiguration { get; }

        public LegacyV2CosmosContainerConfiguration TestLegacyCosmosConfiguration { get; }

        public ICosmosContainerSourceFromDynamicConfiguration ContainerSource => this.containerSource ?? throw new InvalidOperationException("Container source not initialized yet");

        public void RemoveThisDatabaseOnTestTeardown(Database container)
        {
            // We get duplicates because this called both when creating a DB up front, and
            // when fetching a container.
            if (!this.databasesToRemoveAtTeardown.Any(c => c.Id == container.Id))
            {
                this.databasesToRemoveAtTeardown.Add(container);
            }
        }

        /// <summary>
        /// Initializes the container before each scenario runs.
        /// </summary>
        [BeforeScenario("@setupTenantedCosmosContainer", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection)]
        public void InitializeContainer()
        {
            ContainerBindings.ConfigureServices(
                   this.scenarioContext,
                   serviceCollection =>
                   {
                       serviceCollection.AddTenantCosmosConnectionFactory();
                       serviceCollection.AddCosmosContainerV2ToV3Transition();
                   });
        }

        /// <summary>
        /// Gets services from DI required during testing..
        /// </summary>
        [BeforeScenario("@setupTenantedCosmosContainer", Order = ContainerBeforeScenarioOrder.ServiceProviderAvailable)]
        public void GetServices()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.scenarioContext);
            this.containerSource = serviceProvider.GetRequiredService<ICosmosContainerSourceFromDynamicConfiguration>();
        }

        /// <summary>
        /// Tear down the tenanted Azure Container for the scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterScenario("@setupTenantedCosmosContainer", Order = 100000)]
        public Task TeardownCosmosDB()
        {
            return this.scenarioContext.RunAndStoreExceptionsAsync(
                async () =>
                {
                    foreach (Database database in this.databasesToRemoveAtTeardown)
                    {
                        await database.DeleteAsync().ConfigureAwait(false);
                    }
                });
        }
    }
}