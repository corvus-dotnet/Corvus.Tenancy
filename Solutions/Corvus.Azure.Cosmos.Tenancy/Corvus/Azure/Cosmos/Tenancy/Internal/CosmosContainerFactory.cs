// <copyright file="CosmosContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using Corvus.Extensions.Cosmos;
    using Corvus.Tenancy;
    using Corvus.Tenancy.Azure.Common;

    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Fluent;

    /// <summary>
    /// A factory for a <see cref="Container"/>.
    /// </summary>
    internal class CosmosContainerFactory : CachingStorageContextFactory<Container, CosmosConfiguration>
    {
        private const string DevelopmentStorageConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private readonly ConcurrentDictionary<string, Task<CosmosClient>> clients = new ConcurrentDictionary<string, Task<CosmosClient>>();
        private readonly TenantCosmosContainerFactoryOptions? options;
        private readonly ICosmosClientBuilderFactory cosmosClientBuilderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosContainerFactory"/> class.
        /// </summary>
        /// <param name="cosmosClientBuilderFactory">Client builder factory.</param>
        /// <param name="options">Configuration for the TenantCosmosContainerFactory.</param>
        public CosmosContainerFactory(ICosmosClientBuilderFactory cosmosClientBuilderFactory, TenantCosmosContainerFactoryOptions? options = null)
        {
            this.cosmosClientBuilderFactory = cosmosClientBuilderFactory ?? throw new ArgumentNullException(nameof(cosmosClientBuilderFactory));
            this.options = options;
        }

        /// <inheritdoc/>
        protected override async Task<Container> CreateContainerAsync(
            string contextName,
            CosmosConfiguration configuration)
        {
            // Get the Cosmos client for the specified configuration.
            string accountCacheKey = GetCacheKeyForStorageAccount(configuration);
            CosmosClient cosmosClient = await this.clients.GetOrAdd(
                accountCacheKey,
                _ => this.CreateCosmosClientAsync(configuration)).ConfigureAwait(false);

            Database database = cosmosClient.GetDatabase(configuration.DatabaseName);
            Container container = database.GetContainer(configuration.ContainerName);

            return container;
        }

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        private static string GetCacheKeyForStorageAccount(CosmosConfiguration storageConfiguration)
        {
            if (storageConfiguration is null)
            {
                throw new ArgumentNullException(nameof(storageConfiguration));
            }

            return string.IsNullOrEmpty(storageConfiguration.AccountUri) ? "storageConfiguration-developmentStorage" : $"storageConfiguration-{storageConfiguration.AccountUri}";
        }

        private async Task<CosmosClient> CreateCosmosClientAsync(CosmosConfiguration configuration)
        {
            CosmosClientBuilder builder;

            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            if (string.IsNullOrEmpty(configuration.AccountUri) || configuration.AccountUri!.Equals(DevelopmentStorageConnectionString))
            {
                builder = this.cosmosClientBuilderFactory.CreateCosmosClientBuilder(DevelopmentStorageConnectionString);
            }
            else if (string.IsNullOrEmpty(configuration.AccountKeySecretName))
            {
                builder = this.cosmosClientBuilderFactory.CreateCosmosClientBuilder(configuration.AccountUri);
            }
            else
            {
                string accountKey = await this.GetKeyVaultSecretAsync(
                    this.options?.AzureServicesAuthConnectionString,
                    configuration.KeyVaultName!,
                    configuration.AccountKeySecretName!).ConfigureAwait(false);
                builder = this.cosmosClientBuilderFactory.CreateCosmosClientBuilder(configuration.AccountUri, accountKey);
            }

            // TODO: do we want to change any defaults?
            return builder.Build();
        }
    }
}