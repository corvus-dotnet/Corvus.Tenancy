// <copyright file="CosmosStorageTenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    using System;
    using Corvus.Tenancy;

    /// <summary>
    /// Extension methods for storage features for the tenant.
    /// </summary>
    public static class CosmosStorageTenantExtensions
    {
        /// <summary>
        /// Get the configuration for the specified Cosmos container definition for a particular tenant.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="definition">The Cosmos storage container definition.</param>
        /// <returns>The configuration for the Cosmos account for this tenant.</returns>
        public static CosmosConfiguration GetCosmosConfiguration(this ITenant tenant, CosmosContainerDefinition definition)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            // First, try the configuration specific to this instance
            if (tenant.Properties.TryGet(GetConfigurationKey(definition), out CosmosConfiguration configuration) && configuration != null)
            {
                return configuration;
            }

            throw new ArgumentException($"No Cosmos configuration was found for definition with database name '{definition.DatabaseName}' and container name '{definition.ContainerName}'");
        }

        /// <summary>
        /// Sets the Cosmos configuration for the specified container for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to set the configuration.</param>
        /// <param name="definition">The definition of the Cosmos container for which to set the configuration.</param>
        /// <param name="configuration">The configuration to set.</param>
        public static void SetCosmosConfiguration(this ITenant tenant, CosmosContainerDefinition definition, CosmosConfiguration configuration)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            tenant.Properties.Set(GetConfigurationKey(definition), configuration);
        }

        /// <summary>
        /// Clears the Cosmos configuration for the specified container for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to clear the configuration.</param>
        /// <param name="definition">The definition of the Cosmos container for which to clear the configuration.</param>
        public static void ClearCosmosConfiguration(this ITenant tenant, CosmosContainerDefinition definition)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            tenant.Properties.Set<object?>(GetConfigurationKey(definition), null);
        }

        private static string GetConfigurationKey(CosmosContainerDefinition definition)
        {
            return $"StorageConfiguration__{definition.DatabaseName}__{definition.ContainerName}";
        }
    }
}