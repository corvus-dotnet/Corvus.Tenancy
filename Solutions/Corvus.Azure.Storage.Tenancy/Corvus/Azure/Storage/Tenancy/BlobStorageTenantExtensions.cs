// <copyright file="BlobStorageTenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using System;
    using Corvus.Tenancy;

    /// <summary>
    /// Extension methods for storage features for the tenant.
    /// </summary>
    public static class BlobStorageTenantExtensions
    {
        private const string DefaultStorageConfigurationKey = "DefaultStorageConfiguration";
        private const string KeyVaultNameKey = "KeyVaultName";

        /// <summary>
        /// Get the configuration for the specified blob container definition for a particular tenant.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="definition">The blob storage container definition.</param>
        /// <returns>The configuration for the storage account for this tenant.</returns>
        public static IStorageConfiguration GetStorageConfiguration(this ITenant tenant, BlobStorageContainerDefinition definition)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (tenant.Properties.TryGet(GetConfigurationKey(definition), out IStorageConfiguration configuration))
            {
                return configuration;
            }

            // Otherwise, return the default
            return GetDefaultStorageConfiguration(tenant);
        }

        /// <summary>
        /// Gets the default storage configuration for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to get the default configuration.</param>
        /// <returns>The Default <see cref="IStorageConfiguration"/> for the tenant.</returns>
        public static IStorageConfiguration GetDefaultStorageConfiguration(this ITenant tenant)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (tenant.Properties.TryGet(DefaultStorageConfigurationKey, out IStorageConfiguration configuration))
            {
                return configuration;
            }

            return null;
        }

        /// <summary>
        /// Sets the repository configuration or the specified repository for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to set the default configuration.</param>
        /// <param name="definition">The definition of the repository for which to set the configuration.</param>
        /// <param name="configuration">The configuration to set.</param>
        public static void SetBlobStorageConfiguration(this ITenant tenant, BlobStorageContainerDefinition definition, IStorageConfiguration configuration)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            tenant.Properties.Set(GetConfigurationKey(definition), configuration);
        }

        /// <summary>
        /// Sets the default repository configuration for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to set the default configuration.</param>
        /// <param name="defaultConfiguration">The default configuration to set.</param>
        public static void SetDefaultStorageConfiguration(this ITenant tenant, IStorageConfiguration defaultConfiguration)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (defaultConfiguration is null)
            {
                throw new ArgumentNullException(nameof(defaultConfiguration));
            }

            tenant.Properties.Set(DefaultStorageConfigurationKey, defaultConfiguration);
        }

        /// <summary>
        /// Gets the key value name for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to set the key vault name.</param>
        /// <returns>The name of the key vault.</returns>
        public static string GetKeyVaultName(this ITenant tenant)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (tenant.Properties.TryGet(KeyVaultNameKey, out string vaultName))
            {
                return vaultName;
            }

            throw new InvalidOperationException("The vault name has not been set for the tenant");
        }

        /// <summary>
        /// Sets the key value namefor the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to set the key vault name.</param>
        /// <param name="keyVaultName">The name of the key vault.</param>
        public static void SetKeyVaultName(this ITenant tenant, string keyVaultName)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            tenant.Properties.Set(KeyVaultNameKey, keyVaultName);
        }

        private static string GetConfigurationKey(BlobStorageContainerDefinition definition)
        {
            return $"StorageConfiguration__{definition.ContainerName}";
        }
    }
}
