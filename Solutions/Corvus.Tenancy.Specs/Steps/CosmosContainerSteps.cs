namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using Corvus.Azure.Cosmos.Tenancy;
    using Corvus.Tenancy.Specs.Bindings;
    using Corvus.Testing.SpecFlow;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class CosmosContainerSteps
    {
        private readonly FeatureContext featureContext;
        private readonly string storageContextName;

        public CosmosContainerSteps(
            FeatureContext featureContext,
            StorageContextTestContext storageContextTestContext)
        {
            this.featureContext = featureContext;
            this.storageContextName = storageContextTestContext.ContextName;
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
            tenantProvider.Root.UpdateProperties(
                propertiesToRemove: CosmosStorageTenantExtensions.RemoveCosmosConfiguration(this.storageContextName!));
        }

        [Then("attempting to get the Cosmos configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheCosmosConfigurationOnTheTenantThrowsArgumentException()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();

            try
            {
                tenantProvider.Root.GetCosmosConfiguration(this.storageContextName!);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}
