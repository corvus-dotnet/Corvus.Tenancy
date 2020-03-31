// <copyright file="TenantCosmosContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy.Internal
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Corvus.Extensions.Cosmos;
    using Corvus.Tenancy;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Fluent;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for a <see cref="Container"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="Container"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant via <see cref="CosmosStorageTenantExtensions.SetCosmosConfiguration(ITenant, CosmosContainerDefinition, CosmosConfiguration)"/>.
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
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="TenancyCosmosServiceCollectionExtensions.AddTenantCosmosContainerFactory(Microsoft.Extensions.DependencyInjection.IServiceCollection, TenantCosmosContainerFactoryOptions)"/>.
    /// </para>
    /// <para>
    /// Now, whenever you want to obtain a Cosmos container for a tenant, you simply call <see cref="TenantCosmosContainerFactory.GetContainerForTenantAsync(ITenant, CosmosContainerDefinition)"/>, passing
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
    public class TenantCosmosContainerFactory : ITenantCosmosContainerFactory
    {
        private const string DevelopmentStorageConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private readonly ConcurrentDictionary<object, Task<CosmosClient>> clients = new ConcurrentDictionary<object, Task<CosmosClient>>();
        private readonly ConcurrentDictionary<object, Task<Container>> containers = new ConcurrentDictionary<object, Task<Container>>();
        private readonly TenantCosmosContainerFactoryOptions? options;
        private readonly ICosmosClientBuilderFactory cosmosClientBuilderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantCosmosContainerFactory"/> class.
        /// </summary>
        /// <param name="cosmosClientBuilderFactory">Client builder factory.</param>
        /// <param name="options">Configuration for the TenantCosmosContainerFactory.</param>
        public TenantCosmosContainerFactory(ICosmosClientBuilderFactory cosmosClientBuilderFactory, TenantCosmosContainerFactoryOptions? options = null)
        {
            this.cosmosClientBuilderFactory = cosmosClientBuilderFactory ?? throw new System.ArgumentNullException(nameof(cosmosClientBuilderFactory));
            this.options = options;
        }

        /// <summary>
        /// Creates a tenant-specific version of a storage container definition.
        /// </summary>
        /// <param name="tenant">The tenant for which to build the definition.</param>
        /// <param name="containerDefinition">The standard single-tenant version of the definition.</param>
        /// <returns>A Cosmos container definition unique to the tenant.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "Temporarily reverting for a proper fix in its own PR. See Issue #65")]
        public static CosmosContainerDefinition GetContainerDefinitionForTenantAsync(ITenant tenant, CosmosContainerDefinition containerDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(containerDefinition));
            }

            return new CosmosContainerDefinition(BuildTenantSpecificDatabaseName(tenant, containerDefinition.DatabaseName), BuildTenantSpecificContainerName(tenant, containerDefinition.ContainerName), containerDefinition.PartitionKeyPath, containerDefinition.ContainerThroughput, containerDefinition.DatabaseThroughput);
        }

        /// <summary>
        /// Gets the cache key for a tenant Cosmos container.
        /// </summary>
        /// <param name="tenantCosmosContainerDefinition">The definition of the tenant Cosmos container.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(CosmosContainerDefinition tenantCosmosContainerDefinition)
        {
            if (tenantCosmosContainerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(tenantCosmosContainerDefinition));
            }

            return $"{tenantCosmosContainerDefinition.DatabaseName}__{tenantCosmosContainerDefinition.ContainerName}";
        }

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(CosmosConfiguration storageConfiguration)
        {
            if (storageConfiguration is null)
            {
                throw new System.ArgumentNullException(nameof(storageConfiguration));
            }

            return string.IsNullOrEmpty(storageConfiguration.AccountUri) ? "storageConfiguration-developmentStorage" : $"storageConfiguration-{storageConfiguration.AccountUri}";
        }

        /// <summary>
        /// Get a Cosmos container for a tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to retrieve the container.</param>
        /// <param name="containerDefinition">The details of the container to create.</param>
        /// <returns>The container instance for the tenant.</returns>
        /// <remarks>
        /// This caches container instances to ensure that a singleton is used for all request for the same tenant and container definition.
        /// </remarks>
        public Task<Container> GetContainerForTenantAsync(ITenant tenant, CosmosContainerDefinition containerDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(containerDefinition));
            }

            CosmosContainerDefinition tenantedCosmosContainerDefinition = GetContainerDefinitionForTenantAsync(tenant, containerDefinition);
            object key = GetKeyFor(tenantedCosmosContainerDefinition);

            return this.containers.GetOrAdd(
                key,
                async _ => await this.CreateTenantCosmosContainer(tenant, containerDefinition, tenantedCosmosContainerDefinition).ConfigureAwait(false));
        }

        /// <summary>
        /// Create the repository instance.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="tenantedCosmosContainerDefinition">The container definition, adapted for the tenant.</param>
        /// <param name="configuration">The Cosmos configuration.</param>
        /// <returns>A <see cref="Task"/> with completes with the instance of the document repository for the tenant.</returns>
        protected async Task<Container> CreateCosmosContainerInstanceAsync(ITenant tenant, CosmosContainerDefinition tenantedCosmosContainerDefinition, CosmosConfiguration configuration)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (tenantedCosmosContainerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(tenantedCosmosContainerDefinition));
            }

            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

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
            object accountCacheKey = GetKeyFor(configuration);

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
                string accountKey = await this.GetAccountKeyAsync(configuration).ConfigureAwait(false);
                builder = this.cosmosClientBuilderFactory.CreateCosmosClientBuilder(configuration.AccountUri, accountKey);
            }

            // TODO: do we want to change any defaults?
            return builder.Build();
        }

        private async Task<string> GetAccountKeyAsync(CosmosConfiguration storageConfiguration)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider(this.options?.AzureServicesAuthConnectionString);
            using var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            SecretBundle accountKey = await keyVaultClient.GetSecretAsync($"https://{storageConfiguration.KeyVaultName}.vault.azure.net/secrets/{storageConfiguration.AccountKeySecretName}").ConfigureAwait(false);
            return accountKey.Value;
        }

        private async Task<Container> CreateTenantCosmosContainer(ITenant tenant, CosmosContainerDefinition repositoryDefinition, CosmosContainerDefinition tenantedCosmosContainerDefinition)
        {
            CosmosConfiguration? configuration = tenant.GetCosmosConfiguration(repositoryDefinition);

            // Although GetCosmosConfiguration can return null, CreateCosmosContainerInstanceAsync
            // will detect that and throw, so it's OK to silence the compiler warning with the null
            // forgiving operator here.
            return await this.CreateCosmosContainerInstanceAsync(tenant, tenantedCosmosContainerDefinition, configuration!).ConfigureAwait(false);
        }
    }
}