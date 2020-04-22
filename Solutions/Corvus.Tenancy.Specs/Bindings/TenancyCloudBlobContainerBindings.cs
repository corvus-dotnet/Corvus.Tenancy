// <copyright file="TenancyCloudBlobContainerBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Threading.Tasks;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Tenancy;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Specflow bindings to support a tenanted cloud blob container.
    /// </summary>
    [Binding]
    public static class TenancyCloudBlobContainerBindings
    {
        /// <summary>
        /// The key for the tenancy container in the feature context.
        /// </summary>
        public const string TenancySpecsContainer = "TenancySpecsContainer";

        /// <summary>
        /// Set up a tenanted Cloud Blob Container for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <remarks>Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.</remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [BeforeFeature("@setupTenantedCloudBlobContainer", Order = ContainerBeforeFeatureOrder.ServiceProviderAvailable)]
        public static async Task SetupCloudBlobContainerForRootTenant(FeatureContext featureContext)
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(featureContext);
            ITenantCloudBlobContainerFactory factory = serviceProvider.GetRequiredService<ITenantCloudBlobContainerFactory>();
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            IConfigurationRoot config = serviceProvider.GetRequiredService<IConfigurationRoot>();

            string containerBase = Guid.NewGuid().ToString();

            var blobStorageContainerDefinition = new BlobStorageContainerDefinition($"{containerBase}tenancyspecs");
            featureContext.Set(blobStorageContainerDefinition);
            var blobStorageConfiguration = new BlobStorageConfiguration();
            config.Bind("TESTBLOBSTORAGECONFIGURATIONOPTIONS", blobStorageConfiguration);
            tenantProvider.Root.UpdateProperties(values => values.AddBlobStorageConfiguration(blobStorageContainerDefinition, blobStorageConfiguration));

            CloudBlobContainer tenancySpecsContainer = await factory.GetBlobContainerForTenantAsync(
                tenantProvider.Root,
                blobStorageContainerDefinition).ConfigureAwait(false);

            featureContext.Set(tenancySpecsContainer, TenancySpecsContainer);
        }

        /// <summary>
        /// Tear down the tenanted Cloud Blob Container for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterFeature("@setupTenantedCloudBlobContainer", Order = 100000)]
        public static Task TeardownCosmosDB(FeatureContext featureContext)
        {
            return featureContext.RunAndStoreExceptionsAsync(
                async () => await featureContext.Get<CloudBlobContainer>(TenancySpecsContainer).DeleteAsync().ConfigureAwait(false));
        }
    }
}