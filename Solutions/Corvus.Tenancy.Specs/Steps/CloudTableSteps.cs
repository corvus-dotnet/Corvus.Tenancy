namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Tenancy.Specs.Bindings;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class CloudTableSteps
    {
        private readonly FeatureContext featureContext;

        public CloudTableSteps(FeatureContext featureContext)
        {
            this.featureContext = featureContext;
        }

        [Then("I should be able to get the tenanted cloud table")]
        public void ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            CloudTable cloudTable = this.featureContext.Get<CloudTable>(TenancyCloudTableBindings.TenancySpecsContainer);
            Assert.IsNotNull(cloudTable);
        }

        [When("I remove the table storage configuration from the tenant")]
        public void WhenIRemoveTheTableStorageConfigurationFromTheTenant()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            TableStorageTableDefinition definition = this.featureContext.Get<TableStorageTableDefinition>();
            tenantProvider.Root.UpdateProperties(
                propertiesToRemove: definition.RemoveTableStorageConfiguration());
        }

        [Then("attempting to get the table storage configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheTableStorageConfigurationOnTheTenantThrowsArgumentException()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            TableStorageTableDefinition definition = this.featureContext.Get<TableStorageTableDefinition>();

            try
            {
                tenantProvider.Root.GetTableStorageConfiguration(definition);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}
