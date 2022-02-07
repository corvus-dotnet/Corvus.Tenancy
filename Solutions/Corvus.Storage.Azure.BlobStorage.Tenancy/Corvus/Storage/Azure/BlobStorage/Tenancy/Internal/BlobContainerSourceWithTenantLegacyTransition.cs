// <copyright file="BlobContainerSourceWithTenantLegacyTransition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage.Tenancy.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Corvus.Storage.Azure.BlobStorage.Tenancy;
    using Corvus.Tenancy;

    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Blobs.Models;

    /// <summary>
    /// Implementation of <see cref="IBlobContainerSourceWithTenantLegacyTransition"/>.
    /// </summary>
    internal class BlobContainerSourceWithTenantLegacyTransition : IBlobContainerSourceWithTenantLegacyTransition
    {
        private readonly IBlobContainerSourceFromDynamicConfiguration blobContainerSource;

        /// <summary>
        /// Creates a <see cref="BlobContainerSourceWithTenantLegacyTransition"/>.
        /// </summary>
        /// <param name="blobContainerSource">
        /// The underling non-legacy source.
        /// </param>
        public BlobContainerSourceWithTenantLegacyTransition(
            IBlobContainerSourceFromDynamicConfiguration blobContainerSource)
        {
            this.blobContainerSource = blobContainerSource;
        }

        /// <inheritdoc/>
        public async ValueTask<BlobContainerClient> GetBlobContainerClientFromTenantAsync(
            ITenant tenant,
            string v2ConfigurationKey,
            string v3ConfigurationKey,
            string? containerName,
            BlobClientOptions? blobClientOptions,
            CancellationToken cancellationToken)
        {
            bool v3ConfigWasAvailable = false;
            PublicAccessType? publicAccessType = null;
            if (tenant.Properties.TryGet(v3ConfigurationKey, out BlobContainerConfiguration v3Configuration))
            {
                v3ConfigWasAvailable = true;

                if (v3Configuration.Container is null)
                {
                    if (string.IsNullOrEmpty(containerName))
                    {
                        throw new InvalidOperationException($"When the configuration does not specify a Container, you must supply a {containerName}");
                    }

                    v3Configuration = v3Configuration with
                    {
#pragma warning disable SA1101 // Prefix local calls with this - StyleCop is confused
                        Container = AzureStorageBlobContainerNaming.HashAndEncodeBlobContainerName(containerName),
#pragma warning restore SA1101
                    };
                }
            }
            else if (tenant.Properties.TryGet(v2ConfigurationKey, out LegacyV2BlobStorageConfiguration legacyConfiguration))
            {
                v3Configuration = MakeV3Configuration(tenant, containerName, legacyConfiguration);
                if (v3Configuration.Container == null)
                {
                    throw new InvalidOperationException($"When the configuration does not specify a Container, you must supply a {containerName}");
                }

                publicAccessType = legacyConfiguration.AccessType switch
                {
                    LegacyV2BlobContainerPublicAccessType.Blob => PublicAccessType.Blob,
                    LegacyV2BlobContainerPublicAccessType.Container => PublicAccessType.BlobContainer,
                    _ => PublicAccessType.None,
                };
            }
            else
            {
                throw new InvalidOperationException("Nope");
            }

            BlobContainerClient result = await this.blobContainerSource.GetStorageContextAsync(
                v3Configuration,
                blobClientOptions,
                cancellationToken)
                .ConfigureAwait(false);

            // If the settings say to create a new V3 config if there wasn't already one, it's important
            // that we don't do this until after successfully creating the container, because in the world
            // of V3, apps are supposed to create containers before they create the relevant configuration.
            // (There's a wrinkle here: if an application is using multiple logical containers per
            // tenant, then what are they supposed to do? Should we offer a "create all the containers"
            // option?)
            if (!v3ConfigWasAvailable)
            {
                if (publicAccessType.HasValue)
                {
                    await result.CreateIfNotExistsAsync(
                        publicAccessType.Value,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async ValueTask<BlobContainerConfiguration?> MigrateToV3Async(
            ITenant tenant,
            string v2ConfigurationKey,
            string v3ConfigurationKey,
            IEnumerable<string>? containerNames,
            BlobClientOptions? blobClientOptions,
            CancellationToken cancellationToken)
        {
            if (tenant.Properties.TryGet(v3ConfigurationKey, out BlobContainerConfiguration _))
            {
                return null;
            }

            if (!tenant.Properties.TryGet(v2ConfigurationKey, out LegacyV2BlobStorageConfiguration legacyConfiguration))
            {
                throw new InvalidOperationException("Nope");
            }

            if (containerNames == null)
            {
                if (legacyConfiguration.Container is string containerNameFromConfig)
                {
                    containerNames = new[] { containerNameFromConfig };
                }
                else
                {
                    throw new InvalidOperationException($"When the configuration does not specify a Container, you must supply a non-null {nameof(containerNames)}");
                }
            }

            string? logicalContainerName = null;
            int containerCount = 0;
            foreach (string rawContainerName in containerNames)
            {
                containerCount += 1;
                logicalContainerName = rawContainerName;

                // BlobContainerConfiguration thisConfig = GetConfigForContainer(tenant, rawContainerName, v3Configuration);
                BlobContainerConfiguration thisConfig = MakeV3Configuration(tenant, rawContainerName, legacyConfiguration);
                PublicAccessType publicAccessType = legacyConfiguration.AccessType switch
                {
                    LegacyV2BlobContainerPublicAccessType.Blob => PublicAccessType.Blob,
                    LegacyV2BlobContainerPublicAccessType.Container => PublicAccessType.BlobContainer,
                    _ => PublicAccessType.None,
                };

                BlobContainerClient result = await this.blobContainerSource.GetStorageContextAsync(
                    thisConfig,
                    blobClientOptions,
                    cancellationToken)
                    .ConfigureAwait(false);
                await result.CreateIfNotExistsAsync(
                    publicAccessType,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            // In cases where the legacy configuration had no Container property, and we were
            // passed a containerNames containing exactly one name, we can set the Container
            // in the V3 config. But if there were multiple logical container names, we don't
            // want to set the Container in the V3 config because the application is likely
            // plugging in specific container names at runtime.
            if (containerCount > 1)
            {
                logicalContainerName = null;
            }

            return MakeV3Configuration(tenant, logicalContainerName, legacyConfiguration);
        }

        private static BlobContainerConfiguration MakeV3Configuration(
            ITenant tenant,
            string? containerName,
            LegacyV2BlobStorageConfiguration legacyConfiguration)
        {
            return LegacyConfigurationConverter.FromV2ToV3(legacyConfiguration) with
            {
#pragma warning disable SA1101 // Prefix local calls with this - StyleCop is confused
                Container = string.IsNullOrWhiteSpace(legacyConfiguration.Container)
#pragma warning restore SA1101
                        ? containerName is null ? null : AzureStorageBlobContainerNaming.HashAndEncodeBlobContainerName(containerName)
                        : AzureStorageBlobContainerNaming.HashAndEncodeBlobContainerName(
                            legacyConfiguration.DisableTenantIdPrefix
                            ? legacyConfiguration.Container
                            : AzureStorageBlobTenantedContainerNaming.GetTenantedLogicalBlobContainerNameFor(tenant, legacyConfiguration.Container)),
            };
        }
    }
}