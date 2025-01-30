// <copyright file="TenancySqlBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;

    using Corvus.Storage.Sql;
    using Corvus.Testing.ReqnRoll;

    using Microsoft.Extensions.DependencyInjection;

    using Reqnroll;

    /// <summary>
    /// Reqnroll bindings to support a tenanted SQL container.
    /// </summary>
    [Binding]
    public class TenancySqlBindings
    {
        private readonly ScenarioContext scenarioContext;
        private ISqlConnectionFromDynamicConfiguration? connectionSource;

        public TenancySqlBindings(
            ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        public ISqlConnectionFromDynamicConfiguration ConnectionSource => this.connectionSource ?? throw new InvalidOperationException("Container source not initialized yet");

        /// <summary>
        /// Initializes the container before each scenario runs.
        /// </summary>
        [BeforeScenario("@setupTenantedSqlConnection", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection + 1)]
        public void InitializeContainer()
        {
            ContainerBindings.ConfigureServices(
                   this.scenarioContext,
                   serviceCollection => serviceCollection.AddTenantSqlConnectionFactory());
        }

        /// <summary>
        /// Gets services from DI required during testing..
        /// </summary>
        [BeforeScenario("@setupTenantedSqlConnection", Order = ContainerBeforeScenarioOrder.ServiceProviderAvailable)]
        public void GetServices()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.scenarioContext);
            this.connectionSource = serviceProvider.GetRequiredService<ISqlConnectionFromDynamicConfiguration>();
        }
    }
}