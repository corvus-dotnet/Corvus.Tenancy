// <copyright file="IBlobContainerSourceWithTenantLegacyTransition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage.Tenancy
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Corvus.Tenancy;

    using global::Azure.Storage.Blobs;

    /// <summary>
    /// Provides <see cref="BlobContainerClient"/> instance based on configuration stored in tenant
    /// properties, with support for migration from V2 to V3 of <c>Corvus.Tenancy</c>.
    /// </summary>
    public interface IBlobContainerSourceWithTenantLegacyTransition
    {
        /// <summary>
        /// Gets a <see cref="BlobContainerClient"/> based on configuration stored in a tenant.
        /// </summary>
        /// <param name="tenant">The tenant containing the configuration.</param>
        /// <param name="v2ConfigurationKey">
        /// The key identifying the <see cref="ITenant.Properties"/> entry containing legacy v2
        /// configuration, which will be used if there is no v3 configuration present.
        /// </param>
        /// <param name="v3ConfigurationKey">
        /// The key identifying the <see cref="ITenant.Properties"/> entry which, if present, will
        /// contain the<see cref="BlobContainerConfiguration"/> to use.
        /// </param>
        /// <param name="containerName">
        /// An optional container name to use. If this is null, the container name specified in the
        /// configuration will be used. In cases where multiple containers are in use, it's common
        /// to have one configuration entry with a null container name, and to specify the
        /// container name required when asking for a <see cref="BlobContainerClient"/>.
        /// </param>
        /// <param name="blobClientOptions">
        /// Optional blob client parameters to be passed when calling the underlying
        /// <see cref="IStorageContextSourceFromDynamicConfiguration{TStorageContext, TConfiguration, TConnectionOptions}.GetStorageContextAsync(TConfiguration, TConnectionOptions, CancellationToken)"/>
        /// method to create the container client.
        /// </param>
        /// <param name="cancellationToken">
        /// May enable the operation to be cancelled.
        /// </param>
        /// <returns>
        /// A value task that produces a <see cref="BlobContainerClient"/>.
        /// </returns>
        ValueTask<BlobContainerClient> GetBlobContainerClientFromTenantAsync(
            ITenant tenant,
            string v2ConfigurationKey,
            string v3ConfigurationKey,
            string? containerName = null,
            BlobClientOptions? blobClientOptions = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes the necessary preparations for upgrading from v2 to v3 configuration.
        /// </summary>
        /// <param name="tenant">The tenant containing the configuration.</param>
        /// <param name="v2ConfigurationKey">
        /// The key identifying the <see cref="ITenant.Properties"/> entry that will containing the
        /// legacy v2 configuration, when it is present. (An application may call this method for
        /// each of its tenants, in which case it's possible that some of them have only v3
        /// configuration. We do not treat this as an error.)
        /// </param>
        /// <param name="v3ConfigurationKey">
        /// The key identifying the <see cref="ITenant.Properties"/> entry that will containing the
        /// legacy v3 configuration, when it is present. If this is present, this method will make
        /// no changes and will return a null configuration.
        /// </param>
        /// <param name="containerNames">
        /// In cases where the configuration does not specify the container name (typically because
        /// the application uses the same configuration to access several different containers)
        /// this lists all of the containers the application uses. This method will ensure that
        /// tenant-specific containers exist for each name before returning. In cases where the
        /// configuration does specify the container name, this should be null.
        /// </param>
        /// <param name="blobClientOptions">
        /// Optional blob client parameters to be passed when calling the underlying
        /// <see cref="IStorageContextSourceFromDynamicConfiguration{TStorageContext, TConfiguration, TConnectionOptions}.GetStorageContextAsync(TConfiguration, TConnectionOptions, CancellationToken)"/>
        /// method to create the container client.
        /// </param>
        /// <param name="cancellationToken">
        /// May enable the operation to be cancelled.
        /// </param>
        /// <returns>
        /// A value task that produces null if a configuration with the key specified in
        /// <paramref name="v3ConfigurationKey"/> was present in the tenant. Otherwise,
        /// returns a <see cref="BlobContainerConfiguration"/> that contains v3 configuration
        /// equivalent to the existing v2 configuration. If no configuration with
        /// the key specified in <paramref name="v2ConfigurationKey"/> was present in the tenant,
        /// this produces null. (TODO: should it throw.)
        /// </returns>
        ValueTask<BlobContainerConfiguration?> MigrateToV3Async(
            ITenant tenant,
            string v2ConfigurationKey,
            string v3ConfigurationKey,
            IEnumerable<string>? containerNames,
            BlobClientOptions? blobClientOptions = null,
            CancellationToken cancellationToken = default);
    }
}