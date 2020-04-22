namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using Corvus.Azure.GremlinExtensions.Tenancy;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Tenancy.Specs.Bindings;
    using Gremlin.Net.Driver;
    using Microsoft.Extensions.DependencyInjection;
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
            GremlinClient cosmosClient = this.featureContext.Get<GremlinClient>(TenancyGremlinContainerBindings.TenancyGremlinClient);
            Assert.IsNotNull(cosmosClient);
        }

        [When("I remove the Gremlin configuration from the tenant")]
        public void WhenIRemoveTheGremlinConfigurationFromTheTenant()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            GremlinContainerDefinition definition = this.featureContext.Get<GremlinContainerDefinition>();
            tenantProvider.Root.UpdateProperties(
                propertiesToRemove: definition.RemoveGremlinConfiguration());
        }

        [Then("attempting to get the Gremlin configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheGremlinConfigurationOnTheTenantThrowsArgumentException()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            GremlinContainerDefinition definition = this.featureContext.Get<GremlinContainerDefinition>();

            try
            {
                tenantProvider.Root.GetGremlinConfiguration(definition);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}
