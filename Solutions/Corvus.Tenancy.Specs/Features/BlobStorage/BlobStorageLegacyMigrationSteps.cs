// <copyright file="BlobStorageLegacyMigrationSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Features.BlobStorage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.Json;
    using Corvus.Storage.Azure.BlobStorage;
    using Corvus.Storage.Azure.BlobStorage.Tenancy;
    using Corvus.Testing.SpecFlow;

    using FluentAssertions;

    using global::Azure;
    using global::Azure.Core;
    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Blobs.Models;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Assist;

    [Binding]
    public sealed class BlobStorageLegacyMigrationSteps : IDisposable
    {
        private readonly IServiceProvider serviceProvider;
        private readonly string tenantId = RootTenant.RootTenantId.CreateChildId(Guid.NewGuid());
        private readonly TestSettings testStorageOptions;
        private readonly string testStorageConnectionString;
        private readonly Azure.Storage.Tenancy.BlobStorageConfiguration legacyConfigurationInTenant = new ();
        private readonly BlobContainerConfiguration v3ConfigurationInTenant = new ();
        private readonly IPropertyBagFactory pbf;
        private readonly MonitoringPolicy blobClientMonitor = new ();
        private readonly BlobClientOptions blobClientOptions;
        private readonly List<string> containersCreatedByTest = new ();
        private IPropertyBag tenantProperties;
        private ITenant? tenant;
        private BlobServiceClient? blobServiceClient;
        private BlobContainerClient? containerClientFromTestSubject;
        private BlobContainerConfiguration? v3ConfigFromMigration;

        public BlobStorageLegacyMigrationSteps(
            ScenarioContext scenarioContext)
        {
            this.serviceProvider = ContainerBindings.GetServiceProvider(scenarioContext);
            this.testStorageOptions = this.serviceProvider.GetRequiredService<TestSettings>();
            this.testStorageConnectionString = string.IsNullOrWhiteSpace(this.testStorageOptions.AzureStorageConnectionString)
                ? "UseDevelopmentStorage=true"
                : this.testStorageOptions.AzureStorageConnectionString;

            this.pbf = this.serviceProvider.GetRequiredService<IPropertyBagFactory>();
            this.tenantProperties = this.pbf.Create(PropertyBagValues.Empty);

            this.blobClientOptions = new BlobClientOptions();
            this.blobClientOptions.AddPolicy(this.blobClientMonitor, HttpPipelinePosition.PerCall);
        }

        // Annoyingly, SpecFlow currently doesn't support IAsyncDisposable
        public void Dispose()
        {
            IEnumerable<string> containersToDelete = this.blobClientMonitor.ContainersCreated
                .Concat(this.containersCreatedByTest);

            foreach (string containerName in containersToDelete)
            {
                BlobContainerClient? containerClient = this.blobServiceClient!.GetBlobContainerClient(containerName);
                containerClient.Delete();
            }
        }

        [Given("a legacy BlobStorageConfiguration with an AccountName and an AccessType of '([^']*)'")]
        public void GivenALegacyBlobStorageConfigurationWithConnectionStringAndAnAccessTypeOf(string accessType)
        {
            // AccountName is interpreted as a connection string when there's no AccountKeySecretName
            this.legacyConfigurationInTenant.AccountName = this.testStorageConnectionString;
            this.legacyConfigurationInTenant.AccessType = accessType == "null"
                ? null
                : Enum.Parse<Microsoft.Azure.Storage.Blob.BlobContainerPublicAccessType>(accessType);
        }

        [Given("a legacy BlobStorageConfiguration with a bogus AccountName and an AccessType of '([^']*)'")]
        public void GivenALegacyBlobStorageConfigurationWithABogusAccountNameAndAnAccessTypeOf(string accessType)
        {
            // AccountName is interpreted as a connection string when there's no AccountKeySecretName
            this.legacyConfigurationInTenant.AccountName = "ThisIsBogusToVerifyThatItIsNotBeUsed";
            this.legacyConfigurationInTenant.AccessType = accessType == "null"
                ? null
                : Enum.Parse<Microsoft.Azure.Storage.Blob.BlobContainerPublicAccessType>(accessType);
        }

        [Given("this test is using an Azure BlobServiceClient with a connection string")]
        public void GivenThisTestIsUsingAnAzureBlobServiceClientWithAConnectionStringOf()
        {
            this.blobServiceClient = new BlobServiceClient(this.testStorageConnectionString);
        }

        [Given("a tenant with the property '([^']*)' set to the legacy BlobStorageConfiguration")]
        public void GivenATenantWithThePropertySvSetToTheLegacyBlobStorageConfiguration(string configurationKey)
        {
            this.tenantProperties = this.pbf.CreateModified(
                this.tenantProperties,
                new Dictionary<string, object>
                {
                    { configurationKey, this.legacyConfigurationInTenant },
                },
                null);
        }

        [Given("a container with a tenant-specific name derived from '(.*)' exists")]
        public async Task GivenAContainerWithATenant_SpecificNameDerivedFromExists(string containerName)
        {
            string hashedTenantedContainerName = this.GetHashedTenantedContainerName(containerName);
            BlobContainerClient blobContainerClient = this.blobServiceClient!.GetBlobContainerClient(hashedTenantedContainerName);
            await blobContainerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
            this.containersCreatedByTest.Add(hashedTenantedContainerName);
        }

        [Given("a v3 BlobContainerConfiguration with a ConnectionStringPlainText")]
        public void GivenAVBlobContainerConfigurationWithAConnectionStringPlainText()
        {
            this.v3ConfigurationInTenant.ConnectionStringPlainText = this.testStorageConnectionString;
        }

        [Given("a tenant with the property '([^']*)' set to the v3 BlobContainerConfiguration")]
        public void GivenATenantWithThePropertySvSetToTheVBlobContainerConfiguration(string configurationKey)
        {
            this.tenantProperties = this.pbf.CreateModified(
                this.tenantProperties,
                new Dictionary<string, object>
                {
                    { configurationKey, this.v3ConfigurationInTenant },
                },
                null);
        }

        [When(@"IBlobContainerSourceWithTenantLegacyTransition\.GetBlobContainerClientFromTenantAsync is called with a container name of '(.*)' and configuration keys of '(.*)' and '(.*)'")]
        public async Task WhenIBlobContainerSourceWithTenantLegacyTransition_GetBlobContainerClientFromTenantAsyncIsCalledWithAContainerNameOfAndConfigurationKeysOfAnd(
            string containerName, string v2ConfigurationKey, string v3ConfigurationKey)
        {
            ITenant tenant = this.GetTenantCreatingIfNecessary();

            // We also create the container at the last minute to enable other tests steps to
            // update the settings object.
            // TODO: probably not necessary now - don't really need the settings?
            IBlobContainerSourceWithTenantLegacyTransition blobContainerSource = this.serviceProvider.GetRequiredService<IBlobContainerSourceWithTenantLegacyTransition>();

            this.containerClientFromTestSubject = await blobContainerSource.GetBlobContainerClientFromTenantAsync(
                tenant,
                v2ConfigurationKey,
                v3ConfigurationKey,
                containerName,
                this.blobClientOptions)
                .ConfigureAwait(false);
        }

        [When(@"IBlobContainerSourceWithTenantLegacyTransition\.MigrateToV3Async is called with a container name of '(.*)' and configuration keys of '(.*)' and '(.*)'")]
        public async Task WhenIBlobContainerSourceWithTenantLegacyTransition_MigrateToVAsyncIsCalledWithAContainerNameOfAndAVConfigurationKeyOf(
            string containerName, string v2ConfigurationKey, string v3ConfigurationKey)
        {
            ITenant tenant = this.GetTenantCreatingIfNecessary();

            // We also create the container at the last minute to enable other tests steps to
            // update the settings object.
            // TODO: probably not necessary now - don't really need the settings?
            IBlobContainerSourceWithTenantLegacyTransition blobContainerSource = this.serviceProvider.GetRequiredService<IBlobContainerSourceWithTenantLegacyTransition>();

            this.v3ConfigFromMigration = await blobContainerSource.MigrateToV3Async(
                tenant,
                v2ConfigurationKey,
                v3ConfigurationKey,
                new[] { containerName },
                this.blobClientOptions)
                .ConfigureAwait(false);
        }

        [Then("a new container with a tenant-specific name derived from '(.*)' should have been created with public access of '(.*)'")]
        public async Task ThenANewContainerWithATenant_SpecificNameDerivedFromShouldHaveBeenCreatedWithPublicAccessOf(
            string containerName, PublicAccessType publicAccessType)
        {
            string hashedTenantedContainerName = this.GetHashedTenantedContainerName(containerName);
            BlobContainerClient blobContainer = this.blobServiceClient!.GetBlobContainerClient(hashedTenantedContainerName);
            Response<BlobContainerAccessPolicy> response = await blobContainer.GetAccessPolicyAsync().ConfigureAwait(false);
            Assert.AreEqual(200, response.GetRawResponse().Status);
            Assert.AreEqual(publicAccessType, response.Value.BlobPublicAccess);
        }

        [Then("the BlobContainerClient should have access to the container with a tenant-specific name derived from '(.*)'")]
        public void ThenTheBlobContainerClientShouldHaveAccessToTheContainerWithATenant_SpecificNameDerivedFrom(
            string containerName)
        {
            string hashedTenantedContainerName = this.GetHashedTenantedContainerName(containerName);
            Assert.AreEqual(hashedTenantedContainerName, this.containerClientFromTestSubject!.Name);
        }

        [Then(@"IBlobContainerSourceWithTenantLegacyTransition\.MigrateToV3Async should have returned a BlobContainerConfiguration with these settings")]
        public void ThenMigrateToVAsyncShouldHaveReturnedABlobContainerConfigurationWithTheseSettings(Table configurationTable)
        {
            BlobContainerConfiguration expectedConfiguration = configurationTable.CreateInstance<BlobContainerConfiguration>();

            // We recognized a couple of special values in the test for this configuration where we
            // plug in real values at runtime (because the test code can't know what the actual
            // values should be).
            const string tenantedContainerPrefix = "tenanted-";
            if (expectedConfiguration.Container is string containerValueInTable &&
                containerValueInTable.StartsWith(tenantedContainerPrefix))
            {
                string containerName = containerValueInTable[tenantedContainerPrefix.Length..];
                expectedConfiguration.Container = this.GetHashedTenantedContainerName(containerName);
            }

            if (expectedConfiguration.ConnectionStringPlainText == "testAccountConnectionString")
            {
                expectedConfiguration.ConnectionStringPlainText = this.testStorageConnectionString;
            }

            this.v3ConfigFromMigration.Should().BeEquivalentTo(expectedConfiguration);
        }

        [Then("MigrateToV3Async should have returned null")]
        public void ThenMigrateToVAsyncShouldHaveReturnedNull()
        {
            this.v3ConfigFromMigration.Should().BeNull();
        }

        [Then("no new container should have been created")]
        public void ThenNoNewContainerShouldHaveBeenCreated()
        {
            Assert.IsEmpty(this.blobClientMonitor.ContainersCreated);
        }

        private ITenant GetTenantCreatingIfNecessary()
        {
            // We create the tenant at the last minute, to ensure that all relevant Given steps
            // have had the opportunity to update the property bag.
            return this.tenant ??= new Tenant(
                this.tenantId,
                "MyTestTenant",
                this.tenantProperties);
        }

        private string GetHashedTenantedContainerName(string containerName)
        {
            string tenantedUnhashedContainerName = $"{this.tenantId.ToLowerInvariant()}-{containerName}";
            string hashedTenantedContainerName = AzureStorageNameHelper.HashAndEncodeBlobContainerName(tenantedUnhashedContainerName);
            return hashedTenantedContainerName;
        }

        private class MonitoringPolicy : global::Azure.Core.Pipeline.HttpPipelineSynchronousPolicy
        {
            public List<string> ContainerCreateIfExistsCalls { get; } = new List<string>();

            public List<string> ContainersCreated { get; } = new List<string>();

            public override void OnReceivedResponse(HttpMessage message)
            {
                if (message.Request.Method.Equals(RequestMethod.Put))
                {
                    bool isDevStorage = message.Request.Uri.Host == "127.0.0.1" && message.Request.Uri.Port == 10000;
                    string path = isDevStorage
                        ? message.Request.Uri.Path["devstoreaccount1/".Length..]
                        : message.Request.Uri.Path;

                    int slashPos = path.IndexOf('/');
                    string containerName = slashPos < 0 ? path : path[0..slashPos];
                    Dictionary<string, Microsoft.Extensions.Primitives.StringValues> queryString =
                        Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(message.Request.Uri.Query);
                    if (queryString.TryGetValue("restype", out Microsoft.Extensions.Primitives.StringValues restype)
                        && restype.Equals("container"))
                    {
                        this.ContainerCreateIfExistsCalls.Add(containerName);
                        if (message.Response.Status == 201)
                        {
                            this.ContainersCreated.Add(containerName);
                        }
                    }
                }

                base.OnReceivedResponse(message);
            }
        }
    }
}