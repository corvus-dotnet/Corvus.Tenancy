// <copyright file="CosmosStorageTenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        /// <param name="storageContextName">The storage context name identifying the Cosmos container.</param>
        /// <returns>The configuration for the Cosmos account for this tenant.</returns>
        public static CosmosConfiguration GetCosmosConfiguration(this ITenant tenant, string storageContextName)
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
            if (tenant.Properties.TryGet(GetConfigurationKey(storageContextName), out CosmosConfiguration? configuration))
            {
                return configuration;
            }

            throw new ArgumentException($"No Cosmos configuration was found for storage context name '{storageContextName}'");
        }

        /// <summary>
        /// Creates Cosmos container configuration properties suitable for passing to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </summary>
        /// <param name="values">Existing configuration values to which to append these.</param>
        /// <param name="storageContextName">The storage context name identifying the Cosmos container.</param>
        /// <param name="configuration">The configuration to set.</param>
        /// <returns>
        /// Properties pass to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </returns>
        public static IEnumerable<KeyValuePair<string, object>> AddCosmosConfiguration(
            this IEnumerable<KeyValuePair<string, object>> values,
            string storageContextName,
            CosmosConfiguration configuration)
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
        /// Describes how to clear the Cosmos configuration for the specified container from tenant
        /// properties in form suitable for passing as the <c>propertiesToRemove</c> argument to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </summary>
        /// <param name="storageContextName">The definition of the Cosmos container for which to remove the configuration.</param>
        /// <returns>
        /// A single-entry list of properties that can be passed to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>
        /// to remove the storage configuration.
        /// </returns>
        public static IEnumerable<string> RemoveCosmosConfiguration(string storageContextName)
        {
            if (storageContextName is null)
            {
                throw new ArgumentNullException(nameof(storageContextName));
            }

            return new string[] { GetConfigurationKey(storageContextName) };
        }

        private static string GetConfigurationKey(string storageContextName)
            => TenantStorageNameHelper.GetStorageContextConfigurationPropertyName<CosmosConfiguration>(storageContextName);
    }
}