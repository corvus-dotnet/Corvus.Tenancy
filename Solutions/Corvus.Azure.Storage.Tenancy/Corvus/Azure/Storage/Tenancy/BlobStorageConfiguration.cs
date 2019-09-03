// <copyright file="BlobStorageConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using Microsoft.Azure.Storage.Blob;

    /// <summary>
    /// Encapsulates configuration for the blob-specific aspects of a storage account.
    /// </summary>
    public class BlobStorageConfiguration
    {
        /// <summary>
        /// Gets or sets the container name. If set, this overrides the name specified in
        /// <see cref="BlobStorageContainerDefinition.ContainerName"/>.
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// Gets or sets the access type for the container. If set, this overrides the value
        /// specified in <see cref="BlobStorageContainerDefinition.AccessType"/>.
        /// </summary>
        public BlobContainerPublicAccessType? AccessType { get; set; }
    }
}
