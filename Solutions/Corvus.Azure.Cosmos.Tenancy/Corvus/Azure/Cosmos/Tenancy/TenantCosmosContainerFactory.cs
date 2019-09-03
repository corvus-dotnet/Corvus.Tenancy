﻿// <copyright file="TenantCosmosContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Corvus.Tenancy;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for a <see cref="Container"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="Container"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant via the <see cref="CosmosStorageTenantExtensions.SetDefaultCosmosConfiguration(ITenant, ICosmosConfiguration)"/>
    /// and <see cref="CosmosStorageTenantExtensions.SetCosmosConfiguration(ITenant, CosmosContainerDefinition, ICosmosConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and a default configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the Cosmos container factory and the configuration account key provider in your container configuration (assuming you have added a standard ConfigurationRoot to your solution).
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantCosmosContainerFactory();
    /// serviceCollection.AddTenantConfigurationAccountKeyProvider();
    /// </code>
    /// <para>
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="TenancyCosmosServiceCollectionExtensions.AddTenantCosmosContainerFactory(Microsoft.Extensions.DependencyInjection.IServiceCollection, Microsoft.Extensions.Configuration.IConfiguration)"/>.
    /// </para>
    /// <para>
    /// Now, whenever you want to obtain a Cosmos container for a tenant, you simply call <see cref="TenantCosmosContainerFactory.GetContainerForTenantAsync(ITenant, CosmosContainerDefinition)"/>, passing
    /// it the tenant and the container definition you want to use.
    /// </para>
    /// <para>
    /// <code>
    /// TenantCloudBlobContainerFactory factory;
    ///
    /// var repository = await factory.GetBlobContainerForTenantAsync(Tenant.Root, new BlobStorageContainerDefinition("somecontainer"));
    /// </code>
    /// </para>
    /// <para>
    /// If you create containers in this way (rather than just newing them up) then your application can easily be multitented
    /// by ensuring that you always pass the Tenant through your stack, and just default to Tenant.Root at the top level.
    /// </para>
    /// <para>
    /// Note that it will be possible for code that obtains a CloudBlobContainer in this way to use the resulting object to access
    /// the CloudBlobClient and thus access other blob contains in the same container. As such these objects should only ever be
    /// handed to trusted code.
    /// </para>
    /// <para>
    /// Note also that because we have not wrapped the resulting CloudBlobContainer in a class of our own, we cannot automatically
    /// implement key rotation.
    /// </para>
    /// </remarks>
    public class TenantCosmosContainerFactory : ITenantCosmosContainerFactory
    {
        private const string DevelopmentStorageConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private readonly ConcurrentDictionary<object, Task<CosmosClient>> clients = new ConcurrentDictionary<object, Task<CosmosClient>>();
        private readonly ConcurrentDictionary<object, Task<Container>> containers = new ConcurrentDictionary<object, Task<Container>>();
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantCosmosContainerFactory"/> class.
        /// </summary>
        /// <param name="configuration">The current configuration. Used to determine the connection
        /// string to use when requesting an access token for KeyVault.</param>
        public TenantCosmosContainerFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Creates a tenant-specific version of a storage container definition.
        /// </summary>
        /// <param name="tenant">The tenant for which to build the definition.</param>
        /// <param name="containerDefinition">The standard single-tenant version of the definition.</param>
        /// <returns>A blob container definition unique to the tenant.</returns>
        public static CosmosContainerDefinition GetContainerDefinitionForTenantAsync(ITenant tenant, CosmosContainerDefinition containerDefinition)
        {
            return new CosmosContainerDefinition(BuildTenantSpecificDatabaseName(tenant, containerDefinition.DatabaseName), BuildTenantSpecificContainerName(tenant, containerDefinition.ContainerName), containerDefinition.PartitionKeyPath, containerDefinition.ContainerThroughput, containerDefinition.DatabaseThroughput);
        }

        /// <summary>
        /// Gets the cache key for a tenant blob container.
        /// </summary>
        /// <param name="tenantBlobStorageContainerDefinition">The definition of the tenant blob container.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(CosmosContainerDefinition tenantBlobStorageContainerDefinition)
        {
            return $"{tenantBlobStorageContainerDefinition.DatabaseName}__{tenantBlobStorageContainerDefinition.ContainerName}";
        }

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(ICosmosConfiguration storageConfiguration)
        {
            return string.IsNullOrEmpty(storageConfiguration.AccountUri) ? "storageConfiguration-developmentStorage" : $"storageConfiguration-{storageConfiguration.AccountUri}";
        }

        /// <summary>
        /// Get a blob container for a tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to retrieve the container.</param>
        /// <param name="containerDefinition">The details of the container to create.</param>
        /// <returns>The container instance for the tenant.</returns>
        /// <remarks>
        /// This caches container instances to ensure that a singleton is used for all request for the same tenant and container definition.
        /// </remarks>
        public Task<Container> GetContainerForTenantAsync(ITenant tenant, CosmosContainerDefinition containerDefinition)
        {
            CosmosContainerDefinition tenantedBlobStorageContainerDefinition = TenantCosmosContainerFactory.GetContainerDefinitionForTenantAsync(tenant, containerDefinition);
            object key = GetKeyFor(tenantedBlobStorageContainerDefinition);

            return this.containers.GetOrAdd(
                key,
                async _ => await this.CreateTenantCosmosContainer(tenant, containerDefinition, tenantedBlobStorageContainerDefinition).ConfigureAwait(false));
        }

        /// <summary>
        /// Create the repository instance.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="tenantedCosmosContainerDefinition">The container definition, adapted for the tenant.</param>
        /// <param name="configuration">The Cosmos configuration.</param>
        /// <returns>A <see cref="Task"/> with completes with the instance of the document repository for the tenant.</returns>
        protected async Task<Container> CreateCosmosContainerInstanceAsync(ITenant tenant, CosmosContainerDefinition tenantedCosmosContainerDefinition, ICosmosConfiguration configuration)
        {
            string tenantedDatabaseName = string.IsNullOrWhiteSpace(configuration.CosmosContainerDefinition.DatabaseName)
                ? tenantedCosmosContainerDefinition.DatabaseName
                : (configuration.GetDisableTenantIdPrefix()
                    ? configuration.CosmosContainerDefinition.DatabaseName
                    : BuildTenantSpecificDatabaseName(tenant, configuration.CosmosContainerDefinition.DatabaseName));

            string tenantedContainerName = string.IsNullOrWhiteSpace(configuration.CosmosContainerDefinition.ContainerName)
                ? tenantedCosmosContainerDefinition.ContainerName
                : (configuration.GetDisableTenantIdPrefix()
                    ? configuration.CosmosContainerDefinition.ContainerName
                    : BuildTenantSpecificContainerName(tenant, configuration.CosmosContainerDefinition.ContainerName));

            // Get the Cosmos client for the specified configuration.
            object accountCacheKey = GetKeyFor(configuration);

            CosmosClient cosmosClient = await this.clients.GetOrAdd(
                accountCacheKey,
                _ => this.CreateTenantCosmosClientAsync(tenant, configuration)).ConfigureAwait(false);

            DatabaseResponse databaseResponse =
                await cosmosClient.CreateDatabaseIfNotExistsAsync(
                    tenantedDatabaseName,
                    tenantedCosmosContainerDefinition.DatabaseThroughput).ConfigureAwait(false);

            ContainerResponse response =
                await databaseResponse.Database.CreateContainerIfNotExistsAsync(
                    tenantedContainerName,
                    tenantedCosmosContainerDefinition.PartitionKeyPath,
                    tenantedCosmosContainerDefinition.ContainerThroughput).ConfigureAwait(false);

            return response.Container;
        }

        private static string BuildTenantSpecificContainerName(ITenant tenant, string container) => $"{tenant.Id.ToLowerInvariant()}-{container}";

        private static string BuildTenantSpecificDatabaseName(ITenant tenant, string database) => $"{tenant.Id.ToLowerInvariant()}-{database}";

        private async Task<CosmosClient> CreateTenantCosmosClientAsync(ITenant tenant, ICosmosConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.AccountUri) || configuration.AccountUri.Equals(DevelopmentStorageConnectionString))
            {
                account = CloudStorageAccount.DevelopmentStorageAccount;
            }
            else
            {
                string accountKey = await this.GetAccountKeyAsync(tenant, configuration).ConfigureAwait(false);
            }
        }

        private async Task<string> GetAccountKeyAsync(ITenant tenant, ICosmosConfiguration storageConfiguration)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider(this.configuration["AzureServicesAuthConnectionString"]);
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            Microsoft.Azure.KeyVault.Models.SecretBundle accountKey = await keyVaultClient.GetSecretAsync($"https://{tenant.GetKeyVaultName()}.vault.azure.net/secrets/{storageConfiguration.GetStorageAccountKeySecretName()}").ConfigureAwait(false);
            return accountKey.Value;
        }

        private async Task<Container> CreateTenantCosmosContainer(ITenant tenant, CosmosContainerDefinition repositoryDefinition, CosmosContainerDefinition tenantedBlobStorageContainerDefinition)
        {
            ICosmosConfiguration configuration = tenant.GetCosmosConfiguration(repositoryDefinition);

            return await this.CreateCosmosContainerInstanceAsync(tenant, tenantedBlobStorageContainerDefinition, configuration).ConfigureAwait(false);
        }
    }
}