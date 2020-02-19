// <copyright file="CosmosConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    /// <summary>
    /// Encapsulates configuration for a container in a specific Cosmos account.
    /// </summary>
    public class CosmosConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosConfiguration"/> class.
        /// </summary>
        public CosmosConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the account URI.
        /// </summary>
        public string? AccountUri { get; set; }

        /// <summary>
        /// Gets or sets the name of the key vault in which the account secret is stored.
        /// </summary>
        public string? KeyVaultName { get; set; }

        /// <summary>
        /// Gets or sets the account key secret mame.
        /// </summary>
        public string? AccountKeySecretName { get; set; }

        /// <summary>
        /// Gets or sets the account key configuration key.
        /// </summary>
        public string? AccountKeyConfigurationKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable the tenant ID prefix.
        /// </summary>
        public bool DisableTenantIdPrefix { get; set; }

        /// <summary>
        /// Gets or sets the database name. If set, this overrides the value
        /// specified in <see cref="CosmosContainerDefinition.DatabaseName"/>.
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the container name. If set, this overrides the value
        /// specified in <see cref="CosmosContainerDefinition.ContainerName"/>.
        /// </summary>
        public string? ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the partition key path. If set, this overrides the value
        /// specified in <see cref="CosmosContainerDefinition.PartitionKeyPath"/>.
        /// </summary>
        public string? PartitionKeyPath { get; set; }
    }
}