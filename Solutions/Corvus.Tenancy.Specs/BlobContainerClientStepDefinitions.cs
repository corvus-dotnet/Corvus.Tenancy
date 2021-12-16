// <copyright file="BlobContainerClientStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Corvus.Json;
    using Corvus.Storage.Azure.BlobStorage;
    using Corvus.Storage.Azure.BlobStorage.Tenancy;
    using Corvus.Tenancy.Internal;

    using global::Azure.Storage.Blobs;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using TechTalk.SpecFlow;

    [Binding]
    public sealed class BlobContainerClientStepDefinitions : IDisposable
    {
        private const string TenantStoragePropertyKey = "MyStorageSettings";
        private const string StorageAccountName = "myaccount";
        private readonly List<(BlobContainerConfiguration Configuration, BlobClientOptions? ConnectionOptions)> contextsRequested = new ();
        private readonly List<(BlobContainerConfiguration Configuration, BlobClientOptions? ConnectionOptions)> contextsReplaced = new ();
        private readonly ServiceProvider serviceProvider;
        private readonly RootTenant tenant;
        private readonly IBlobContainerSourceFromDynamicConfiguration containerSourceSource;

        private BlobContainerConfiguration? configuration;

        public BlobContainerClientStepDefinitions()
        {
            this.containerSourceSource = new FakeBlobContainerSourceFromDynamicConfiguration(
                this.contextsRequested,
                this.contextsReplaced);
            var services = new ServiceCollection();
            services.AddRequiredTenancyServices();
            this.serviceProvider = services.BuildServiceProvider();

            this.tenant = new RootTenant(this.serviceProvider.GetRequiredService<IPropertyBagFactory>());
        }

        public void Dispose()
        {
            this.serviceProvider.Dispose();
        }

        [Given("I have added blob storage configuration a tenant with the a container name of '([^']*)'")]
        public void GivenIHaveAddedBlobStorageConfigurationATenantWithTheAContainerNameOf(string containerName)
        {
            this.configuration = new BlobContainerConfiguration
            {
                AccountName = StorageAccountName,
                Container = containerName,
            };

            this.tenant.UpdateProperties(values =>
                values.AddBlobStorageConfiguration(TenantStoragePropertyKey, this.configuration));
        }

        [Given("I have added blob storage configuration a tenant without a container name")]
        public void GivenIHaveAddedBlobStorageConfigurationATenantWithoutAContainerName()
        {
            this.configuration = new BlobContainerConfiguration
            {
                AccountName = StorageAccountName,
            };

            this.tenant.UpdateProperties(values =>
                values.AddBlobStorageConfiguration(TenantStoragePropertyKey, this.configuration));
        }

        [Given("I get a BlobContainerClient for the tenant without specifying a container name")]
        [When("I get a BlobContainerClient for the tenant without specifying a container name")]
        public async Task WhenIGetABlobContainerClientForTheTenantWithoutSpecifyingAContainerName()
        {
            await this.containerSourceSource.GetBlobContainerClientFromTenantAsync(
                this.tenant,
                TenantStoragePropertyKey)
                .ConfigureAwait(false);
        }

        [Given("I get a BlobContainerClient for the tenant specifying a container name of '([^']*)'")]
        [When("I get a BlobContainerClient for the tenant specifying a container name of '([^']*)'")]
        public async Task WhenIGetABlobContainerClientForTheTenantSpecifyingAContainerNameOf(string containerName)
        {
            await this.containerSourceSource.GetBlobContainerClientFromTenantAsync(
                this.tenant,
                TenantStoragePropertyKey,
                containerName)
                .ConfigureAwait(false);
        }

        [When("I get a replacement BlobContainerClient for the tenant without specifying a container name")]
        public async Task WhenIGetAReplacementBlobContainerClientForTheTenantWithoutSpecifyingAContainerName()
        {
            await this.containerSourceSource.GetReplacementForFailedBlobContainerClientFromTenantAsync(
                this.tenant,
                TenantStoragePropertyKey)
                .ConfigureAwait(false);
        }

        [When("I get a replacement BlobContainerClient for the tenant specifying a container name of '([^']*)'")]
        public async Task WhenIGetAReplacementBlobContainerClientForTheTenantSpecifyingAContainerNameOfAsync(string containerName)
        {
            await this.containerSourceSource.GetReplacementForFailedBlobContainerClientFromTenantAsync(
                this.tenant,
                TenantStoragePropertyKey,
                containerName)
                .ConfigureAwait(false);
        }

        [When("I remove the blob storage configuration from the tenant")]
        public void WhenIRemoveTheBlobStorageConfigurationFromTheTenant()
        {
            this.tenant.UpdateProperties(
                propertiesToRemove: new[] { TenantStoragePropertyKey });
        }

        [Then("the BlobContainerClient source should have been given a configuration identical to the original one")]
        public void ThenTheBlobContainerClientSourceShouldHaveBeenGivenAConfigurationIdenticalToTheOriginalOne()
        {
            Assert.AreEqual(this.configuration, this.contextsRequested.Single().Configuration);
        }

        [Then("the BlobContainerClient source should have been given a configuration based on the original one but with the Container set to '([^']*)'")]
        public void ThenTheBlobContainerClientSourceShouldHaveBeenGivenAConfigurationBasedOnTheOriginalOneButWithTheContainerSetTo(
            string containerName)
        {
            BlobContainerConfiguration? expectedConfiguration = this.configuration! with
            {
#pragma warning disable SA1101 // Prefix local calls with this - StyleCop doesn't understand record types yet
                Container = containerName,
#pragma warning restore SA1101 // Prefix local calls with this
            };

            Assert.AreEqual(expectedConfiguration, this.contextsRequested.Single().Configuration);
        }

        [Then("the BlobContainerClient source should have been asked to replace a configuration identical to the original one")]
        public void ThenTheBlobContainerClientSourceShouldHaveBeenAskedToReplaceAConfigurationIdenticalToTheOriginalOne()
        {
            Assert.AreEqual(this.configuration, this.contextsReplaced.Single().Configuration);
        }

        [Then("the BlobContainerClient source should have been asked to replace a configuration based on the original one but with the Container set to '([^']*)'")]
        public void ThenTheBlobContainerClientSourceShouldHaveBeenAskedToReplaceAConfigurationBasedOnTheOriginalOneButWithTheContainerSetTo(
            string containerName)
        {
            BlobContainerConfiguration? expectedConfiguration = this.configuration! with
            {
#pragma warning disable SA1101 // Prefix local calls with this - StyleCop doesn't understand record types yet
                Container = containerName,
#pragma warning restore SA1101 // Prefix local calls with this
            };

            Assert.AreEqual(expectedConfiguration, this.contextsReplaced.Single().Configuration);
        }

        [Then("attempting to get the blob storage configuration from the tenant throws an ArgumentException")]
        public void ThenAttemptingToGetTheBlobStorageConfigurationFromTheTenantThrowsAnArgumentException()
        {
            try
            {
                this.tenant.GetBlobContainerConfiguration(TenantStoragePropertyKey);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }

        private class FakeBlobContainerSourceFromDynamicConfiguration : IBlobContainerSourceFromDynamicConfiguration
        {
            private readonly List<(BlobContainerConfiguration Configuration, BlobClientOptions? ConnectionOptions)> contextsRequested;
            private readonly List<(BlobContainerConfiguration Configuration, BlobClientOptions? ConnectionOptions)> contextsReplaced;

            public FakeBlobContainerSourceFromDynamicConfiguration(
                List<(BlobContainerConfiguration Configuration, BlobClientOptions? ConnectionOptions)> contextsRequested,
                List<(BlobContainerConfiguration Configuration, BlobClientOptions? ConnectionOptions)> contextsReplaced)
            {
                this.contextsRequested = contextsRequested;
                this.contextsReplaced = contextsReplaced;
            }

            public ValueTask<BlobContainerClient> GetReplacementForFailedStorageContextAsync(
                BlobContainerConfiguration contextConfiguration,
                BlobClientOptions? connectionOptions,
                CancellationToken cancellationToken)
            {
                this.contextsReplaced.Add((contextConfiguration, connectionOptions));
                return new ValueTask<BlobContainerClient>(new BlobContainerClient(new Uri("https://example.com/test")));
            }

            public ValueTask<BlobContainerClient> GetStorageContextAsync(
                BlobContainerConfiguration contextConfiguration,
                BlobClientOptions? connectionOptions,
                CancellationToken cancellationToken)
            {
                this.contextsRequested.Add((contextConfiguration, connectionOptions));
                return new ValueTask<BlobContainerClient>(new BlobContainerClient(new Uri("https://example.com/test")));
            }
        }
    }
}