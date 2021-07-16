// <copyright file="TenantBlobContainerClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy.Internal
{
    using Corvus.Tenancy;

    using global::Azure.Storage.Blobs;

    /// <summary>
    /// A factory for a tenanted <see cref="BlobContainerClient"/> instances.
    /// </summary>
    public class TenantBlobContainerClientFactory :
        TenantedStorageContextFactory<BlobContainerClient, BlobStorageConfiguration>,
        ITenantBlobContainerClientFactory
    {
        /// <summary>
        /// Creates a <see cref="TenantBlobContainerClientFactory"/>.
        /// </summary>
        /// <param name="options">Configuration settings.</param>
        public TenantBlobContainerClientFactory(TenantBlobContainerClientFactoryOptions? options = null)
            : base(new BlobContainerClientFactory(options))
        {
        }
    }
}
