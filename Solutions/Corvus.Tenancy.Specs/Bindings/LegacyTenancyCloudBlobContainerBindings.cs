// <copyright file="LegacyTenancyCloudBlobContainerBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Testing.ReqnRoll;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.DependencyInjection;

    using Reqnroll;

    /// <summary>
    /// Specflow bindings to support a tenanted cloud blob container.
    /// </summary>
    [Binding]
    public class LegacyTenancyCloudBlobContainerBindings
    {
        private readonly FeatureContext featureContext;
        private readonly ScenarioContext scenarioContext;
        private readonly List<CloudBlobContainer> containersToRemoveAtTeardown = new();
        private ITenantCloudBlobContainerFactory? containerFactory;

        public LegacyTenancyCloudBlobContainerBindings(
            FeatureContext featureContext,
            ScenarioContext scenarioContext)
        {
            this.featureContext = featureContext;
            this.scenarioContext = scenarioContext;
        }

        public ITenantCloudBlobContainerFactory ContainerFactory => this.containerFactory ?? throw new InvalidOperationException("Factory has not been set up yet");

        public void RemoveThisContainerOnTestTeardown(CloudBlobContainer container)
        {
            this.containersToRemoveAtTeardown.Add(container);
        }

        /// <summary>
        /// Initializes the container before each scenario runs.
        /// </summary>
        [BeforeScenario("@setupTenantedCloudBlobContainer", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection + 1)]
        public void InitializeContainer()
        {
            if (!this.featureContext.FeatureInfo.Tags.Any(t => t == "perFeatureContainer"))
            {
                ContainerBindings.ConfigureServices(
                    this.scenarioContext,
                    services => Init(services, TenancyContainerScenarioBindings.Configuration["AzureServicesAuthConnectionString"]));
            }
        }

        [BeforeFeature("@setupTenantedCloudBlobContainer", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection + 1)]
        public static void InitializeContainer(FeatureContext featureContext)
        {
            if (featureContext.FeatureInfo.Tags.Any(t => t == "perFeatureContainer"))
            {
                ContainerBindings.ConfigureServices(
                    featureContext,
                    services => Init(services, TenancyContainerScenarioBindings.Configuration["AzureServicesAuthConnectionString"]));
            }
        }

        [BeforeScenario("@setupTenantedCloudBlobContainer", Order = ContainerBeforeScenarioOrder.ServiceProviderAvailable)]
        public void GetServices()
        {
            IServiceProvider serviceProvider = this.featureContext.FeatureInfo.Tags.Any(t => t == "perFeatureContainer")
                ? ContainerBindings.GetServiceProvider(this.featureContext)
                : ContainerBindings.GetServiceProvider(this.scenarioContext);
            this.containerFactory = serviceProvider.GetRequiredService<ITenantCloudBlobContainerFactory>();
        }

        /// <summary>
        /// Tear down any Cloud Blob Containers created while running the test.
        /// </summary>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterScenario("@setupTenantedCloudBlobContainer", Order = 100000)]
        public Task TeardownCloudBlobs()
        {
            return this.scenarioContext.RunAndStoreExceptionsAsync(
                async () =>
                {
                    foreach (CloudBlobContainer container in this.containersToRemoveAtTeardown)
                    {
                        await container.DeleteAsync().ConfigureAwait(false);
                    }
                });
        }

        private static void Init(IServiceCollection serviceCollection, string? azureServicesAuthConnectionString)
        {
            var blobOptions = new TenantCloudBlobContainerFactoryOptions
            {
                AzureServicesAuthConnectionString = azureServicesAuthConnectionString,
            };

            serviceCollection.AddTenantCloudBlobContainerFactory(blobOptions);
        }
    }
}