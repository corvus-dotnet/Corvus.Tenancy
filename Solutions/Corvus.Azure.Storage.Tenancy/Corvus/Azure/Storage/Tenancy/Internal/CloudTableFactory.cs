// <copyright file="CloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using Corvus.Tenancy;
    using Corvus.Tenancy.Azure.Common;

    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// A caching factory for a <see cref="CloudTable"/>.
    /// </summary>
    internal class CloudTableFactory :
        CachingStorageContextFactory<CloudTable, TableStorageConfiguration>
    {
        private const string DevelopmentStorageConnectionString = "UseDevelopmentStorage=true";

        private readonly ConcurrentDictionary<string, Task<CloudTableClient>> clients = new ConcurrentDictionary<string, Task<CloudTableClient>>();
        private readonly TenantCloudTableFactoryOptions? options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudTableFactory"/> class.
        /// </summary>
        /// <param name="options">Configuration for the TenantCloudTableFactory.</param>
        public CloudTableFactory(TenantCloudTableFactoryOptions? options = null)
        {
            this.options = options;
        }

        /// <inheritdoc/>
        protected override async Task<CloudTable> CreateContainerAsync(
            string contextName,
            TableStorageConfiguration configuration)
        {
            // Get the cloud table client for the specified configuration.
            string accountCacheKey = GetCacheKeyForStorageAccount(configuration);

            CloudTableClient tableClient = await this.clients.GetOrAdd(
                accountCacheKey,
                _ => this.CreateCloudTableClientAsync(configuration)).ConfigureAwait(false);

            CloudTable container = tableClient.GetTableReference(configuration.TableName);

            return container;
        }

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        private static string GetCacheKeyForStorageAccount(TableStorageConfiguration storageConfiguration)
        {
            if (storageConfiguration is null)
            {
                throw new ArgumentNullException(nameof(storageConfiguration));
            }

            return string.IsNullOrEmpty(storageConfiguration.AccountName) ? "storageConfiguration-developmentStorage" : $"storageConfiguration-{storageConfiguration.AccountName}";
        }

        private async Task<CloudTableClient> CreateCloudTableClientAsync(TableStorageConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            CloudStorageAccount account;

            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            if (string.IsNullOrEmpty(configuration.AccountName) || configuration.AccountName!.Equals(DevelopmentStorageConnectionString))
            {
                account = CloudStorageAccount.DevelopmentStorageAccount;
            }
            else if (string.IsNullOrWhiteSpace(configuration.AccountKeySecretName))
            {
                account = CloudStorageAccount.Parse(configuration.AccountName);
            }
            else
            {
                string accountKey = await this.GetKeyVaultSecretAsync(
                    this.options?.AzureServicesAuthConnectionString,
                    configuration.KeyVaultName!,
                    configuration.AccountKeySecretName!).ConfigureAwait(false);
                var credentials = new StorageCredentials(configuration.AccountName, accountKey);
                account = new CloudStorageAccount(credentials, configuration.AccountName, null, true);
            }

            return account.CreateCloudTableClient();
        }
    }
}