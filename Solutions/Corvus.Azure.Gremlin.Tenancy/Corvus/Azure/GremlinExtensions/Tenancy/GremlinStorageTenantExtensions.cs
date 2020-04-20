// <copyright file="GremlinStorageTenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy
{
    using System;
    using Corvus.Tenancy;

    /// <summary>
    /// Extension methods for storage features for the tenant.
    /// </summary>
    public static class GremlinStorageTenantExtensions
    {
        /// <summary>
        /// Get the configuration for the specified Gremlin container definition for a particular tenant.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="definition">The Gremlin storage container definition.</param>
        /// <returns>The configuration for the Gremlin account for this tenant.</returns>
        public static GremlinConfiguration GetGremlinConfiguration(this ITenant tenant, GremlinContainerDefinition definition)
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
            if (tenant.Properties.TryGet(GetConfigurationKey(definition), out GremlinConfiguration configuration) && configuration != null)
            {
                return configuration;
            }

            throw new ArgumentException($"No Gremlin configuration was found for definition with database name '{definition.DatabaseName}' and container name '{definition.ContainerName}'");
        }

        /// <summary>
        /// Sets the Gremlin configuration for the specified container for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to set the configuration.</param>
        /// <param name="definition">The definition of the Gremlin container for which to set the configuration.</param>
        /// <param name="configuration">The configuration to set.</param>
        public static void SetGremlinConfiguration(this ITenant tenant, GremlinContainerDefinition definition, GremlinConfiguration configuration)
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
        /// Clears the Gremlin configuration for the specified container for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to clear the configuration.</param>
        /// <param name="definition">The definition of the Gremlin container for which to clear the configuration.</param>
        public static void ClearGremlinConfiguration(this ITenant tenant, GremlinContainerDefinition definition)
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

        private static string GetConfigurationKey(GremlinContainerDefinition definition)
        {
            return $"StorageConfiguration__{definition.DatabaseName}__{definition.ContainerName}";
        }
    }
}