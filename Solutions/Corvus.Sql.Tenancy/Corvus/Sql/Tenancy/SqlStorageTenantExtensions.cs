// <copyright file="SqlStorageTenantExtensions.cs" company="Endjin Limited">
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
        /// <param name="storageContextName">The storage context name identifying the SQL container.</param>
        /// <returns>The configuration for the Sql account for this tenant.</returns>
        public static SqlConfiguration GetSqlConfiguration(this ITenant tenant, string storageContextName)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (storageContextName is null)
            {
                throw new ArgumentNullException(nameof(storageContextName));
            }

            // First, try the configuration specific to this instance
            if (tenant.Properties.TryGet(GetConfigurationKey(storageContextName), out SqlConfiguration? configuration))
            {
                return configuration;
            }

            throw new ArgumentException($"No Sql configuration was found for connection definition with database name '{storageContextName}'");
        }

        /// <summary>
        /// Creates Sql configuration properties for the specified container suitable for passing to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </summary>
        /// <param name="values">Existing configuration values to which to append these.</param>
        /// <param name="storageContextName">The storage context name identifying the SQL container for which to set the configuration.</param>
        /// <param name="configuration">The configuration to set.</param>
        /// <returns>
        /// Properties to pass to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </returns>
        public static IEnumerable<KeyValuePair<string, object>> AddSqlConfiguration(
            this IEnumerable<KeyValuePair<string, object>> values,
            string storageContextName,
            SqlConfiguration configuration)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (storageContextName is null)
            {
                throw new ArgumentNullException(nameof(storageContextName));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return values.Append(new KeyValuePair<string, object>(GetConfigurationKey(storageContextName), configuration));
        }

        /// <summary>
        /// Describes how to clear the Sql configuration for the specified container from tenant
        /// properties in form suitable for passing as the <c>propertiesToRemove</c> argument to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </summary>
        /// <param name="storageContextName">The storage context name identifying the SQL container for which to clear the configuration.</param>
        /// <returns>
        /// A single-entry list of properties that can be passed to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>
        /// to remove the storage configuration.
        /// </returns>
        public static IEnumerable<string> RemoveSqlConfiguration(string storageContextName)
        {
            if (storageContextName is null)
            {
                throw new ArgumentNullException(nameof(storageContextName));
            }

            return new string[] { GetConfigurationKey(storageContextName) };
        }

        private static string GetConfigurationKey(string storageContextName)
            => TenantStorageNameHelper.GetStorageContextConfigurationPropertyName<SqlConfiguration>(storageContextName);
    }
}