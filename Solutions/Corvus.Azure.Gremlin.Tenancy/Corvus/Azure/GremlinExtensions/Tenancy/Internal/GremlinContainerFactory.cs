// <copyright file="GremlinContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy.Internal
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using Corvus.Tenancy;
    using Corvus.Tenancy.Azure.Common;

    using Gremlin.Net.Driver;

    /// <summary>
    /// A factory for a <see cref="GremlinClient"/>.
    /// </summary>
    internal class GremlinContainerFactory : CachingStorageContextFactory<GremlinClient, GremlinConfiguration>
    {
        private const string DevelopmentHostName = "localhost";
        private const int DevelopmentPort = 8901;
        private const string DevelopmentAuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private readonly ConcurrentDictionary<object, Task<GremlinServer>> servers = new ConcurrentDictionary<object, Task<GremlinServer>>();
        private readonly TenantGremlinContainerFactoryOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="GremlinContainerFactory"/> class.
        /// </summary>
        /// <param name="options">Configuration for the TenantGremlinContainerFactory.</param>
        public GremlinContainerFactory(TenantGremlinContainerFactoryOptions options)
        {
            this.options = options;
        }

        /// <inheritdoc/>
        protected override async Task<GremlinClient> CreateContainerAsync(
            string contextName,
            GremlinConfiguration configuration)
        {
            // Get the Gremlin client for the specified configuration.
            object accountCacheKey = GetCacheKeyForStorageAccount(configuration);

            GremlinServer gremlinServer = await this.servers.GetOrAdd(
                accountCacheKey,
                _ => this.CreateTenantGremlinServerAsync(configuration, configuration.DatabaseName!, configuration.ContainerName!)).ConfigureAwait(false);

            // TODO: do we need to create the database/container?
            return new GremlinClient(gremlinServer, new GraphSONJTokenReader(), mimeType: GremlinClient.GraphSON2MimeType);
        }

        private static string BuildTenantSpecificUserName(string databaseName, string containerName) => $"/dbs/{databaseName}/colls/{containerName}";

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        private static object GetCacheKeyForStorageAccount(GremlinConfiguration storageConfiguration)
        {
            if (storageConfiguration is null)
            {
                throw new System.ArgumentNullException(nameof(storageConfiguration));
            }

            return string.IsNullOrEmpty(storageConfiguration.HostName) ? "storageConfiguration-developmentStorage" : $"storageConfiguration-{storageConfiguration.HostName}";
        }

        private async Task<GremlinServer> CreateTenantGremlinServerAsync(GremlinConfiguration configuration, string databaseName, string containerName)
        {
            string username = BuildTenantSpecificUserName(databaseName, containerName);
            if (string.IsNullOrEmpty(configuration.HostName) || configuration.HostName == DevelopmentHostName)
            {
                return new GremlinServer(DevelopmentHostName, DevelopmentPort, false, username, DevelopmentAuthKey);
            }
            else
            {
                string authKey = await this.GetKeyVaultSecretAsync(
                    this.options?.AzureServicesAuthConnectionString,
                    configuration.KeyVaultName!,
                    configuration.AuthKeySecretName!).ConfigureAwait(false);
                return new GremlinServer(configuration.HostName, configuration.Port, true, username, authKey);
            }
        }
    }
}