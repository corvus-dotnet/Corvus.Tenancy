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

        public BlobContainerClientSteps(FeatureContext featureContext)
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
            ITenantBlobContainerClientFactory factory = this.serviceProvider.GetRequiredService<ITenantBlobContainerClientFactory>();

            BlobStorageContainerDefinition blobStorageContainerDefinition = this.featureContext.Get<BlobStorageContainerDefinition>();

            BlobContainerClient tenancySpecsContainer = await factory.GetBlobContainerForTenantAsync(
                tenantProvider.Root,
                blobStorageContainerDefinition).ConfigureAwait(false);

            Assert.IsNotNull(tenancySpecsContainer);

            // Add to feature context so it will be torn down after the test.
            this.featureContext.Set(tenancySpecsContainer, TenancyBlobContainerClientBindings.TenancySpecsContainer);
        }

        [Then("the tenanted cloud blob container should be named using a hash of the tenant Id and the name specified in the blob container definition")]
        public void ThenTheTenantedBlobContainerClientShouldBeNamedUsingAHashOfTheTenantIdAndTheNameSpecifiedInTheBlobContainerDefinition()
        {
            BlobStorageContainerDefinition blobStorageContainerDefinition = this.featureContext.Get<BlobStorageContainerDefinition>();
            string expectedNamePlain = string.Concat(RootTenant.RootTenantId, "-", blobStorageContainerDefinition.ContainerName);
            string expectedName = AzureStorageNameHelper.HashAndEncodeBlobContainerName(expectedNamePlain);

            BlobContainerClient container = this.featureContext.Get<BlobContainerClient>(TenancyBlobContainerClientBindings.TenancySpecsContainer);

            Assert.AreEqual(expectedName, container.Name);
        }

        [Then("the tenanted cloud blob container should be named using a hash of the tenant Id and the name specified in the blob configuration")]
        public void ThenTheTenantedBlobContainerClientShouldBeNamedUsingAHashOfTheTenantIdAndTheNameSpecifiedInTheBlobConfiguration()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            BlobStorageContainerDefinition blobStorageContainerDefinition = this.featureContext.Get<BlobStorageContainerDefinition>();
            BlobStorageConfiguration blobStorageConfiguration = tenantProvider.Root.GetBlobStorageConfiguration(blobStorageContainerDefinition);

            string expectedNamePlain = string.Concat(RootTenant.RootTenantId, "-", blobStorageConfiguration.Container);
            string expectedName = AzureStorageNameHelper.HashAndEncodeBlobContainerName(expectedNamePlain);

            BlobContainerClient container = this.featureContext.Get<BlobContainerClient>(TenancyBlobContainerClientBindings.TenancySpecsContainer);

            Assert.AreEqual(expectedName, container.Name);
        }

        [Then("the tenanted cloud blob container should be named using a hash of the name specified in the blob configuration")]
        public void ThenTheTenantedBlobContainerClientShouldBeNamedUsingAHashOfTheNameSpecifiedInTheBlobConfiguration()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            BlobStorageContainerDefinition blobStorageContainerDefinition = this.featureContext.Get<BlobStorageContainerDefinition>();
            BlobStorageConfiguration blobStorageConfiguration = tenantProvider.Root.GetBlobStorageConfiguration(blobStorageContainerDefinition);

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
