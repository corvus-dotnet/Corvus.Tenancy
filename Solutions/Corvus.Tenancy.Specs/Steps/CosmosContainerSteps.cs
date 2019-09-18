namespace Corvus.Tenancy.Specs.Steps
{
    using Corvus.Tenancy.Specs.Bindings;
    using Microsoft.Azure.Cosmos;
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

        [Then(@"I should be able to get the tenanted cosmos container")]
        public void ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            Container cosmosContainer = this.featureContext.Get<Container>(TenancyCosmosContainerBindings.TenancySpecsContainer);
            Assert.IsNotNull(cosmosContainer);
        }
    }
}
