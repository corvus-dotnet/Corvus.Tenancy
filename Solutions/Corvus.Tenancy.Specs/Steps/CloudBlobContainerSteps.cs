namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Tenancy.Specs.Bindings;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class CloudBlobContainerSteps
    {
        private readonly FeatureContext featureContext;

        public CloudBlobContainerSteps(FeatureContext featureContext)
        {
            this.featureContext = featureContext;
        }

        [Then("I should be able to get the tenanted cloud blob container")]
        public void ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            CloudBlobContainer cloudBlobContainer = this.featureContext.Get<CloudBlobContainer>(TenancyCloudBlobContainerBindings.TenancySpecsContainer);
            Assert.IsNotNull(cloudBlobContainer);
        }

        [When("I remove the blob storage configuration from the tenant")]
        public void WhenIRemoveTheBlobStorageConfigurationFromTheTenant()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            BlobStorageContainerDefinition definition = this.featureContext.Get<BlobStorageContainerDefinition>();
            tenantProvider.Root.UpdateProperties(
                propertiesToRemove: definition.RemoveBlobStorageConfiguration());
        }

        [Then("attempting to get the blob storage configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheBlobStorageConfigurationOnTheTenantThrowsArgumentException()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            BlobStorageContainerDefinition definition = this.featureContext.Get<BlobStorageContainerDefinition>();

            try
            {
                tenantProvider.Root.GetBlobStorageConfiguration(definition);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}
