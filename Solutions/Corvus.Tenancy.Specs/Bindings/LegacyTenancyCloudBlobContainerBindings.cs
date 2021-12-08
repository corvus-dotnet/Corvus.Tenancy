// <copyright file="LegacyTenancyCloudBlobContainerBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Testing.SpecFlow;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.DependencyInjection;

    using TechTalk.SpecFlow;

    /// <summary>
    /// Specflow bindings to support a tenanted cloud blob container.
    /// </summary>
    [Binding]
    public class LegacyTenancyCloudBlobContainerBindings
    {
        private readonly ScenarioContext scenarioContext;
        private readonly TenancyContainerScenarioBindings tenancyContainer;
        private readonly List<CloudBlobContainer> containersToRemoveAtTeardown = new ();
        private ITenantCloudBlobContainerFactory? containerFactory;

        public LegacyTenancyCloudBlobContainerBindings(
            ScenarioContext scenarioContext,
            TenancyContainerScenarioBindings tenancyContainer)
        {
            this.scenarioContext = scenarioContext;
            this.tenancyContainer = tenancyContainer;
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
            ContainerBindings.ConfigureServices(
                   this.scenarioContext,
                   serviceCollection =>
                   {
                       var blobOptions = new TenantCloudBlobContainerFactoryOptions
                       {
                           AzureServicesAuthConnectionString = this.tenancyContainer.Configuration["AzureServicesAuthConnectionString"],
                       };

                       serviceCollection.AddTenantCloudBlobContainerFactory(blobOptions);
                   });
        }

        [BeforeScenario("@setupTenantedCloudBlobContainer", Order = ContainerBeforeScenarioOrder.ServiceProviderAvailable)]
        public void GetServices()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.scenarioContext);
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
    }
}