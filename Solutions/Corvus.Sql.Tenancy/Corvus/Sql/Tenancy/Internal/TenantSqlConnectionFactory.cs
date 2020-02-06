// <copyright file="TenantSqlConnectionFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Sql.Tenancy.Internal
{
    using System.Collections.Concurrent;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Text;
    using System.Threading.Tasks;
    using Corvus.Tenancy;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for a <see cref="Sql"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of a <see cref="SqlConnection"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the connection string for the tenant, and the
    /// configuration comes from the tenant via the <see cref="SqlStorageTenantExtensions.SetDefaultSqlConfiguration(ITenant, SqlConfiguration)"/>
    /// and <see cref="SqlStorageTenantExtensions.SetSqlConfiguration(ITenant, SqlConnectionDefinition, SqlConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and a default configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the Sql container factory and the configuration account key provider in your container configuration (assuming you have added a standard ConfigurationRoot to your solution).
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantSqlContainerFactory();
    /// serviceCollection.AddTenantConfigurationAccountKeyProvider();
    /// </code>
    /// <para>
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="Microsoft.Extensions.DependencyInjection.TenancySqlServiceCollectionExtensions.AddTenantSqlConnectionFactory(IServiceCollection, IConfiguration)"/>.
    /// </para>
    /// <para>
    /// Now, whenever you want to obtain a Sql connection for a tenant, you simply call <see cref="GetSqlConnectionForTenantAsync(ITenant, SqlConnectionDefinition)"/>, passing
    /// it the tenant and the container definition you want to use.
    /// </para>
    /// <para>
    /// <code>
    /// TenantSqlConnectionFactory factory;
    ///
    /// var repository = await factory.GetSqlConnectionForTenantAsync(tenantProvider.Root, new SqlConnectionDefinition("somedatabase"));
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
    public class TenantSqlConnectionFactory : ITenantSqlConnectionFactory
    {
        private const string DevelopmentStorageConnectionString = "Server=(localdb)\\mssqllocaldb;Database=testtenant;Trusted_Connection=True;MultipleActiveResultSets=true";

        private readonly IConfiguration configuration;
        private readonly ConcurrentDictionary<object, Task<string>> connectionStrings = new ConcurrentDictionary<object, Task<string>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantSqlConnectionFactory"/> class.
        /// </summary>
        /// <param name="configuration">The current configuration. Used to determine the connection
        /// string to use when requesting an access token for KeyVault.</param>
        public TenantSqlConnectionFactory(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Creates a tenant-specific version of a sql connection definition.
        /// </summary>
        /// <param name="tenant">The tenant for which to build the definition.</param>
        /// <param name="connectionDefinition">The standard single-tenant version of the definition.</param>
        /// <returns>A Sql connection definition unique to the tenant.</returns>
        public static SqlConnectionDefinition GetContainerDefinitionForTenant(ITenant tenant, SqlConnectionDefinition connectionDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (connectionDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(connectionDefinition));
            }

            return new SqlConnectionDefinition(BuildTenantSpecificDatabaseName(tenant, connectionDefinition.Database));
        }

        /// <summary>
        /// Gets the cache key for a tenant Sql connection.
        /// </summary>
        /// <param name="tenantSqlConnectionDefinition">The definition of the tenant Sql connection.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(SqlConnectionDefinition tenantSqlConnectionDefinition)
        {
            if (tenantSqlConnectionDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(tenantSqlConnectionDefinition));
            }

            return $"{tenantSqlConnectionDefinition.Database}";
        }

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(SqlConfiguration storageConfiguration)
        {
            if (storageConfiguration is null)
            {
                throw new System.ArgumentNullException(nameof(storageConfiguration));
            }

            return string.IsNullOrEmpty(storageConfiguration.Database) ? "storageConfiguration-developmentStorage" : $"storageConfiguration-{storageConfiguration.Database}";
        }

        /// <summary>
        /// Get a Sql container for a tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to retrieve the container.</param>
        /// <param name="connectionDefinition">The details of the connection to create.</param>
        /// <returns>A connection instance for the tenant.</returns>
        public Task<SqlConnection> GetSqlConnectionForTenantAsync(ITenant tenant, SqlConnectionDefinition connectionDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (connectionDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(connectionDefinition));
            }

            SqlConnectionDefinition tenantedSqlConnectionDefinition = GetContainerDefinitionForTenant(tenant, connectionDefinition);
            return this.CreateSqlConnectionAsync(tenant, connectionDefinition, tenantedSqlConnectionDefinition);
        }

        /// <summary>
        /// Create the repository instance.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="tenantedSqlConnectionDefinition">The connection definition, adapted for the tenant.</param>
        /// <param name="configuration">The Sql configuration.</param>
        /// <returns>A <see cref="Task"/> with completes with the instance of the document repository for the tenant.</returns>
        protected async Task<SqlConnection> CreateSqlConnectionAsync(ITenant tenant, SqlConnectionDefinition tenantedSqlConnectionDefinition, SqlConfiguration configuration)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (tenantedSqlConnectionDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(tenantedSqlConnectionDefinition));
            }

            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            configuration.Database = string.IsNullOrWhiteSpace(configuration.Database)
                ? tenantedSqlConnectionDefinition.Database
                : configuration.DisableTenantIdPrefix
                    ? configuration.Database
                    : BuildTenantSpecificDatabaseName(tenant, configuration.Database);

            return await this.CreateSqlConnectionAsync(configuration).ConfigureAwait(false);
        }

        private static string BuildTenantSpecificDatabaseName(ITenant tenant, string database) => $"{tenant.Id.ToLowerInvariant()}-{database}";

        private async Task<SqlConnection> CreateSqlConnectionAsync(SqlConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.ConnectionStringSecretName) &&
                string.IsNullOrEmpty(configuration.ConnectionString) &&
                (string.IsNullOrEmpty(configuration.Database) || configuration.Database.Equals(DevelopmentStorageConnectionString)))
            {
                return new SqlConnection(DevelopmentStorageConnectionString);
            }
            else
            {
                object connectionStringKey = GetKeyFor(configuration);
                string connectionString = await this.connectionStrings.GetOrAdd(
                    connectionStringKey,
                    _ => this.GetConnectionStringAsync(configuration)).ConfigureAwait(false);

                var connectionStringBuilder = new StringBuilder(connectionString);
                if (!string.IsNullOrEmpty(configuration.Database))
                {
                    if (configuration.IsLocalDatabase)
                    {
                        // Append the database name as initial catalog if available
                        DbConnectionStringBuilder.AppendKeyValuePair(connectionStringBuilder, "Database", configuration.Database);
                    }
                    else
                    {
                        // Append the database name as initial catalog if available
                        DbConnectionStringBuilder.AppendKeyValuePair(connectionStringBuilder, "InitialCatalog", configuration.Database);
                    }
                }

                return new SqlConnection(connectionStringBuilder.ToString());
            }
        }

        private async Task<string> GetConnectionStringAsync(SqlConfiguration storageConfiguration)
        {
            if (string.IsNullOrEmpty(storageConfiguration.ConnectionString))
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider(this.configuration["AzureServicesAuthConnectionString"]);
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                SecretBundle accountKey = await keyVaultClient.GetSecretAsync($"https://{storageConfiguration.KeyVaultName}.vault.azure.net/secrets/{storageConfiguration.ConnectionStringSecretName}").ConfigureAwait(false);
                return accountKey.Value;
            }

            return storageConfiguration.ConnectionString;
        }

        private Task<SqlConnection> CreateSqlConnectionAsync(ITenant tenant, SqlConnectionDefinition connectionDefinition, SqlConnectionDefinition tenantedSqlConnectionDefinition)
        {
            SqlConfiguration configuration = tenant.GetSqlConfiguration(connectionDefinition);

            return this.CreateSqlConnectionAsync(tenant, tenantedSqlConnectionDefinition, configuration);
        }
    }
}