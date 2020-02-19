// <copyright file="ClaimsCosmosDbBindings.cs" company="Endjin">
// Copyright (c) Endjin. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Threading.Tasks;
    using Corvus.Azure.GremlinExtensions.Tenancy;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Tenancy;
    using Gremlin.Net.Driver;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Specflow bindings to support a tenanted cosmos gremlin container.
    /// </summary>
    [Binding]
    public static class TenancyGremlinContainerBindings
    {
        /// <summary>
        /// The key for the claims permissions container in the feature context
        /// </summary>
        public const string TenancySpecsContainer = "TenancySpecsContainer";

        /// <summary>
        /// Set up a tenanted Gremlin Client for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <remarks>Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.</remarks>
        [BeforeFeature("@setupTenantedGremlinClient", Order = ContainerBeforeFeatureOrder.ServiceProviderAvailable)]
        public static async Task SetupGremlinContainerForRootTenant(FeatureContext featureContext)
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(featureContext);
            ITenantGremlinContainerFactory factory = serviceProvider.GetRequiredService<ITenantGremlinContainerFactory>();
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();

            string containerBase = Guid.NewGuid().ToString();

            GremlinConfiguration config = tenantProvider.Root.GetDefaultGremlinConfiguration()!;
            config.DatabaseName = "endjinspecssharedthroughput";
            config.DisableTenantIdPrefix = true;
            tenantProvider.Root.SetDefaultGremlinConfiguration(config);

            GremlinClient tenancySpecsClient = await factory.GetClientForTenantAsync(
                tenantProvider.Root,
                new GremlinContainerDefinition("endjinspecssharedthroughput", $"{containerBase}tenancyspecs")).ConfigureAwait(false);

            featureContext.Set(tenancySpecsClient, TenancySpecsContainer);
        }

        /// <summary>
        /// Tear down the tenanted Gremlin Client for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterFeature("@setupTenantedGremlinContainer", Order = 100000)]
        public static Task TeardownGremlinDB(FeatureContext featureContext)
        {
            return featureContext.RunAndStoreExceptionsAsync(
                () =>
                {
                    // Note: the gremlin container factory doesn't currently create the container, so
                    // we don't currently have anything to delete.
                    featureContext.Get<GremlinClient>(TenancySpecsContainer).Dispose();
                    return Task.CompletedTask;
                });
        }
    }
}