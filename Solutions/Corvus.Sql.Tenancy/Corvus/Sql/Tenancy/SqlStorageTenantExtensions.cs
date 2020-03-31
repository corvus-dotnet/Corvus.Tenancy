// <copyright file="SqlStorageTenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Sql.Tenancy
{
    using System;
    using Corvus.Tenancy;

    /// <summary>
    /// Extension methods for storage features for the tenant.
    /// </summary>
    public static class SqlStorageTenantExtensions
    {
        /// <summary>
        /// Get the configuration for the specified Sql container definition for a particular tenant.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="definition">The Sql storage container definition.</param>
        /// <returns>The configuration for the Sql account for this tenant.</returns>
        public static SqlConfiguration GetSqlConfiguration(this ITenant tenant, SqlConnectionDefinition definition)
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
            if (tenant.Properties.TryGet(GetConfigurationKey(definition), out SqlConfiguration configuration))
            {
                return configuration;
            }

            throw new ArgumentException($"No Sql configuration was found for connection definition with database name '{definition.Database}'");
        }

        /// <summary>
        /// Sets the Sql configuration for the specified container for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to set the configuration.</param>
        /// <param name="definition">The definition of the Sql container for which to set the configuration.</param>
        /// <param name="configuration">The configuration to set.</param>
        public static void SetSqlConfiguration(this ITenant tenant, SqlConnectionDefinition definition, SqlConfiguration configuration)
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

        private static string GetConfigurationKey(SqlConnectionDefinition definition)
        {
            return $"StorageConfiguration__{definition.Database}";
        }
    }
}