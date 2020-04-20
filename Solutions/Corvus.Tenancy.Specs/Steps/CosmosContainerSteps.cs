namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using Corvus.Azure.Cosmos.Tenancy;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Tenancy.Specs.Bindings;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class CosmosContainerSteps
    {
        private readonly FeatureContext featureContext;

        public CosmosContainerSteps(FeatureContext featureContext)
        {
            this.featureContext = featureContext;
        }

        [Then("I should be able to get the tenanted cosmos container")]
        public void ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            Container cosmosContainer = this.featureContext.Get<Container>(TenancyCosmosContainerBindings.TenancySpecsContainer);
            Assert.IsNotNull(cosmosContainer);
        }

        [When("I remove the Cosmos configuration from the tenant")]
        public void WhenIRemoveTheCosmosConfigurationFromTheTenant()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            CosmosContainerDefinition definition = this.featureContext.Get<CosmosContainerDefinition>();
            tenantProvider.Root.ClearCosmosConfiguration(definition);
        }

        [Then("attempting to get the Cosmos configuration from the tenant throws an ArgumentException")]
        public void ThenTheCosmosConfigurationOnTheTenantIsSetToNull()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            CosmosContainerDefinition definition = this.featureContext.Get<CosmosContainerDefinition>();

            try
            {
                tenantProvider.Root.GetCosmosConfiguration(definition);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}
