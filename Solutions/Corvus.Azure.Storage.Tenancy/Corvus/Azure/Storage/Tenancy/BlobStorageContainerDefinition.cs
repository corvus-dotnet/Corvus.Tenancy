﻿// <copyright file="BlobStorageContainerDefinition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using Microsoft.Azure.Storage.Blob;

    /// <summary>
    /// A definition of a blob storage container.
    /// </summary>
    public class BlobStorageContainerDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageContainerDefinition"/> class.
        /// </summary>
        /// <param name="containerName">The <see cref="ContainerName"/>.</param>
        /// <param name="accessType">The <see cref="AccessType"/>.</param>
        public BlobStorageContainerDefinition(string containerName, BlobContainerPublicAccessType accessType = BlobContainerPublicAccessType.Off)
        {
            this.ContainerName = containerName;
            this.AccessType = accessType;
        }

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the access type for the container.
        /// </summary>
        public BlobContainerPublicAccessType AccessType { get; set; }
    }
}