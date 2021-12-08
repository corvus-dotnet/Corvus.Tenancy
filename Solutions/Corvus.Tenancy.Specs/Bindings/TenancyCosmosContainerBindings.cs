namespace Corvus.Tenancy.Specs.Bindings
{
    using System;

    using Corvus.Storage.Azure.Cosmos;
    using Corvus.Testing.SpecFlow;

    using Microsoft.Extensions.DependencyInjection;

    using TechTalk.SpecFlow;

    [Binding]
    public class TenancyCosmosContainerBindings
    {
        private readonly ScenarioContext scenarioContext;
        private ICosmosContainerSourceFromDynamicConfiguration? containerSource;

        public TenancyCosmosContainerBindings(
            ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        public ICosmosContainerSourceFromDynamicConfiguration ContainerSource => this.containerSource ?? throw new InvalidOperationException("Container source not initialized yet");

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
                       serviceCollection.AddCosmosContainerSourceFromDynamicConfiguration();
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
    }
}