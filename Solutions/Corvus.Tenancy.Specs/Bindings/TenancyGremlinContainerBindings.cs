// <copyright file="TenancyGremlinContainerBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Corvus.Azure.GremlinExtensions.Tenancy;
    using Corvus.Testing.ReqnRoll;

    using Gremlin.Net.Driver;

    using Microsoft.Extensions.DependencyInjection;

    using Reqnroll;

    /// <summary>
    /// Reqnroll bindings to support a tenanted cosmos gremlin container.
    /// </summary>
    [Binding]
    public class TenancyGremlinContainerBindings
    {
        private readonly ScenarioContext scenarioContext;
        private readonly List<GremlinClient> clientsToDisposeAtTeardown = [];
        private ITenantGremlinContainerFactory? containerFactory;

        public TenancyGremlinContainerBindings(
            ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        public ITenantGremlinContainerFactory ContainerFactory => this.containerFactory ?? throw new InvalidOperationException("Factory has not been set up yet");

        public void DisposeThisClientOnTestTeardown(GremlinClient container)
        {
            this.clientsToDisposeAtTeardown.Add(container);
        }

        /// <summary>
        /// Initializes the container before each scenario runs.
        /// </summary>
        [BeforeScenario("@setupTenantedGremlinClient", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection + 1)]
        public void InitializeContainer()
        {
            ContainerBindings.ConfigureServices(
                   this.scenarioContext,
                   serviceCollection =>
                   {
                       var gremlinOptions = new TenantGremlinContainerFactoryOptions
                       {
                           AzureServicesAuthConnectionString = TenancyContainerScenarioBindings.Configuration["AzureServicesAuthConnectionString"],
                       };

                       serviceCollection.AddTenantGremlinContainerFactory(gremlinOptions);
                   });
        }

        /// <summary>
        /// Set up a tenanted Gremlin Client for the scenario.
        /// </summary>
        /// <remarks>Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.</remarks>
        [BeforeScenario("@setupTenantedGremlinClient", Order = ContainerBeforeScenarioOrder.ServiceProviderAvailable)]
        public void GetServices()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.scenarioContext);
            this.containerFactory = serviceProvider.GetRequiredService<ITenantGremlinContainerFactory>();
        }

        /// <summary>
        /// Tear down the tenanted Gremlin Client for the scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterScenario("@setupTenantedGremlinClient", Order = 100000)]
        public async Task TeardownGremlinDB()
        {
            await this.scenarioContext.RunAndStoreExceptionsAsync(
                () =>
                {
                    // Note: the gremlin container factory doesn't currently create the container, so
                    // we don't currently have anything to delete.
                    foreach (GremlinClient client in this.clientsToDisposeAtTeardown)
                    {
                        client.Dispose();
                    }

                    return Task.CompletedTask;
                }).ConfigureAwait(false);
        }
    }
}