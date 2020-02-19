// <copyright file="BlobStorageConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using Microsoft.Azure.Storage.Blob;

    /// <summary>
    /// Encapsulates configuration for a storage account.
    /// </summary>
    public class BlobStorageConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageConfiguration"/> class.
        /// </summary>
        public BlobStorageConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        /// <remarks>If the account key secret name is empty, then this should contain a complete connection string.</remarks>
        public string? AccountName { get; set; }

        /// <summary>
        /// Gets or sets the container name. If set, this overrides the name specified in
        /// <see cref="BlobStorageContainerDefinition.ContainerName"/>.
        /// </summary>
        public string? Container { get; set; }

        /// <summary>
        /// Gets or sets the access type for the container. If set, this overrides the value
        /// specified in <see cref="BlobStorageContainerDefinition.AccessType"/>.
        /// </summary>
        public BlobContainerPublicAccessType? AccessType { get; set; }

        /// <summary>
        /// Gets or sets the key value name.
        /// </summary>
        public string? KeyVaultName { get; set; }

        /// <summary>
        /// Gets or sets the account key secret mame.
        /// </summary>
        public string? AccountKeySecretName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable the tenant ID prefix.
        /// </summary>
        public bool DisableTenantIdPrefix { get; set; }
    }
}
