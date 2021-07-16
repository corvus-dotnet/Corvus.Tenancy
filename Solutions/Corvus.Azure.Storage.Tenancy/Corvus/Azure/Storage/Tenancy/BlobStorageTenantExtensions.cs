// <copyright file="BlobStorageTenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Corvus.Tenancy;

    /// <summary>
    /// Extension methods for storage features for the tenant.
    /// </summary>
    public static class BlobStorageTenantExtensions
    {
        /// <summary>
        /// Determines whether the tenant has a storage blob configuration.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="storageContextName">The name of the storage context for this blob container.</param>
        /// <returns>True if there is an explicit configuration set for blob storage with this storage context name.</returns>
        /// <remarks>This can be more efficient than trying to get the property, as it avoids a deserialization.</remarks>
        public static bool HasStorageBlobConfiguration(this ITenant tenant, string storageContextName)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (storageContextName is null)
            {
                throw new ArgumentNullException(nameof(storageContextName));
            }

            return tenant.Properties.TryGet<BlobStorageConfiguration>(GetConfigurationKey(storageContextName), out BlobStorageConfiguration _);
        }

        /// <summary>
        /// Get the configuration for the specified blob container definition for a particular tenant.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="storageContextName">The name of the storage context for this blob container.</param>
        /// <returns>The configuration for the storage account for this tenant.</returns>
        public static BlobStorageConfiguration GetBlobStorageConfiguration(this ITenant tenant, string storageContextName)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (storageContextName is null)
            {
                throw new ArgumentNullException(nameof(storageContextName));
            }

            if (tenant.Properties.TryGet(GetConfigurationKey(storageContextName), out BlobStorageConfiguration? configuration))
            {
                return configuration;
            }

            throw new ArgumentException($"No Blob storage configuration was found for storage context with name '{storageContextName}'");
        }

        /// <summary>
        /// Creates repository configuration properties suitable for passing to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </summary>
        /// <param name="values">Existing configuration values to which to append these.</param>
        /// <param name="storageContextName">The the storage context name for the container for which to set the configuration.</param>
        /// <param name="configuration">The configuration to set.</param>
        /// <returns>
        /// Properties to pass to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </returns>
        public static IEnumerable<KeyValuePair<string, object>> AddBlobStorageConfiguration(
            this IEnumerable<KeyValuePair<string, object>> values,
            string storageContextName,
            BlobStorageConfiguration configuration)
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
        /// Describes how to clear the repository configuration or the specified repository from tenant
        /// properties in form suitable for passing as the <c>propertiesToRemove</c> argument to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </summary>
        /// <param name="storageContextName">The the storage context name for the container to remove.</param>
        /// <returns>
        /// A single-entry list of properties that can be passed to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>
        /// to remove the storage configuration.
        /// </returns>
        public static IEnumerable<string> RemoveBlobStorageConfiguration(string storageContextName)
        {
            if (storageContextName is null)
            {
                throw new ArgumentNullException(nameof(storageContextName));
            }

            return new string[] { GetConfigurationKey(storageContextName) };
        }

        private static string GetConfigurationKey(string contextName)
            => TenantStorageNameHelper.GetStorageContextConfigurationPropertyName<BlobStorageConfiguration>(contextName);
    }
}