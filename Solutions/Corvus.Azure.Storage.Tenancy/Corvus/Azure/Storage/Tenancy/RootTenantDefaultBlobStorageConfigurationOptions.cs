// <copyright file="RootTenantDefaultBlobStorageConfigurationOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    /// <summary>
    /// Defines settings for the default storage account in the root tenant.
    /// </summary>
    public sealed class RootTenantDefaultBlobStorageConfigurationOptions
    {
        /// <summary>
        /// Gets or sets the storage account name to use.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the name of the key vault in which the account secret is stored.
        /// </summary>
        public string KeyVaultName { get; set; }

        /// <summary>
        /// Gets or sets the name of secret in the key vault in which the account secret is stored.
        /// </summary>
        public string AccountKeySecretName { get; set; }

        /// <summary>
        /// Gets or sets the storage container name to use. Set this to force a particular
        /// container to be used regardless of what a <see cref="BlobStorageContainerDefinition"/> might
        /// specify.
        /// </summary>
        public string BlobStorageContainerName { get; set; }
    }
}
