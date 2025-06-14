﻿// <copyright file="TenantSqlConnectionFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Sql.Tenancy.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Data.Common;
    using System.Security.Principal;
    using System.Text;
    using System.Threading.Tasks;

    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;

    using Corvus.Tenancy;

    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for a <see cref="Sql"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of a <see cref="SqlConnection"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the connection string for the tenant, and the
    /// configuration comes from the tenant via <see cref="SqlStorageTenantExtensions.AddSqlConfiguration(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{string, object}}, SqlConnectionDefinition, SqlConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the Sql container factory and the configuration account key provider in your container configuration (assuming you have added a standard ConfigurationRoot to your solution).
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantSqlContainerFactory();
    /// serviceCollection.AddTenantConfigurationAccountKeyProvider();
    /// </code>
    /// <para>
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="Microsoft.Extensions.DependencyInjection.TenancySqlServiceCollectionExtensions.AddTenantSqlConnectionFactory(IServiceCollection, TenantSqlConnectionFactoryOptions)"/>.
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
    internal class TenantSqlConnectionFactory : ITenantSqlConnectionFactory
    {
        private const string DevelopmentStorageConnectionString = "Server=(localdb)\\mssqllocaldb;Database=testtenant;Trusted_Connection=True;MultipleActiveResultSets=true";

        private readonly ConcurrentDictionary<object, Task<string>> connectionStrings = new();
        private readonly TenantSqlConnectionFactoryOptions? options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantSqlConnectionFactory"/> class.
        /// </summary>
        /// <param name="options">Configuration for the TenantCloudBlobContainerFactory.</param>
        public TenantSqlConnectionFactory(TenantSqlConnectionFactoryOptions? options = null)
        {
            this.options = options;
        }

        /// <summary>
        /// Creates a tenant-specific version of a sql connection definition.
        /// </summary>
        /// <param name="tenant">The tenant for which to build the definition.</param>
        /// <param name="connectionDefinition">The standard single-tenant version of the definition.</param>
        /// <returns>A Sql connection definition unique to the tenant.</returns>
        public static SqlConnectionDefinition GetContainerDefinitionForTenant(ITenant tenant, SqlConnectionDefinition connectionDefinition)
        {
            ArgumentNullException.ThrowIfNull(tenant);
            ArgumentNullException.ThrowIfNull(connectionDefinition);

            return new SqlConnectionDefinition(BuildTenantSpecificDatabaseName(tenant, connectionDefinition.Database));
        }

        /// <summary>
        /// Gets the cache key for a tenant Sql connection.
        /// </summary>
        /// <param name="tenantSqlConnectionDefinition">The definition of the tenant Sql connection.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(SqlConnectionDefinition tenantSqlConnectionDefinition)
        {
            ArgumentNullException.ThrowIfNull(tenantSqlConnectionDefinition);

            return $"{tenantSqlConnectionDefinition.Database}";
        }

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(SqlConfiguration storageConfiguration)
        {
            ArgumentNullException.ThrowIfNull(storageConfiguration);

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
            ArgumentNullException.ThrowIfNull(tenant);
            ArgumentNullException.ThrowIfNull(connectionDefinition);

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
            ArgumentNullException.ThrowIfNull(tenant);
            ArgumentNullException.ThrowIfNull(tenantedSqlConnectionDefinition);
            ArgumentNullException.ThrowIfNull(configuration);

            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            configuration.Database = string.IsNullOrWhiteSpace(configuration.Database)
                ? tenantedSqlConnectionDefinition.Database
                : configuration.DisableTenantIdPrefix
                    ? configuration.Database
                    : BuildTenantSpecificDatabaseName(tenant, configuration.Database!);

            return await this.CreateSqlConnectionAsync(configuration).ConfigureAwait(false);
        }

        private static string BuildTenantSpecificDatabaseName(ITenant tenant, string database) => $"{tenant.Id.ToLowerInvariant()}-{database}";

        private static void ThrowConfigurationException(SqlConfiguration storageConfiguration)
        {
            throw new InvalidOperationException("You must provide either a ConnectionString or both a KeyVaultName and a ConnectionStringSecretName to configure your SqlConnection. " +
                $"You have provided ConnectionString=\"{storageConfiguration.ConnectionString}\", KeyVaultName=\"{storageConfiguration.KeyVaultName}\", ConnectionStringSecretName=\"{storageConfiguration.ConnectionStringSecretName}\"");
        }

        private async Task<SqlConnection> CreateSqlConnectionAsync(SqlConfiguration configuration)
        {
            if (configuration.IsEmpty())
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
                        DbConnectionStringBuilder.AppendKeyValuePair(connectionStringBuilder, "Initial Catalog", configuration.Database);
                    }
                }

                return new SqlConnection(connectionStringBuilder.ToString());
            }
        }

        private async Task<string> GetConnectionStringAsync(SqlConfiguration storageConfiguration)
        {
            if (!storageConfiguration.Validate())
            {
                ThrowConfigurationException(storageConfiguration);
            }

            if (string.IsNullOrEmpty(storageConfiguration.ConnectionString))
            {
                var keyVaultCredentials = Identity.ClientAuthentication.Azure.LegacyAzureServiceTokenProviderConnectionString.ToTokenCredential(this.options?.AzureServicesAuthConnectionString!);

                var keyVaultUri = new Uri($"https://{storageConfiguration.KeyVaultName}.vault.azure.net/");
                var keyVaultClient = new SecretClient(keyVaultUri, keyVaultCredentials);

                global::Azure.Response<KeyVaultSecret>? accountKeyResponse = await keyVaultClient.GetSecretAsync(storageConfiguration.ConnectionStringSecretName).ConfigureAwait(false);

                return accountKeyResponse.Value.Value;
            }

            if (!string.IsNullOrEmpty(storageConfiguration.KeyVaultName) || !string.IsNullOrEmpty(storageConfiguration.ConnectionStringSecretName))
            {
                ThrowConfigurationException(storageConfiguration);
            }

            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            return storageConfiguration.ConnectionString!;
        }

        private Task<SqlConnection> CreateSqlConnectionAsync(ITenant tenant, SqlConnectionDefinition connectionDefinition, SqlConnectionDefinition tenantedSqlConnectionDefinition)
        {
            SqlConfiguration? configuration = tenant.GetSqlConfiguration(connectionDefinition);

            return this.CreateSqlConnectionAsync(tenant, tenantedSqlConnectionDefinition, configuration!);
        }
    }
}