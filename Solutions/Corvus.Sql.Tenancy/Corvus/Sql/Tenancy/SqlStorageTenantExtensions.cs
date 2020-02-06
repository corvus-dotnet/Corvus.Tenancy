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
        private const string DefaultSqlConfigurationKey = "DefaultSqlConfiguration";

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

            // Otherwise, return the default
            return tenant.GetDefaultSqlConfiguration();
        }

        /// <summary>
        /// Gets the default Sql configuration for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to get the default configuration.</param>
        /// <returns>The Default <see cref="SqlConfiguration"/> for the tenant.</returns>
        public static SqlConfiguration GetDefaultSqlConfiguration(this ITenant tenant)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (tenant.Properties.TryGet(DefaultSqlConfigurationKey, out SqlConfiguration configuration))
            {
                return configuration;
            }

            return null;
        }

        /// <summary>
        /// Sets the Sql configuration for the specified container for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to set the default configuration.</param>
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

        /// <summary>
        /// Sets the default Sql configuration for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to set the default configuration.</param>
        /// <param name="defaultConfiguration">The default configuration to set.</param>
        public static void SetDefaultSqlConfiguration(this ITenant tenant, SqlConfiguration defaultConfiguration)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (defaultConfiguration is null)
            {
                throw new ArgumentNullException(nameof(defaultConfiguration));
            }

            tenant.Properties.Set(DefaultSqlConfigurationKey, defaultConfiguration);
        }

        private static string GetConfigurationKey(SqlConnectionDefinition definition)
        {
            return $"StorageConfiguration__{definition.Database}";
        }
    }
}