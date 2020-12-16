namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Threading.Tasks;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Tenancy.Specs.Bindings;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class CloudBlobContainerSteps
    {
        private readonly FeatureContext featureContext;
        private readonly IServiceProvider serviceProvider;

        public CloudBlobContainerSteps(FeatureContext featureContext)
        {
            this.featureContext = featureContext;
            this.serviceProvider = ContainerBindings.GetServiceProvider(featureContext);
        }

        [Given("I have added blob storage configuration to the current tenant")]
        public void GivenIHaveAddedBlobStorageConfigurationToTheCurrentTenant(Table table)
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            IConfigurationRoot config = this.serviceProvider.GetRequiredService<IConfigurationRoot>();

            string containerBase = Guid.NewGuid().ToString();

            var blobStorageContainerDefinition = new BlobStorageContainerDefinition($"{containerBase}tenancyspecs");
            this.featureContext.Set(blobStorageContainerDefinition);

            var blobStorageConfiguration = new BlobStorageConfiguration();
            config.Bind("TESTBLOBSTORAGECONFIGURATIONOPTIONS", blobStorageConfiguration);

            string overriddenContainerName = table.Rows[0]["Container"];
            if (!string.IsNullOrEmpty(overriddenContainerName))
            {
                blobStorageConfiguration.Container = overriddenContainerName;
            }

            blobStorageConfiguration.DisableTenantIdPrefix = bool.Parse(table.Rows[0]["DisableTenantIdPrefix"]);

            tenantProvider.Root.UpdateProperties(values => values.AddBlobStorageConfiguration(blobStorageContainerDefinition, blobStorageConfiguration));
        }

        [Then("I should be able to get the tenanted cloud blob container")]
        public async Task ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            ITenantCloudBlobContainerFactory factory = this.serviceProvider.GetRequiredService<ITenantCloudBlobContainerFactory>();

            BlobStorageContainerDefinition blobStorageContainerDefinition = this.featureContext.Get<BlobStorageContainerDefinition>();

            CloudBlobContainer tenancySpecsContainer = await factory.GetBlobContainerForTenantAsync(
                tenantProvider.Root,
                blobStorageContainerDefinition).ConfigureAwait(false);

            Assert.IsNotNull(tenancySpecsContainer);

            // Add to feature context so it will be torn down after the test.
            this.featureContext.Set(tenancySpecsContainer, TenancyCloudBlobContainerBindings.TenancySpecsContainer);
        }

        [Then("the tenanted cloud blob container should be named using a hash of the tenant Id and the name specified in the blob container definition")]
        public void ThenTheTenantedCloudBlobContainerShouldBeNamedUsingAHashOfTheTenantIdAndTheNameSpecifiedInTheBlobContainerDefinition()
        {
            BlobStorageContainerDefinition blobStorageContainerDefinition = this.featureContext.Get<BlobStorageContainerDefinition>();
            string expectedNamePlain = string.Concat(RootTenant.RootTenantId, "-", blobStorageContainerDefinition.ContainerName);
            string expectedName = AzureStorageNameHelper.HashAndEncodeBlobContainerName(expectedNamePlain);

            CloudBlobContainer container = this.featureContext.Get<CloudBlobContainer>(TenancyCloudBlobContainerBindings.TenancySpecsContainer);

            Assert.AreEqual(expectedName, container.Name);
        }

        [Then("the tenanted cloud blob container should be named using a hash of the tenant Id and the name specified in the blob configuration")]
        public void ThenTheTenantedCloudBlobContainerShouldBeNamedUsingAHashOfTheTenantIdAndTheNameSpecifiedInTheBlobConfiguration()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            BlobStorageContainerDefinition blobStorageContainerDefinition = this.featureContext.Get<BlobStorageContainerDefinition>();
            BlobStorageConfiguration blobStorageConfiguration = tenantProvider.Root.GetBlobStorageConfiguration(blobStorageContainerDefinition);

            string expectedNamePlain = string.Concat(RootTenant.RootTenantId, "-", blobStorageConfiguration.Container);
            string expectedName = AzureStorageNameHelper.HashAndEncodeBlobContainerName(expectedNamePlain);

            CloudBlobContainer container = this.featureContext.Get<CloudBlobContainer>(TenancyCloudBlobContainerBindings.TenancySpecsContainer);

            Assert.AreEqual(expectedName, container.Name);
        }

        [Then("the tenanted cloud blob container should be named using a hash of the name specified in the blob configuration")]
        public void ThenTheTenantedCloudBlobContainerShouldBeNamedUsingAHashOfTheNameSpecifiedInTheBlobConfiguration()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            BlobStorageContainerDefinition blobStorageContainerDefinition = this.featureContext.Get<BlobStorageContainerDefinition>();
            BlobStorageConfiguration blobStorageConfiguration = tenantProvider.Root.GetBlobStorageConfiguration(blobStorageContainerDefinition);

            string expectedNamePlain = string.Concat(blobStorageConfiguration.Container);
            string expectedName = AzureStorageNameHelper.HashAndEncodeBlobContainerName(expectedNamePlain);

            CloudBlobContainer container = this.featureContext.Get<CloudBlobContainer>(TenancyCloudBlobContainerBindings.TenancySpecsContainer);

            Assert.AreEqual(expectedName, container.Name);
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
