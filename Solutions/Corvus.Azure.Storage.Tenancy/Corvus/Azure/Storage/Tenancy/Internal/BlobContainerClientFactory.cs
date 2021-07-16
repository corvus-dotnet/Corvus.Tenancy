// <copyright file="BlobContainerClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using Corvus.Tenancy;
    using Corvus.Tenancy.Azure.Common;

    using global::Azure.Storage;
    using global::Azure.Storage.Blobs;

    /// <summary>
    /// A caching factory for a <see cref="BlobContainerClient"/>.
    /// </summary>
    internal class BlobContainerClientFactory :
        CachingStorageContextFactory<BlobContainerClient, BlobStorageConfiguration>
    {
        private const string DevelopmentStorageConnectionString = "UseDevelopmentStorage=true";

        private readonly ConcurrentDictionary<string, Task<BlobServiceClient>> clients = new ConcurrentDictionary<string, Task<BlobServiceClient>>();
        private readonly TenantBlobContainerClientFactoryOptions? options;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobContainerClientFactory"/> class.
        /// </summary>
        /// <param name="options">Configuration for the TenantBlobContainerClientFactory.</param>
        public BlobContainerClientFactory(TenantBlobContainerClientFactoryOptions? options = null)
        {
            this.options = options;
        }

        /// <inheritdoc/>
        protected override async Task<BlobContainerClient> CreateContainerAsync(
            string contextName,
            BlobStorageConfiguration configuration)
        {
            // Get the cloud blob client for the specified configuration.
            string accountCacheKey = GetCacheKeyForStorageAccount(configuration);

            BlobServiceClient blobClient = await this.clients.GetOrAdd(
                accountCacheKey,
                _ => this.CreateBlockBlobClientAsync(configuration)).ConfigureAwait(false);

            BlobContainerClient container = blobClient.GetBlobContainerClient(configuration.Container);

            return container;
        }

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        private static string GetCacheKeyForStorageAccount(BlobStorageConfiguration storageConfiguration)
        {
            if (storageConfiguration is null)
            {
                throw new System.ArgumentNullException(nameof(storageConfiguration));
            }

            return string.IsNullOrEmpty(storageConfiguration.AccountName) ? "storageConfiguration-developmentStorage" : $"storageConfiguration-{storageConfiguration.AccountName}";
        }

        private async Task<BlobServiceClient> CreateBlockBlobClientAsync(BlobStorageConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            BlobServiceClient client;

            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            if (string.IsNullOrEmpty(configuration.AccountName) || configuration.AccountName!.Equals(DevelopmentStorageConnectionString))
            {
                client = new BlobServiceClient(DevelopmentStorageConnectionString);
            }
            else if (string.IsNullOrWhiteSpace(configuration.AccountKeySecretName))
            {
                // As the documentation for BlobStorageConfiguration.AccountName says:
                //  "If the account key secret name is empty, then this should contain
                //   a complete connection string."
                client = new BlobServiceClient(configuration.AccountName);
            }
            else
            {
                string accountKey = await this.GetKeyVaultSecretAsync(
                    this.options?.AzureServicesAuthConnectionString,
                    configuration.KeyVaultName!,
                    configuration.AccountKeySecretName!).ConfigureAwait(false);
                var credentials = new StorageSharedKeyCredential(configuration.AccountName, accountKey);
                client = new BlobServiceClient(
                    new Uri($"https://{configuration.AccountName}.blob.core.windows.net"),
                    credentials);
            }

            return client;
        }
    }
}
