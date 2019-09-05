// <copyright file="RootTenantDefaultCosmosConfigurationOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    /// <summary>
    /// Defines settings for the default storage account in the root tenant.
    /// </summary>
    public sealed class RootTenantDefaultCosmosConfigurationOptions
    {
        /// <summary>
        /// Gets or sets the storage account name to use.
        /// </summary>
        public string CosmosAccountUri { get; set; }

        /// <summary>
        /// Gets or sets the name of the key vault in which the account secret is stored.
        /// </summary>
        public string KeyVaultName { get; set; }

        /// <summary>
        /// Gets or sets the name of secret in the key vault in which the account secret is stored.
        /// </summary>
        public string CosmosAccountKeySecretName { get; set; }

        /// <summary>
        /// Gets or sets the Cosmos database name to use. Set this to force a particular
        /// container to be used regardless of what a <see cref="CosmosContainerDefinition"/> might
        /// specify.
        /// </summary>
        public string CosmosDatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the Cosmos container name to use. Set this to force a particular
        /// container to be used regardless of what a <see cref="CosmosContainerDefinition"/> might
        /// specify.
        /// </summary>
        public string CosmosContainerName { get; set; }
    }
}