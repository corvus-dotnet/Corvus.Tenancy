// <copyright file="RootTenantCosmosConfigurationOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    /// <summary>
    /// Defines settings for the default storage account in the root tenant.
    /// </summary>
    public sealed class RootTenantCosmosConfigurationOptions : CosmosConfiguration
    {
        /// <summary>
        /// Creates a new instance of a <see cref="CosmosConfiguration"/> from these options.
        /// </summary>
        /// <returns>The <see cref="CosmosConfiguration"/> derived from these options.</returns>
        public CosmosConfiguration CreateCosmosConfigurationInstance()
        {
            return new CosmosConfiguration()
            {
                AccountKeyConfigurationKey = this.AccountKeyConfigurationKey,
                AccountKeySecretName = this.AccountKeySecretName,
                ContainerName = string.IsNullOrWhiteSpace(this.ContainerName) ? null : this.ContainerName,
                DatabaseName = string.IsNullOrWhiteSpace(this.DatabaseName) ? null : this.DatabaseName,
                DisableTenantIdPrefix = this.DisableTenantIdPrefix,
                KeyVaultName = this.KeyVaultName,
                AccountUri = this.AccountUri,
                PartitionKeyPath = this.PartitionKeyPath,
            };
        }
    }
}