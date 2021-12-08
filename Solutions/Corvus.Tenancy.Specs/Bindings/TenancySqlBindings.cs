// <copyright file="TenancySqlBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;

    using Corvus.Sql.Tenancy;
    using Corvus.Testing.SpecFlow;

    using Microsoft.Extensions.DependencyInjection;

    using TechTalk.SpecFlow;

    /// <summary>
    /// Specflow bindings to support a tenanted SQL container.
    /// </summary>
    [Binding]
    public class TenancySqlBindings
    {
        private readonly ScenarioContext scenarioContext;
        private readonly TenancyContainerScenarioBindings tenancyContainer;
        private ITenantSqlConnectionFactory? connectionFactory;

        public TenancySqlBindings(
            ScenarioContext scenarioContext,
            TenancyContainerScenarioBindings tenancyContainer)
        {
            this.scenarioContext = scenarioContext;
            this.tenancyContainer = tenancyContainer;
        }

        public ITenantSqlConnectionFactory ConnectionFactory => this.connectionFactory ?? throw new InvalidOperationException("Container source not initialized yet");

        /// <summary>
        /// Initializes the container before each scenario runs.
        /// </summary>
        [BeforeScenario("@setupTenantedSqlConnection", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection + 1)]
        public void InitializeContainer()
        {
            ContainerBindings.ConfigureServices(
                   this.scenarioContext,
                   serviceCollection =>
                   {
                       var sqlOptions = new TenantSqlConnectionFactoryOptions
                       {
                           AzureServicesAuthConnectionString = this.tenancyContainer.Configuration["AzureServicesAuthConnectionString"],
                       };

                       serviceCollection.AddTenantSqlConnectionFactory(sqlOptions);
                   });
        }

        /// <summary>
        /// Gets services from DI required during testing..
        /// </summary>
        [BeforeScenario("@setupTenantedSqlConnection", Order = ContainerBeforeScenarioOrder.ServiceProviderAvailable)]
        public void GetServices()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.scenarioContext);
            this.connectionFactory = serviceProvider.GetRequiredService<ITenantSqlConnectionFactory>();
        }
    }
}