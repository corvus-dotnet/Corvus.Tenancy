// <copyright file="TenantCosmosContainerFactory.cs" company="Endjin Limited">
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
    internal class TenantCosmosContainerFactory :
        TenantStorageFactory<Container, CosmosContainerDefinition, CosmosConfiguration>,
        ITenantCosmosContainerFactory
    {
        private const string DevelopmentStorageConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private readonly ConcurrentDictionary<string, Task<CosmosClient>> clients = new ConcurrentDictionary<string, Task<CosmosClient>>();
        private readonly TenantCosmosContainerFactoryOptions? options;
        private readonly ICosmosClientBuilderFactory cosmosClientBuilderFactory;
        private readonly Random random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantCosmosContainerFactory"/> class.
        /// </summary>
        /// <param name="cosmosClientBuilderFactory">Client builder factory.</param>
        /// <param name="options">Configuration for the TenantCosmosContainerFactory.</param>
        public TenantCosmosContainerFactory(ICosmosClientBuilderFactory cosmosClientBuilderFactory, TenantCosmosContainerFactoryOptions? options = null)
        {
            this.cosmosClientBuilderFactory = cosmosClientBuilderFactory ?? throw new ArgumentNullException(nameof(cosmosClientBuilderFactory));
            this.options = options;
        }

        /// <inheritdoc/>
        protected override async Task<Container> CreateContainerAsync(
            ITenant tenant,
            CosmosContainerDefinition tenantedCosmosContainerDefinition,
            CosmosConfiguration configuration)
        {
            // Note: the use of the null forgiving operator in the next two statements is necessary
            // only for as long as we target .NET Standard 2.0. It does not have the NotNullWhen
            // attribute on string.IsNullOrWhiteSpace, meaning the compiler cannot infer that
            // configuration.DatabaseName will be non-null in the 2nd half of these conditional
            // statements. These kinds of attributes are present in .NET Core 3.0 or later, and
            // .NET Standard 2.1 or later, so these assertions will become redundant once we
            // drop support for .NET Standard 2.0.
            string tenantedDatabaseName = string.IsNullOrWhiteSpace(configuration.DatabaseName)
                ? tenantedCosmosContainerDefinition.DatabaseName
                : (configuration.DisableTenantIdPrefix
                    ? configuration.DatabaseName!
                    : BuildTenantSpecificDatabaseName(tenant, configuration.DatabaseName!));

            string tenantedContainerName = string.IsNullOrWhiteSpace(configuration.ContainerName)
                ? tenantedCosmosContainerDefinition.ContainerName
                : (configuration.DisableTenantIdPrefix
                    ? configuration.ContainerName!
                    : BuildTenantSpecificContainerName(tenant, configuration.ContainerName!));

            // Get the Cosmos client for the specified configuration.
            string accountCacheKey = GetCacheKeyForStorageAccount(configuration);
            CosmosClient cosmosClient = await this.clients.GetOrAdd(
                accountCacheKey,
                _ => this.CreateCosmosClientAsync(configuration)).ConfigureAwait(false);

            DatabaseResponse databaseResponse =
                await cosmosClient.CreateDatabaseIfNotExistsAsync(
                    tenantedDatabaseName,
                    tenantedCosmosContainerDefinition.DatabaseThroughput).ConfigureAwait(false);

            ContainerResponse response =
                await databaseResponse.Database.CreateContainerIfNotExistsAsync(
                    tenantedContainerName,
                    string.IsNullOrWhiteSpace(configuration.PartitionKeyPath) ? tenantedCosmosContainerDefinition.PartitionKeyPath : configuration.PartitionKeyPath,
                    tenantedCosmosContainerDefinition.ContainerThroughput).ConfigureAwait(false);

            return response.Container;
        }

        /// <inheritdoc/>
        protected override CosmosContainerDefinition MakeDefinition(
            string tenantSpecificContainerName,
            ITenant tenant,
            CosmosContainerDefinition nonTenantSpecificContainerDefinition)
            => new CosmosContainerDefinition(
                BuildTenantSpecificDatabaseName(tenant, nonTenantSpecificContainerDefinition.DatabaseName),
                tenantSpecificContainerName,
                nonTenantSpecificContainerDefinition.PartitionKeyPath,
                nonTenantSpecificContainerDefinition.ContainerThroughput,
                nonTenantSpecificContainerDefinition.DatabaseThroughput);

        /// <inheritdoc/>
        protected override string GetCacheKeyForContainer(CosmosContainerDefinition tenantCosmosContainerDefinition)
            => $"{tenantCosmosContainerDefinition.DatabaseName}__{tenantCosmosContainerDefinition.ContainerName}";

        /// <inheritdoc/>
        protected override string GetContainerName(CosmosContainerDefinition definition)
            => definition.ContainerName;

        /// <inheritdoc/>
        protected override CosmosConfiguration GetConfiguration(ITenant tenant, CosmosContainerDefinition definition)
            => tenant.GetCosmosConfiguration(definition);

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

        private static string BuildTenantSpecificContainerName(ITenant tenant, string container) => $"{tenant.Id.ToLowerInvariant()}-{container}";

        private static string BuildTenantSpecificDatabaseName(ITenant tenant, string database) => $"{tenant.Id.ToLowerInvariant()}-{database}";

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