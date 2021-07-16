// <copyright file="TenancyCosmosContainerBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Threading.Tasks;
    using Corvus.Azure.Cosmos.Tenancy;
    using Corvus.Tenancy;
    using Corvus.Testing.SpecFlow;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Specflow bindings to support a tenanted cloud blob container.
    /// </summary>
    [Binding]
    public static class TenancyCosmosContainerBindings
    {
        /// <summary>
        /// The key for the tenancy container in the feature context.
        /// </summary>
        public const string TenancySpecsContainer = "TenancySpecsContainer";

        /// <summary>
        /// Set up a tenanted Cloud Blob Container for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <param name="storageContextTestContext">
        /// Information about the storage context shared across test bindings.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.</remarks>
        [BeforeFeature("@setupTenantedCosmosContainer", Order = ContainerBeforeFeatureOrder.ServiceProviderAvailable)]
        public static async Task SetupCosmosContainerForRootTenant(
            FeatureContext featureContext,
            StorageContextTestContext storageContextTestContext)
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(featureContext);
            ITenantCosmosContainerFactory factory = serviceProvider.GetRequiredService<ITenantCosmosContainerFactory>();
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            IConfigurationRoot config = serviceProvider.GetRequiredService<IConfigurationRoot>();

            string databaseName = "endjinspecssharedthroughput";
            string containerStorageContextName = storageContextTestContext.ContextName;

            var cosmosConfiguration = new CosmosConfiguration();
            config.Bind("TESTCOSMOSCONFIGURATIONOPTIONS", cosmosConfiguration);
            cosmosConfiguration.DatabaseName = databaseName;
            cosmosConfiguration.ContainerName = storageContextTestContext.ContextName;
            tenantProvider.Root.UpdateProperties(values => values.AddCosmosConfiguration(containerStorageContextName, cosmosConfiguration));

            Container tenancySpecsContainer = await factory.GetContextForTenantAsync(
                tenantProvider.Root,
                containerStorageContextName).ConfigureAwait(false);

            await tenancySpecsContainer.Database.Client.CreateDatabaseIfNotExistsAsync(
                tenancySpecsContainer.Database.Id,
                throughput: 400);
            await tenancySpecsContainer.Database.CreateContainerIfNotExistsAsync(
                tenancySpecsContainer.Id,
                "/partitionKey");
            featureContext.Set(tenancySpecsContainer, TenancySpecsContainer);
        }

        /// <summary>
        /// Tear down the tenanted Cloud Blob Container for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterFeature("@setupTenantedCosmosContainer", Order = 100000)]
        public static Task TeardownCosmosDB(FeatureContext featureContext)
        {
            return featureContext.RunAndStoreExceptionsAsync(
                async () => await featureContext.Get<Container>(TenancySpecsContainer).DeleteContainerAsync().ConfigureAwait(false));
        }
    }
}