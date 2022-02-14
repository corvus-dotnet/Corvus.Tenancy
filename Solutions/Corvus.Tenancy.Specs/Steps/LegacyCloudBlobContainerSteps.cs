// <copyright file="LegacyCloudBlobContainerSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Threading.Tasks;

    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.Tenancy.Specs.Bindings;
    using Corvus.Testing.SpecFlow;

    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using TechTalk.SpecFlow;

    [Binding]
    public class LegacyCloudBlobContainerSteps
    {
        private readonly TenancyContainerScenarioBindings tenancyBindings;
        private readonly LegacyTenancyCloudBlobContainerBindings cloudBlobContainerBindings;
        private readonly IServiceProvider serviceProvider;
        private readonly BlobStorageContainerDefinition blobStorageContainerDefinition;
        private CloudBlobContainer? container;

        public LegacyCloudBlobContainerSteps(
            ScenarioContext scenarioContext,
            TenancyContainerScenarioBindings tenancyBindings,
            LegacyTenancyCloudBlobContainerBindings cloudBlobContainerBindings)
        {
            this.tenancyBindings = tenancyBindings;
            this.cloudBlobContainerBindings = cloudBlobContainerBindings;
            this.serviceProvider = ContainerBindings.GetServiceProvider(scenarioContext);

            string containerBase = Guid.NewGuid().ToString();

            this.blobStorageContainerDefinition = new BlobStorageContainerDefinition($"{containerBase}tenancyspecs");
        }

        private CloudBlobContainer Container => this.container ?? throw new InvalidOperationException("Container not created yet");

        [Given("I have added legacy blob storage configuration to the current tenant")]
        public void GivenIHaveAddedBlobStorageConfigurationToTheCurrentTenant(Table table)
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();

            var blobStorageConfiguration = new BlobStorageConfiguration();
            TenancyContainerScenarioBindings.Configuration.Bind("TESTBLOBSTORAGECONFIGURATIONOPTIONS", blobStorageConfiguration);

            blobStorageConfiguration.DisableTenantIdPrefix = bool.Parse(table.Rows[0]["DisableTenantIdPrefix"]);
            string overriddenContainerName = table.Rows[0]["Container"];
            if (!string.IsNullOrEmpty(overriddenContainerName))
            {
                // We can run into a problem when running tests multiple times: the container
                // name ends up being the same each time, and since we delete the container at
                // the end of the test, the next execution can fail because Azure Storage doesn't
                // let you create a container with the same name as one you've just deleted. (You
                // sometimes need to wait for a few minutes.) So we use a unique name.
                overriddenContainerName += Guid.NewGuid();

                blobStorageConfiguration.Container = overriddenContainerName;
            }

            tenantProvider.Root.UpdateProperties(values =>
                values.AddBlobStorageConfiguration(this.blobStorageContainerDefinition, blobStorageConfiguration));
        }

        [Then("I should be able to get the tenanted cloud blob container")]
        public async Task ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            this.container = await this.cloudBlobContainerBindings.ContainerFactory.GetBlobContainerForTenantAsync(
                this.tenancyBindings.RootTenant,
                this.blobStorageContainerDefinition).ConfigureAwait(false);

            Assert.IsNotNull(this.container);

            this.cloudBlobContainerBindings.RemoveThisContainerOnTestTeardown(this.Container);
        }

        [Then("the tenanted cloud blob container should be named using a hash of the tenant Id and the name specified in the blob container definition")]
        public void ThenTheTenantedCloudBlobContainerShouldBeNamedUsingAHashOfTheTenantIdAndTheNameSpecifiedInTheBlobContainerDefinition()
        {
            string expectedNamePlain = string.Concat(RootTenant.RootTenantId, "-", this.blobStorageContainerDefinition.ContainerName);
            string expectedName = AzureStorageNameHelper.HashAndEncodeBlobContainerName(expectedNamePlain);

            Assert.AreEqual(expectedName, this.Container.Name);
        }

        [Then("the tenanted cloud blob container should be named using a hash of the tenant Id and the name specified in the blob configuration")]
        public void ThenTheTenantedCloudBlobContainerShouldBeNamedUsingAHashOfTheTenantIdAndTheNameSpecifiedInTheBlobConfiguration()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            BlobStorageConfiguration blobStorageConfiguration = tenantProvider.Root.GetBlobStorageConfiguration(this.blobStorageContainerDefinition);

            string expectedNamePlain = string.Concat(RootTenant.RootTenantId, "-", blobStorageConfiguration.Container);
            string expectedName = AzureStorageNameHelper.HashAndEncodeBlobContainerName(expectedNamePlain);

            Assert.AreEqual(expectedName, this.Container.Name);
        }

        [Then("the tenanted cloud blob container should be named using a hash of the name specified in the blob configuration")]
        public void ThenTheTenantedCloudBlobContainerShouldBeNamedUsingAHashOfTheNameSpecifiedInTheBlobConfiguration()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            BlobStorageConfiguration blobStorageConfiguration = tenantProvider.Root.GetBlobStorageConfiguration(this.blobStorageContainerDefinition);

            string expectedNamePlain = blobStorageConfiguration.Container!;
            string expectedName = AzureStorageNameHelper.HashAndEncodeBlobContainerName(expectedNamePlain);

            Assert.AreEqual(expectedName, this.Container.Name);
        }

        [When("I remove the legacy blob storage configuration from the tenant")]
        public void WhenIRemoveTheBlobStorageConfigurationFromTheTenant()
        {
            this.tenancyBindings.RootTenant.UpdateProperties(
                propertiesToRemove: this.blobStorageContainerDefinition.RemoveBlobStorageConfiguration());
        }

        [Then("attempting to get the legacy blob storage configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheBlobStorageConfigurationOnTheTenantThrowsArgumentException()
        {
            try
            {
                this.tenancyBindings.RootTenant.GetBlobStorageConfiguration(this.blobStorageContainerDefinition);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}