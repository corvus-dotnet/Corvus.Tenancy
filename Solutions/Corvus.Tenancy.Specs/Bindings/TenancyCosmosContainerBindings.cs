// <copyright file="ClaimsCosmosDbBindings.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Threading.Tasks;
    using Corvus.Azure.Cosmos.Tenancy;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Tenancy;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Specflow bindings to support a tenanted cloud blob container.
    /// </summary>
    [Binding]
    public static class TenancyCosmosContainerBindings
    {
        /// <summary>
        /// The key for the claims permissions container in the feature context
        /// </summary>
        public const string TenancySpecsContainer = "TenancySpecsContainer";

        /// <summary>
        /// Set up a tenanted Cloud Blob Container for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <remarks>Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.</remarks>
        [BeforeFeature("@setupTenantedCosmosContainer", Order = ContainerBeforeFeatureOrder.ServiceProviderAvailable)]
        public static async Task SetupCosmosContainerForRootTenant(FeatureContext featureContext)
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(featureContext);
            ITenantCosmosContainerFactory factory = serviceProvider.GetRequiredService<ITenantCosmosContainerFactory>();
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();

            string containerBase = Guid.NewGuid().ToString();

            CosmosConfiguration config = tenantProvider.Root.GetDefaultCosmosConfiguration();
          //  config.DatabaseName = "endjinspecsshareddatabase";
          //  config.DisableTenantIdPrefix = true;
            tenantProvider.Root.SetDefaultCosmosConfiguration(config);

            Container tenancySpecsContainer = await factory.GetContainerForTenantAsync(
                tenantProvider.Root,
                new CosmosContainerDefinition("endjinspecsshareddatabase", $"{containerBase}tenancyspecs", "/partitionKey", databaseThroughput: 400));

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
                async () =>
                {
                    await featureContext.Get<Container>(TenancySpecsContainer).DeleteContainerAsync().ConfigureAwait(false);
                });
        }
    }
}