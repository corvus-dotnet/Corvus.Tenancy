// <copyright file="BlobStorageTenancyExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage.Tenancy
{
    using System;
    using System.Threading.Tasks;

    using Corvus.Tenancy;

    using global::Azure.Storage.Blobs;

    /// <summary>
    /// Extension methods providing tenanted access to blob storage.
    /// </summary>
    public static class BlobStorageTenancyExtensions
    {
        /// <summary>
        /// Gets a <see cref="BlobContainerClient"/> using <see cref="BlobContainerConfiguration"/>
        /// stored in a tenant.
        /// </summary>
        /// <param name="blobContainerSource">
        /// The <see cref="IBlobContainerSourceFromDynamicConfiguration"/> that provides the underlying
        /// ability to supply a <see cref="BlobContainerClient"/> for a
        /// <see cref="BlobContainerConfiguration"/>.
        /// </param>
        /// <param name="tenant">
        /// The tenant containing the <see cref="BlobContainerConfiguration"/>.
        /// </param>
        /// <param name="configurationKey">
        /// The key identifying the <see cref="ITenant.Properties"/> entry containing the
        /// <see cref="BlobContainerConfiguration"/> to use.
        /// </param>
        /// <param name="containerName">
        /// An optional container name to use. If this is null, the container name specified in the
        /// <see cref="BlobContainerConfiguration"/> will be used. In cases where multiple
        /// containers are in use, it's common to have one <see cref="BlobContainerConfiguration"/>
        /// with a null <see cref="BlobContainerConfiguration.Container"/>, and to specify the
        /// container name required when asking for a <see cref="BlobContainerClient"/>.
        /// </param>
        /// <returns>
        /// A value task that produces a <see cref="BlobContainerClient"/>.
        /// </returns>
        public static async ValueTask<BlobContainerClient> GetBlobContainerClientFromTenantAsync(
            this IBlobContainerSourceFromDynamicConfiguration blobContainerSource,
            ITenant tenant,
            string configurationKey,
            string? containerName = null)
        {
            if (!tenant.Properties.TryGet(configurationKey, out BlobContainerConfiguration? configuration))
            {
                throw new ArgumentException($"Tenant {tenant.Id} does not contain a property '{configurationKey}'");
            }

            if (containerName is not null)
            {
                configuration = configuration.ForContainer(containerName);
            }

            return await blobContainerSource.GetStorageContextAsync(configuration).ConfigureAwait(false);
        }
    }
}