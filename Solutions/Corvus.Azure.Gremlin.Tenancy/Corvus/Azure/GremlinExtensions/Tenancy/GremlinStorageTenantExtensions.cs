// <copyright file="GremlinStorageTenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Corvus.Extensions.Json;
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
            if (tenant.Properties.TryGetNonNullValue(GetConfigurationKey(definition), out GremlinConfiguration? configuration))
            {
                return configuration;
            }

            throw new ArgumentException($"No Gremlin configuration was found for definition with database name '{definition.DatabaseName}' and container name '{definition.ContainerName}'");
        }

        /// <summary>
        /// Creates Gremlin configuration for the specified container suitable for passing to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </summary>
        /// <param name="values">Existing configuration values to which to append these.</param>
        /// <param name="definition">The definition of the Gremlin container for which to set the configuration.</param>
        /// <param name="configuration">The configuration to set.</param>
        /// <returns>
        /// Properties to pass to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </returns>
        public static IEnumerable<KeyValuePair<string, object>> AddGremlinConfiguration(
            this IEnumerable<KeyValuePair<string, object>> values,
            GremlinContainerDefinition definition,
            GremlinConfiguration configuration)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return values.Append(new KeyValuePair<string, object>(GetConfigurationKey(definition), configuration));
        }

        private static string GetConfigurationKey(GremlinContainerDefinition definition)
        {
            return $"StorageConfiguration__{definition.DatabaseName}__{definition.ContainerName}";
        }
    }
}