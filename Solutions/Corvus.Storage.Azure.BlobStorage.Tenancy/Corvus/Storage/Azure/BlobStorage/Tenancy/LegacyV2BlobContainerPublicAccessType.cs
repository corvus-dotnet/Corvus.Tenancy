// <copyright file="LegacyV2BlobContainerPublicAccessType.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage.Tenancy
{
    /// <summary>
    /// Identical to the Azure Storage SDK v11's <c>BlobContainerPublicAccessType</c> type. This
    /// enables this project to read configuration in the legacy v2 <c>BlobStorageConfiguration</c>
    /// format without imposing a dependency on the old v2 components, or on the old Azure Storage
    /// SDK.
    /// </summary>
    public enum LegacyV2BlobContainerPublicAccessType
    {
        /// <summary>
        /// No public access. Only the account owner can read resources in this container.
        /// </summary>
        Off = 0,

        /// <summary>
        /// Container-level public access. Anonymous clients can read container and blob
        /// data.
        /// </summary>
        Container = 1,

        /// <summary>
        /// Blob-level public access. Anonymous clients can read blob data within this container,
        /// but not container data.
        /// </summary>
        Blob = 2,

        /// <summary>
        /// Unknown access type.
        /// </summary>
        Unknown = 3,
    }
}