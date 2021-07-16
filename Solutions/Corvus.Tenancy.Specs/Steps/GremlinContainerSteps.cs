namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using Corvus.Azure.GremlinExtensions.Tenancy;
    using Corvus.Tenancy.Specs.Bindings;
    using Corvus.Testing.SpecFlow;
    using Gremlin.Net.Driver;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class GremlinContainerSteps
    {
        private readonly FeatureContext featureContext;
        private readonly string storageContextName;

        public GremlinContainerSteps(
            FeatureContext featureContext,
            StorageContextTestContext storageContextTestContext)
        {
            this.featureContext = featureContext;
            this.storageContextName = storageContextTestContext.ContextName;
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
            tenantProvider.Root.UpdateProperties(
                propertiesToRemove: GremlinStorageTenantExtensions.RemoveGremlinConfiguration(this.storageContextName));
        }

        [Then("attempting to get the Gremlin configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheGremlinConfigurationOnTheTenantThrowsArgumentException()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();

            try
            {
                tenantProvider.Root.GetGremlinConfiguration(this.storageContextName);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}
