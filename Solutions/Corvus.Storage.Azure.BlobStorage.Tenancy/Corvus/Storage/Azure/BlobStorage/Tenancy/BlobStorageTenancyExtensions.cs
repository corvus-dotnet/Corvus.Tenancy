// <copyright file="BlobStorageTenancyExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Corvus.Tenancy;

    using global::Azure.Storage.Blobs;

    /// <summary>
    /// Extension methods providing tenanted access to blob storage.
    /// </summary>
    public static class BlobStorageTenancyExtensions
    {
        /// <summary>
        /// Creates repository configuration properties suitable for passing to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </summary>
        /// <param name="values">Existing configuration values to which to append these.</param>
        /// <param name="key">The key to use for the property.</param>
        /// <param name="configuration">The configuration to set.</param>
        /// <returns>
        /// Properties to pass to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </returns>
        public static IEnumerable<KeyValuePair<string, object>> AddBlobStorageConfiguration(
            this IEnumerable<KeyValuePair<string, object>> values,
            string key,
            BlobContainerConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(values);
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(configuration);

            return values.Append(new KeyValuePair<string, object>(key, configuration));
        }

        /// <summary>
        /// Get the configuration for the specified blob container definition for a particular tenant.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="key">The key of the tenancy property containing the settings.</param>
        /// <returns>The configuration for the storage account for this tenant.</returns>
        public static BlobContainerConfiguration GetBlobContainerConfiguration(
            this ITenant tenant,
            string key)
        {
            ArgumentNullException.ThrowIfNull(tenant);
            ArgumentNullException.ThrowIfNull(key);

            if (tenant.Properties.TryGet(key, out BlobContainerConfiguration? configuration))
            {
                return configuration;
            }

            throw new InvalidOperationException($"Tenant {tenant.Id} does not contain a property '{key}'");
        }

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
            BlobContainerConfiguration configuration = tenant.GetBlobContainerConfiguration(configurationKey);

            if (containerName is not null)
            {
                configuration = configuration with
                {
                    Container = containerName,
                };
            }

            return await blobContainerSource.GetStorageContextAsync(configuration).ConfigureAwait(false);
        }

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
        public static async ValueTask<BlobContainerClient> GetReplacementForFailedBlobContainerClientFromTenantAsync(
            this IBlobContainerSourceFromDynamicConfiguration blobContainerSource,
            ITenant tenant,
            string configurationKey,
            string? containerName = null)
        {
            BlobContainerConfiguration configuration = tenant.GetBlobContainerConfiguration(configurationKey);

            if (containerName is not null)
            {
                configuration = configuration with
                {
                    Container = containerName,
                };
            }

            return await blobContainerSource.GetReplacementForFailedStorageContextAsync(configuration).ConfigureAwait(false);
        }
    }
}