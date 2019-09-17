// <copyright file="RootTenantBlobStorageConfigurationOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    /// <summary>
    /// Defines settings for the default storage account in the root tenant.
    /// </summary>
    public sealed class RootTenantBlobStorageConfigurationOptions : BlobStorageConfiguration
    {
        /// <summary>
        /// Creates a new instance of a <see cref="BlobStorageConfiguration"/> from these options.
        /// </summary>
        /// <returns>The <see cref="BlobStorageConfiguration"/> derived from these options.</returns>
        public BlobStorageConfiguration CreateBlobStorageConfigurationInstance()
        {
            return new BlobStorageConfiguration()
            {
                AccessType = this.AccessType,
                AccountKeySecretName = this.AccountKeySecretName,
                AccountName = this.AccountName,
                Container = string.IsNullOrWhiteSpace(this.Container) ? null : this.Container,
                DisableTenantIdPrefix = this.DisableTenantIdPrefix,
                KeyVaultName = this.KeyVaultName,
            };
        }
    }
}
