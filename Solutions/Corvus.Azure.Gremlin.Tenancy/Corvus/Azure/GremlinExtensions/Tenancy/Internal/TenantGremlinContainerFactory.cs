// <copyright file="TenantGremlinContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy.Internal
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Corvus.Tenancy;
    using Gremlin.Net.Driver;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for a <see cref="GremlinClient"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="GremlinClient"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant via the <see cref="GremlinStorageTenantExtensions.SetDefaultGremlinConfiguration(ITenant, GremlinConfiguration)"/>
    /// and <see cref="GremlinStorageTenantExtensions.SetGremlinConfiguration(ITenant, GremlinContainerDefinition, GremlinConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and a default configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the Gremlin container factory and the configuration account key provider in your container configuration (assuming you have added a standard ConfigurationRoot to your solution).
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantGremlinContainerFactory();
    /// serviceCollection.AddTenantConfigurationAccountKeyProvider();
    /// </code>
    /// <para>
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="TenancyGremlinServiceCollectionExtensions.AddTenantGremlinContainerFactory(Microsoft.Extensions.DependencyInjection.IServiceCollection, Microsoft.Extensions.Configuration.IConfiguration)"/>.
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
    public class TenantGremlinContainerFactory : ITenantGremlinContainerFactory
    {
        private const string DevelopmentHostName = "https://localhost";
        private const int DevelopmentPort = 8081;
        private const string DevelopmentAuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private readonly ConcurrentDictionary<object, Task<GremlinServer>> servers = new ConcurrentDictionary<object, Task<GremlinServer>>();
        private readonly ConcurrentDictionary<object, Task<GremlinClient>> clients = new ConcurrentDictionary<object, Task<GremlinClient>>();
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantGremlinContainerFactory"/> class.
        /// </summary>
        /// <param name="configuration">The current configuration. Used to determine the connection
        /// string to use when requesting an access token for KeyVault.</param>
        public TenantGremlinContainerFactory(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
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
        public Task<GremlinClient> GetClientForTenantAsync(ITenant tenant, GremlinContainerDefinition containerDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(containerDefinition));
            }

            GremlinContainerDefinition tenantedBlobStorageContainerDefinition = TenantGremlinContainerFactory.GetContainerDefinitionForTenant(tenant, containerDefinition);
            object key = GetKeyFor(tenantedBlobStorageContainerDefinition);

            return this.clients.GetOrAdd(
                key,
                async _ => await this.CreateTenantGremlinClient(tenant, containerDefinition, tenantedBlobStorageContainerDefinition).ConfigureAwait(false));
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

            string tenantedDatabaseName = string.IsNullOrWhiteSpace(configuration.DatabaseName)
                ? tenantedGremlinContainerDefinition.DatabaseName
                : (configuration.DisableTenantIdPrefix
                    ? configuration.DatabaseName
                    : BuildTenantSpecificDatabaseName(tenant, configuration.DatabaseName));

            string tenantedContainerName = string.IsNullOrWhiteSpace(configuration.ContainerName)
                ? tenantedGremlinContainerDefinition.ContainerName
                : (configuration.DisableTenantIdPrefix
                    ? configuration.ContainerName
                    : BuildTenantSpecificContainerName(tenant, configuration.ContainerName));

            // Get the Gremlin client for the specified configuration.
            object accountCacheKey = GetKeyFor(configuration);

            GremlinServer gremlinServer = await this.servers.GetOrAdd(
                accountCacheKey,
                _ => this.CreateTenantGremlinServerAsync(tenant, configuration)).ConfigureAwait(false);

            // TODO: do we need to create the database/container?
            return new GremlinClient(gremlinServer, new GraphSONJTokenReader(), mimeType: GremlinClient.GraphSON2MimeType);
        }

        private static string BuildTenantSpecificContainerName(ITenant tenant, string container) => $"{tenant.Id.ToLowerInvariant()}-{container}";

        private static string BuildTenantSpecificDatabaseName(ITenant tenant, string database) => $"{tenant.Id.ToLowerInvariant()}-{database}";

        private static string BuildTenantSpecificUserName(ITenant tenant, GremlinConfiguration configuration) => $"/dbs/{BuildTenantSpecificDatabaseName(tenant, configuration.DatabaseName)}/colls/{BuildTenantSpecificContainerName(tenant, configuration.DatabaseName)}";

        private async Task<GremlinServer> CreateTenantGremlinServerAsync(ITenant tenant, GremlinConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.HostName) || configuration.HostName == DevelopmentHostName)
            {
                return new GremlinServer(DevelopmentHostName, DevelopmentPort, true, BuildTenantSpecificUserName(tenant, configuration), DevelopmentAuthKey);
            }
            else
            {
                string authKey = await this.GetAuthKey(configuration).ConfigureAwait(false);
                return new GremlinServer(configuration.HostName, configuration.Port, true, BuildTenantSpecificUserName(tenant, configuration), authKey);
            }
        }

        private async Task<string> GetAuthKey(GremlinConfiguration storageConfiguration)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider(this.configuration["AzureServicesAuthConnectionString"]);
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            SecretBundle authKey = await keyVaultClient.GetSecretAsync($"https://{storageConfiguration.KeyVaultName}.vault.azure.net/secrets/{storageConfiguration.AuthKeySecretName}").ConfigureAwait(false);
            return authKey.Value;
        }

        private async Task<GremlinClient> CreateTenantGremlinClient(ITenant tenant, GremlinContainerDefinition repositoryDefinition, GremlinContainerDefinition tenantedBlobStorageContainerDefinition)
        {
            GremlinConfiguration configuration = tenant.GetGremlinConfiguration(repositoryDefinition);

            return await this.CreateGremlinClientInstanceAsync(tenant, tenantedBlobStorageContainerDefinition, configuration).ConfigureAwait(false);
        }
    }
}