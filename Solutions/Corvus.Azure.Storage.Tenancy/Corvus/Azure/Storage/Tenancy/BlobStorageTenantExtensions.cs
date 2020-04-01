// <copyright file="BlobStorageTenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using System;
    using Corvus.Tenancy;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Extension methods for storage features for the tenant.
    /// </summary>
    public static class BlobStorageTenantExtensions
    {
        /// <summary>
        /// Determines whether the tenant has a storage blob configuration.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="definition">The blob storage container definition.</param>
        /// <returns>True if there is an explicit configuration set for this blob storage container definition.</returns>
        /// <remarks>This can be more efficient than trying to get the property, as it avoids a deserialization.</remarks>
        public static bool HasStorageBlobConfiguration(this ITenant tenant, BlobStorageContainerDefinition definition)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return ((JObject)tenant.Properties)[GetConfigurationKey(definition)] != null;
        }

        /// <summary>
        /// Get the configuration for the specified blob container definition for a particular tenant.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="definition">The blob storage container definition.</param>
        /// <returns>The configuration for the storage account for this tenant.</returns>
        public static BlobStorageConfiguration GetBlobStorageConfiguration(this ITenant tenant, BlobStorageContainerDefinition definition)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (tenant.Properties.TryGet(GetConfigurationKey(definition), out BlobStorageConfiguration configuration))
            {
                return configuration;
            }

            throw new ArgumentException($"No Blob storage configuration was found for definition with container name '{definition.ContainerName}'");
        }

        /// <summary>
        /// Sets the repository configuration or the specified repository for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to set the configuration.</param>
        /// <param name="definition">The definition of the repository for which to set the configuration.</param>
        /// <param name="configuration">The configuration to set.</param>
        public static void SetBlobStorageConfiguration(this ITenant tenant, BlobStorageContainerDefinition definition, BlobStorageConfiguration configuration)
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

        private static string GetConfigurationKey(BlobStorageContainerDefinition definition)
        {
            return $"StorageConfiguration__{definition.ContainerName}";
        }
    }
}
