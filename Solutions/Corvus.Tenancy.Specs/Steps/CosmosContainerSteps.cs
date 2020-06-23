namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Threading.Tasks;
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
        private readonly ScenarioContext scenarioContext;

        public CosmosContainerSteps(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            this.featureContext = featureContext;
            this.scenarioContext = scenarioContext;
        }

        [Given(@"I get the recreatable tenanted cosmos container as ""(.*)""")]
        public void GivenIGetTheRecreatableTenantedComsosContainerAs(string propertyName)
        {
            RecreatableContainer cosmosContainer = this.featureContext.Get<RecreatableContainer>(TenancyCosmosContainerBindings.TenancySpecsContainer);
            this.scenarioContext.Set(cosmosContainer.Instance, propertyName);
        }

        [When(@"I recreate the cosmos container as ""(.*)""")]
        public async Task WhenIRecreateTheCosmosContainerAs(string newName)
        {
            RecreatableContainer cosmosContainer = this.featureContext.Get<RecreatableContainer>(TenancyCosmosContainerBindings.TenancySpecsContainer);
            await cosmosContainer.RecreateContainer().ConfigureAwait(false);
            this.scenarioContext.Set(cosmosContainer.Instance, newName);
        }

        [Then(@"the ""(.*)"" container should not be null")]
        public void ThenTheContainerShouldNotBeNull(string name)
        {
            Assert.IsNotNull(this.scenarioContext.Get<Container>(name));
        }

        [Then(@"the ""(.*)"" container should not be the same as the ""(.*)"" container")]
        public void TheContainerShouldNotEqualTheContainer(string first, string second)
        {
            Container firstContainer = this.scenarioContext.Get<Container>(first);
            Container secondContainer = this.scenarioContext.Get<Container>(second);
            Assert.IsFalse(ReferenceEquals(firstContainer, secondContainer));
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
            tenantProvider.Root.UpdateProperties(
                propertiesToRemove: definition.RemoveCosmosConfiguration());
        }

        [Then("attempting to get the Cosmos configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheCosmosConfigurationOnTheTenantThrowsArgumentException()
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
