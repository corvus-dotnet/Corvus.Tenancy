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
        /// The <see cref="IBlobContainerSourceByConfiguration"/> that provides the underlying
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
            this IBlobContainerSourceByConfiguration blobContainerSource,
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

        /// <summary>
        /// Gets a <see cref="BlobContainerClient"/> using configuration stored in a tenant using
        /// the old tenancy-v2-style <c>BlobStorageConfiguration</c>.
        /// </summary>
        /// <param name="blobContainerSource">
        /// The <see cref="IBlobContainerSourceByConfiguration"/> that provides the underlying
        /// ability to supply a <see cref="BlobContainerClient"/> for a
        /// <see cref="BlobContainerConfiguration"/>.
        /// </param>
        /// <param name="tenant">
        /// The tenant containing the legacy v2 <c>BlobStorageConfiguration</c>.
        /// </param>
        /// <param name="containerName">
        /// The name that legacy code would have specified in the
        /// <c>BlobStorageContainerDefinition.ContainerName</c> property when using the old v2
        /// API.
        /// </param>
        /// <returns>
        /// A value task that produces a <see cref="BlobContainerClient"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This enables systems that were using <c>Corvus.Tenancy</c> v2 to transition to the
        /// current version. Those older systems will have existing tenant hierarchies where the
        /// tenant property bags contain configuration settings in the old format. (The old
        /// <c>BlobStorageConfiguration</c>, instead of the <see cref="BlobContainerConfiguration"/>
        /// type introduced by <c>Corvus.Storage</c>.)
        /// </para>
        /// <para>
        /// This method enables applications to move away from the deprecated Azure Storage SDK v11
        /// client types that <c>Corvus.Tenancy</c> v2 works with, while continuing to use the old
        /// configuration format in their tenant data. It works by reading data out of the tenant
        /// property bag in the old format, and dynamically converting that to a
        /// <see cref="BlobContainerConfiguration"/> before deferring to the
        /// <see cref="IBlobContainerSourceByConfiguration"/>.
        /// </para>
        /// </remarks>
        public static ValueTask<BlobContainerClient> GetBlobContainerClientFromTenantWithV2BlobStorageConfigurationAsync(
            this IBlobContainerSourceByConfiguration blobContainerSource,
            ITenant tenant,
            string containerName)
        {
            ////if (!tenant.Properties.TryGet(configurationKey, out BlobContainerConfiguration? configuration))
            ////{
            ////    throw new ArgumentException($"Tenant {tenant.Id} does not contain a property '{configurationKey}'")
            ////}

            throw new NotImplementedException();
        }
    }
}