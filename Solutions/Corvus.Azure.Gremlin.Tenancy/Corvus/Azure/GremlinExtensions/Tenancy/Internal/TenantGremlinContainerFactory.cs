// <copyright file="TenantGremlinContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using Corvus.Tenancy;

    using global::Azure.Security.KeyVault.Secrets;

    using Gremlin.Net.Driver;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for a <see cref="GremlinClient"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="GremlinClient"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant via <see cref="GremlinStorageTenantExtensions.AddGremlinConfiguration(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{string, object}}, GremlinContainerDefinition, GremlinConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the Gremlin container factory and the configuration account key provider in your container configuration (assuming you have added a standard ConfigurationRoot to your solution).
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantGremlinContainerFactory();
    /// serviceCollection.AddTenantConfigurationAccountKeyProvider();
    /// </code>
    /// <para>
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="TenancyGremlinServiceCollectionExtensions.AddTenantGremlinContainerFactory(Microsoft.Extensions.DependencyInjection.IServiceCollection, TenantGremlinContainerFactoryOptions)"/>.
    /// </para>
    /// <para>
    /// Now, whenever you want to obtain a Gremlin container for a tenant, you simply call <see cref="TenantGremlinContainerFactory.GetClientForTenantAsync(ITenant, GremlinContainerDefinition)"/>, passing
    /// it the tenant and the container definition you want to use.
    /// </para>
    /// <para>
    /// <code>
    /// TenantCloudBlobContainerFactory factory;
    ///
    /// var repository = await factory.GetBlobContainerForTenantAsync(tenantProvider.Root, new BlobStorageContainerDefinition("somecontainer"));
    /// </code>
    /// </para>
    /// <para>
    /// If you create containers in this way (rather than just newing them up) then your application can easily be multitented
    /// by ensuring that you always pass the Tenant through your stack, and just default to tenantProvider.Root at the top level.
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
    internal class TenantGremlinContainerFactory : ITenantGremlinContainerFactory
    {
        private const string DevelopmentHostName = "localhost";
        private const int DevelopmentPort = 8901;
        private const string DevelopmentAuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private readonly ConcurrentDictionary<object, Task<GremlinServer>> servers = new();
        private readonly ConcurrentDictionary<object, Task<GremlinClient>> clients = new();
        private readonly TenantGremlinContainerFactoryOptions options;
        private readonly Random random = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantGremlinContainerFactory"/> class.
        /// </summary>
        /// <param name="options">Configuration for the TenantGremlinContainerFactory.</param>
        public TenantGremlinContainerFactory(TenantGremlinContainerFactoryOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Creates a tenant-specific version of a storage container definition.
        /// </summary>
        /// <param name="tenant">The tenant for which to build the definition.</param>
        /// <param name="containerDefinition">The standard single-tenant version of the definition.</param>
        /// <returns>A blob container definition unique to the tenant.</returns>
        public static GremlinContainerDefinition GetContainerDefinitionForTenant(ITenant tenant, GremlinContainerDefinition containerDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(containerDefinition));
            }

            return new GremlinContainerDefinition(BuildTenantSpecificDatabaseName(tenant, containerDefinition.DatabaseName), BuildTenantSpecificContainerName(tenant, containerDefinition.ContainerName));
        }

        /// <summary>
        /// Gets the cache key for a tenant blob container.
        /// </summary>
        /// <param name="tenantBlobStorageContainerDefinition">The definition of the tenant blob container.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(GremlinContainerDefinition tenantBlobStorageContainerDefinition)
        {
            if (tenantBlobStorageContainerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(tenantBlobStorageContainerDefinition));
            }

            return $"{tenantBlobStorageContainerDefinition.DatabaseName}__{tenantBlobStorageContainerDefinition.ContainerName}";
        }

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(GremlinConfiguration storageConfiguration)
        {
            if (storageConfiguration is null)
            {
                throw new System.ArgumentNullException(nameof(storageConfiguration));
            }

            return string.IsNullOrEmpty(storageConfiguration.HostName) ? "storageConfiguration-developmentStorage" : $"storageConfiguration-{storageConfiguration.HostName}";
        }

        /// <summary>
        /// Get a gremlin client for a tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to retrieve the container.</param>
        /// <param name="containerDefinition">The details of the container to create.</param>
        /// <returns>The container instance for the tenant.</returns>
        /// <remarks>
        /// This caches container instances to ensure that a singleton is used for all request for the same tenant and container definition.
        /// </remarks>
        public async Task<GremlinClient> GetClientForTenantAsync(ITenant tenant, GremlinContainerDefinition containerDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(containerDefinition));
            }

            GremlinContainerDefinition tenantedContainerDefinition = TenantGremlinContainerFactory.GetContainerDefinitionForTenant(tenant, containerDefinition);
            object key = GetKeyFor(tenantedContainerDefinition);

            Task<GremlinClient> result = this.clients.GetOrAdd(
                key,
                _ => this.CreateTenantGremlinClient(tenant, containerDefinition, tenantedContainerDefinition));

            if (result.IsFaulted)
            {
                // If a task has been created in the previous statement, it won't have completed yet. Therefore if it's
                // faulted, that means it was added as part of a previous request to this method, and subsequently
                // failed. As such, we will remove the item from the dictionary, and attempt to create a new one to
                // return. If removing the value fails, that's likely because it's been removed by a different thread,
                // so we will ignore that and just attempt to create and return a new value anyway.
                this.clients.TryRemove(key, out Task<GremlinClient>? _);

                // Wait for a short and random time, to reduce the potential for large numbers of spurious container
                // recreation that could happen if multiple threads are trying to rectify the failure simultanously.
                await Task.Delay(this.random.Next(150, 250)).ConfigureAwait(false);

                result = this.clients.GetOrAdd(
                    key,
                    _ => this.CreateTenantGremlinClient(tenant, containerDefinition, tenantedContainerDefinition));
            }

            return await result.ConfigureAwait(false);
        }

        /// <summary>
        /// Create the repository instance.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="tenantedGremlinContainerDefinition">The container definition, adapted for the tenant.</param>
        /// <param name="configuration">The Gremlin configuration.</param>
        /// <returns>A <see cref="Task"/> with completes with the instance of the document repository for the tenant.</returns>
        protected async Task<GremlinClient> CreateGremlinClientInstanceAsync(ITenant tenant, GremlinContainerDefinition tenantedGremlinContainerDefinition, GremlinConfiguration configuration)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (tenantedGremlinContainerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(tenantedGremlinContainerDefinition));
            }

            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            string tenantedDatabaseName = string.IsNullOrWhiteSpace(configuration.DatabaseName)
                ? tenantedGremlinContainerDefinition.DatabaseName
                : (configuration.DisableTenantIdPrefix
                    ? configuration.DatabaseName!
                    : BuildTenantSpecificDatabaseName(tenant, configuration.DatabaseName!));

            string tenantedContainerName = string.IsNullOrWhiteSpace(configuration.ContainerName)
                ? tenantedGremlinContainerDefinition.ContainerName
                : (configuration.DisableTenantIdPrefix
                    ? configuration.ContainerName!
                    : BuildTenantSpecificContainerName(tenant, configuration.ContainerName!));

            // Get the Gremlin client for the specified configuration.
            object accountCacheKey = GetKeyFor(configuration);

            GremlinServer gremlinServer = await this.servers.GetOrAdd(
                accountCacheKey,
                _ => this.CreateTenantGremlinServerAsync(configuration, tenantedDatabaseName, tenantedContainerName)).ConfigureAwait(false);

            // TODO: do we need to create the database/container?
            return new GremlinClient(gremlinServer, new GraphSONJTokenReader(), mimeType: GremlinClient.GraphSON2MimeType);
        }

        private static string BuildTenantSpecificContainerName(ITenant tenant, string container) => $"{tenant.Id.ToLowerInvariant()}-{container}";

        private static string BuildTenantSpecificDatabaseName(ITenant tenant, string database) => $"{tenant.Id.ToLowerInvariant()}-{database}";

        private static string BuildTenantSpecificUserName(string databaseName, string containerName) => $"/dbs/{databaseName}/colls/{containerName}";

        private async Task<GremlinServer> CreateTenantGremlinServerAsync(GremlinConfiguration configuration, string databaseName, string containerName)
        {
            string username = BuildTenantSpecificUserName(databaseName, containerName);
            if (string.IsNullOrEmpty(configuration.HostName) || configuration.HostName == DevelopmentHostName)
            {
                return new GremlinServer(DevelopmentHostName, DevelopmentPort, false, username, DevelopmentAuthKey);
            }
            else
            {
                string authKey = await this.GetAuthKey(configuration).ConfigureAwait(false);
                return new GremlinServer(configuration.HostName, configuration.Port, true, username, authKey);
            }
        }

        private async Task<string> GetAuthKey(GremlinConfiguration storageConfiguration)
        {
            var keyVaultCredentials = Identity.ClientAuthentication.Azure.LegacyAzureServiceTokenProviderConnectionString.ToTokenCredential(this.options?.AzureServicesAuthConnectionString!);

            var keyVaultUri = new Uri($"https://{storageConfiguration.KeyVaultName}.vault.azure.net/");
            var keyVaultClient = new SecretClient(keyVaultUri, keyVaultCredentials);

            global::Azure.Response<KeyVaultSecret>? accountKeyResponse = await keyVaultClient.GetSecretAsync(storageConfiguration.AuthKeySecretName).ConfigureAwait(false);

            return accountKeyResponse.Value.Value;
        }

        private async Task<GremlinClient> CreateTenantGremlinClient(ITenant tenant, GremlinContainerDefinition repositoryDefinition, GremlinContainerDefinition tenantedBlobStorageContainerDefinition)
        {
            GremlinConfiguration? configuration = tenant.GetGremlinConfiguration(repositoryDefinition);

            // Although GetGremlinConfiguration can return null, CreateGremlinClientInstanceAsync
            // will detect that and throw, so it's OK to silence the compiler warning with the null
            // forgiving operator here.
            return await this.CreateGremlinClientInstanceAsync(tenant, tenantedBlobStorageContainerDefinition, configuration!).ConfigureAwait(false);
        }
    }
}