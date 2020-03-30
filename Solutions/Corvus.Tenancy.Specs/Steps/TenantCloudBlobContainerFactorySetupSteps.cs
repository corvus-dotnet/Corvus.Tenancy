// <copyright file="TenantCloudBlobContainerFactorySetupSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Steps
{
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Tenancy.Specs.Bindings;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class TenantCloudBlobContainerFactorySetupSteps
    {
        private readonly ScenarioContext scenarioContext;

        public TenantCloudBlobContainerFactorySetupSteps(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        [Given("I do not have default configuration for tenanted blob storage")]
        public void GivenIDoNotHaveDefaultConfigurationForTenantedBlobStorage()
        {
            var options = new TenantCloudBlobContainerFactoryOptions
            {
                AzureServicesAuthConnectionString = "test connection string",
            };

            this.scenarioContext.Set(options);
        }

        [Given("I have default configuration for tenanted blob storage that uses the storage emulator")]
        public void GivenIHaveDefaultConfigurationForTenantedBlobStorageThatUsesTheStorageEmulator()
        {
            var options = new TenantCloudBlobContainerFactoryOptions
            {
                AzureServicesAuthConnectionString = "test connection string",
                RootTenantBlobStorageConfiguration = new BlobStorageConfiguration(),
            };

            this.scenarioContext.Set(options);
        }

        [Given("I have default configuration for tenanted blob storage that uses a real storage account")]
        public void GivenIHaveDefaultConfigurationForTenantedBlobStorageThatUsesARealStorageAccount()
        {
            // We need to populate the configuration's account name to simulate using real storage, but
            // the value doesn't actually matter as the targetted storage will not be used.
            var options = new TenantCloudBlobContainerFactoryOptions
            {
                AzureServicesAuthConnectionString = "test connection string",
                RootTenantBlobStorageConfiguration = new BlobStorageConfiguration
                {
                    AccountName = "teststorage",
                    KeyVaultName = "testkeyvault",
                },
            };

            this.scenarioContext.Set(options);
        }

        [When("I add the tenant cloud blob container factory to my service collection using the provided extension method")]
        public void WhenIAddTheTenantCloudBlobContainerFactoryToMyServiceCollectionUsingTheProvidedExtensionMethod()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<ITenantProvider, FakeTenantProvider>();

            TenantCloudBlobContainerFactoryOptions options = this.scenarioContext.Get<TenantCloudBlobContainerFactoryOptions>();

            serviceCollection.AddTenantCloudBlobContainerFactory(options);

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            this.scenarioContext.Set(serviceProvider);
        }

        [Then("no default storage configuration is added to the root tenant")]
        public void ThenNoDefaultStorageConfigurationIsAddedToTheRootTenant()
        {
            ServiceProvider serviceProvider = this.scenarioContext.Get<ServiceProvider>();

            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();

            ITenant rootTenant = tenantProvider.Root;

            // Resolve the tenancy service to force the initialisation to happen.
            serviceProvider.GetRequiredService<ITenantCloudBlobContainerFactory>();

            BlobStorageConfiguration? config = rootTenant.GetDefaultBlobStorageConfiguration();

            Assert.IsNull(config);
        }

        [Then("the default storage configuration is added to the root tenant")]
        public void ThenTheDefaultStorageConfigurationIsAddedToTheRootTenant()
        {
            ServiceProvider serviceProvider = this.scenarioContext.Get<ServiceProvider>();

            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();

            ITenant rootTenant = tenantProvider.Root;

            TenantCloudBlobContainerFactoryOptions expectedOptions = this.scenarioContext.Get<TenantCloudBlobContainerFactoryOptions>();

            // Resolve the tenancy service to force the initialisation to happen.
            serviceProvider.GetRequiredService<ITenantCloudBlobContainerFactory>();

            BlobStorageConfiguration? config = rootTenant.GetDefaultBlobStorageConfiguration();

            Assert.IsNotNull(config);

            // The config on the root tenant will not be the same instance as that we provided due to the
            // way that PropertyBag (used to store tenant properties) works.
            Assert.AreSame(expectedOptions.RootTenantBlobStorageConfiguration!.AccountName, config!.AccountName);
        }
    }
}
