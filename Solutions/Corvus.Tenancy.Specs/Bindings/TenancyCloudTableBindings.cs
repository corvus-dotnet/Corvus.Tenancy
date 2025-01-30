// <copyright file="TenancyCloudTableBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Testing.ReqnRoll;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.DependencyInjection;

    using Reqnroll;

    /// <summary>
    /// Reqnroll bindings to support a tenanted cloud table container.
    /// </summary>
    [Binding]
    public class TenancyCloudTableBindings
    {
        private readonly ScenarioContext scenarioContext;
        private readonly List<CloudTable> tablesToRemoveAtTeardown = new();
        private ITenantCloudTableFactory? connectionFactory;

        public TenancyCloudTableBindings(
            ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        public ITenantCloudTableFactory ConnectionFactory => this.connectionFactory ?? throw new InvalidOperationException("Factory not initialized yet");

        public void RemoveThisTableOnTestTeardown(CloudTable container)
        {
            this.tablesToRemoveAtTeardown.Add(container);
        }

        /// <summary>
        /// Initializes the container before each scenario runs.
        /// </summary>
        [BeforeScenario("@setupTenantedCloudTable", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection + 1)]
        public void InitializeContainer()
        {
            ContainerBindings.ConfigureServices(
                   this.scenarioContext,
                   serviceCollection =>
                   {
                       var tableOptions = new TenantCloudTableFactoryOptions
                       {
                           AzureServicesAuthConnectionString = TenancyContainerScenarioBindings.Configuration["AzureServicesAuthConnectionString"],
                       };

                       serviceCollection.AddTenantCloudTableFactory(tableOptions);
                   });
        }

        /// <summary>
        /// Gets services from DI required during testing..
        /// </summary>
        [BeforeScenario("@setupTenantedCloudTable", Order = ContainerBeforeScenarioOrder.ServiceProviderAvailable)]
        public void GetServices()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.scenarioContext);
            this.connectionFactory = serviceProvider.GetRequiredService<ITenantCloudTableFactory>();
        }

        /// <summary>
        /// Tear down the tenanted Cloud Table Container for the feature.
        /// </summary>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterScenario("@setupTenantedCloudTable", Order = 100000)]
        public Task TeardownCosmosDB()
        {
            return this.scenarioContext.RunAndStoreExceptionsAsync(
                async () =>
                {
                    foreach (CloudTable table in this.tablesToRemoveAtTeardown)
                    {
                        await table.DeleteAsync().ConfigureAwait(false);
                    }
                });
        }
    }
}