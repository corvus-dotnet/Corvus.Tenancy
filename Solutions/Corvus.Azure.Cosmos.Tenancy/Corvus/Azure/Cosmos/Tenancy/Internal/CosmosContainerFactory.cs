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
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for a <see cref="Container"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="Container"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant via <see cref="CosmosStorageTenantExtensions.AddCosmosConfiguration(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{string, object}}, CosmosContainerDefinition, CosmosConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the Cosmos container factory and the configuration account key provider in your container configuration (assuming you have added a standard ConfigurationRoot to your solution).
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantCosmosContainerFactory();
    /// serviceCollection.AddTenantConfigurationAccountKeyProvider();
    /// </code>
    /// <para>
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="TenancyCosmosServiceCollectionExtensions.AddTenantCosmosContainerFactory(IServiceCollection, TenantCosmosContainerFactoryOptions)"/>.
    /// </para>
    /// <para>
    /// Now, whenever you want to obtain a Cosmos container for a tenant, you simply call <see cref="ITenantCosmosContainerFactory.GetContainerForTenantAsync(ITenant, CosmosContainerDefinition)"/>, passing
    /// it the tenant and the container definition you want to use.
    /// </para>
    /// <para>
    /// <code>
    /// TenantCosmosContainerFactory factory;
    ///
    /// var repository = await factory.GetComosContainerForTenantAsync(tenantProvider.Root, new CosmosContainerDefinition("somecontainer"));
    /// </code>
    /// </para>
    /// <para>
    /// If you create containers in this way (rather than just newing them up) then your application can easily be multitented
    /// by ensuring that you always pass the Tenant through your stack, and just default to tenantProvider.Root at the top level.
    /// </para>
    /// <para>
    /// Note also that because we have not wrapped the resulting Container in a class of our own, we cannot automatically
    /// implement key rotation.
    /// </para>
    /// </remarks>
    internal class CosmosContainerFactory :
        CachingStorageContextFactory<Container, CosmosConfiguration>
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
            IStorageContextScope<CosmosConfiguration> scope,
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