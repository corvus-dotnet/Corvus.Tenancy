namespace Corvus.Tenancy.Specs.Steps
{
    using Corvus.Tenancy.Specs.Bindings;
    using Gremlin.Net.Driver;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class GremlinContainerSteps
    {
        private readonly FeatureContext featureContext;

        public GremlinContainerSteps(FeatureContext featureContext)
        {
            this.featureContext = featureContext;
        }

        [Then("I should be able to get the tenanted gremlin client")]
        public void ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            GremlinClient cosmosClient = this.featureContext.Get<GremlinClient>(TenancyGremlinContainerBindings.TenancySpecsContainer);
            Assert.IsNotNull(cosmosClient);
        }
    }
}
