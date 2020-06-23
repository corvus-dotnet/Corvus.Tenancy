// <copyright file="TenancyCloudTableBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Threading.Tasks;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Tenancy;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Specflow bindings to support a tenanted cloud table container.
    /// </summary>
    [Binding]
    public static class TenancyCloudTableBindings
    {
        /// <summary>
        /// The key for the tenancy container in the feature context.
        /// </summary>
        public const string TenancySpecsContainer = "TenancySpecsContainer";

        /// <summary>
        /// Set up a tenanted Cloud Table Container for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <remarks>Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.</remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [BeforeFeature("@setupTenantedCloudTable", Order = ContainerBeforeFeatureOrder.ServiceProviderAvailable)]
        public static async Task SetupCloudTableForRootTenant(FeatureContext featureContext)
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(featureContext);
            ITenantCloudTableFactory factory = serviceProvider.GetRequiredService<ITenantCloudTableFactory>();
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            IConfigurationRoot config = serviceProvider.GetRequiredService<IConfigurationRoot>();

            string containerBase = Guid.NewGuid().ToString();

            var tableStorageTableDefinition = new TableStorageTableDefinition($"{containerBase}tenancyspecs");
            featureContext.Set(tableStorageTableDefinition);
            var tableStorageConfiguration = new TableStorageConfiguration();
            config.Bind("TESTTABLESTORAGECONFIGURATIONOPTIONS", tableStorageConfiguration);
            tenantProvider.Root.UpdateProperties(values => values.AddTableStorageConfiguration(tableStorageTableDefinition, tableStorageConfiguration));

            CloudTable tenancySpecsContainer = await factory.GetTableForTenantAsync(
                tenantProvider.Root,
                tableStorageTableDefinition).ConfigureAwait(false);

            featureContext.Set(tenancySpecsContainer, TenancySpecsContainer);
        }

        /// <summary>
        /// Tear down the tenanted Cloud Table Container for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <returns>A <see cref="Task"/> which completes once the operation has completed.</returns>
        [AfterFeature("@setupTenantedCloudTable", Order = 100000)]
        public static Task TeardownCosmosDB(FeatureContext featureContext)
        {
            return featureContext.RunAndStoreExceptionsAsync(
                async () => await featureContext.Get<CloudTable>(TenancySpecsContainer).DeleteAsync().ConfigureAwait(false));
        }
    }
}