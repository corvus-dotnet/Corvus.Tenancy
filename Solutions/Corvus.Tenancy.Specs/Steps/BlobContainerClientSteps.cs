namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Threading.Tasks;

    using global::Azure.Storage.Blobs;

    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.Tenancy.Specs.Bindings;
    using Corvus.Testing.SpecFlow;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class BlobContainerClientSteps
    {
        private readonly FeatureContext featureContext;
        private readonly IServiceProvider serviceProvider;

        private string? blobStorageContextName;

        public BlobContainerClientSteps(FeatureContext featureContext)
        {
            this.featureContext = featureContext;
            this.serviceProvider = ContainerBindings.GetServiceProvider(featureContext);
        }

        [Given(@"I have added blob storage configuration to the current tenant with a table name of '(.*)'")]
        public void GivenIHaveAddedBlobStorageConfigurationToTheCurrentTenantWithATableNameOf(string tableName)
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            IConfigurationRoot config = this.serviceProvider.GetRequiredService<IConfigurationRoot>();

            string containerBase = Guid.NewGuid().ToString();

            this.blobStorageContextName = $"tenancyspecs{Guid.NewGuid()}";

            var blobStorageConfiguration = new BlobStorageConfiguration
            {
                Container = tableName,
            };

            tenantProvider.Root.UpdateProperties(values => values.AddBlobStorageConfiguration(this.blobStorageContextName, blobStorageConfiguration));
        }

        [Then("I should be able to get the tenanted cloud blob container")]
        public async Task ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            ITenantBlobContainerClientFactory factory = this.serviceProvider.GetRequiredService<ITenantBlobContainerClientFactory>();

            BlobContainerClient tenancySpecsContainer = await factory.GetContextForTenantAsync(
                tenantProvider.Root,
                this.blobStorageContextName!).ConfigureAwait(false);

            Assert.IsNotNull(tenancySpecsContainer);

            // Add to feature context so it will be torn down after the test.
            this.featureContext.Set(tenancySpecsContainer, TenancyBlobContainerClientBindings.TenancySpecsContainer);
        }

        [Then("the tenanted cloud blob container should be named using a hash of the tenant Id and the name specified in the blob configuration")]
        public void ThenTheTenantedBlobContainerClientShouldBeNamedUsingAHashOfTheTenantIdAndTheNameSpecifiedInTheBlobConfiguration()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            BlobStorageConfiguration blobStorageConfiguration = tenantProvider.Root.GetBlobStorageConfiguration(this.blobStorageContextName!);

            string expectedNamePlain = string.Concat(RootTenant.RootTenantId, "-", blobStorageConfiguration.Container);
            string expectedName = AzureStorageNameHelper.HashAndEncodeBlobContainerName(expectedNamePlain);

            BlobContainerClient container = this.featureContext.Get<BlobContainerClient>(TenancyBlobContainerClientBindings.TenancySpecsContainer);

            Assert.AreEqual(expectedName, container.Name);
        }

        [Then("the tenanted cloud blob container should be named using a hash of the name specified in the blob configuration")]
        public void ThenTheTenantedBlobContainerClientShouldBeNamedUsingAHashOfTheNameSpecifiedInTheBlobConfiguration()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            BlobStorageConfiguration blobStorageConfiguration = tenantProvider.Root.GetBlobStorageConfiguration(this.blobStorageContextName!);

            string expectedNamePlain = blobStorageConfiguration.Container!;
            string expectedName = AzureStorageNameHelper.HashAndEncodeBlobContainerName(expectedNamePlain);

            BlobContainerClient container = this.featureContext.Get<BlobContainerClient>(TenancyBlobContainerClientBindings.TenancySpecsContainer);

            Assert.AreEqual(expectedName, container.Name);
        }

        [When("I remove the blob storage configuration from the tenant")]
        public void WhenIRemoveTheBlobStorageConfigurationFromTheTenant()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            tenantProvider.Root.UpdateProperties(
                propertiesToRemove: BlobStorageTenantExtensions.RemoveBlobStorageConfiguration(this.blobStorageContextName!));
        }

        [Then("attempting to get the blob storage configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheBlobStorageConfigurationOnTheTenantThrowsArgumentException()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();

            try
            {
                tenantProvider.Root.GetBlobStorageConfiguration(this.blobStorageContextName!);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}
