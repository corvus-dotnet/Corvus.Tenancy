﻿// <copyright file="SqlStorageTenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Sql.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
            if (tenant.Properties.TryGet(GetConfigurationKey(definition), out SqlConfiguration? configuration))
            {
                return configuration;
            }

            throw new ArgumentException($"No Sql configuration was found for connection definition with database name '{definition.Database}'");
        }

        /// <summary>
        /// Creates Sql configuration properties for the specified container suitable for passing to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </summary>
        /// <param name="values">Existing configuration values to which to append these.</param>
        /// <param name="definition">The definition of the Sql container for which to set the configuration.</param>
        /// <param name="configuration">The configuration to set.</param>
        /// <returns>
        /// Properties to pass to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </returns>
        public static IEnumerable<KeyValuePair<string, object>> AddSqlConfiguration(
            this IEnumerable<KeyValuePair<string, object>> values,
            SqlConnectionDefinition definition,
            SqlConfiguration configuration)
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

        private static string GetConfigurationKey(SqlConnectionDefinition definition)
        {
            return $"StorageConfiguration__{definition.Database}";
        }
    }
}