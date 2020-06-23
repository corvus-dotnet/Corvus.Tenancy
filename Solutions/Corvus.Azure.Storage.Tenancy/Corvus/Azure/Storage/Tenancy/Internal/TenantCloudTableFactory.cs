// <copyright file="TenantCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using System.Collections.Concurrent;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Corvus.Tenancy;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for a <see cref="CloudTable"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="CloudTable"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant the <see cref="TableStorageTenantExtensions.AddTableStorageConfiguration(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{string, object}}, TableStorageTableDefinition, TableStorageConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the table factory and the configuration account key provider in your container configuration (assuming you have added a standard ConfigurationRoot to your solution).
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantCloudTableFactory();
    /// serviceCollection.AddTenantConfigurationAccountKeyProvider();
    /// </code>
    /// <para>
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="TenancyTableStorageServiceCollectionExtensions.AddTenantCloudTableFactory(Microsoft.Extensions.DependencyInjection.IServiceCollection, TenantCloudTableFactoryOptions)"/>.
    /// </para>
    /// <para>
    /// Now, whenever you want to obtain a table for a tenant, you simply call <see cref="TenantCloudTableFactory.GetTableForTenantAsync(ITenant, TableStorageTableDefinition)"/>, passing
    /// it the tenant and the container definition you want to use.
    /// </para>
    /// <para>
    /// <code>
    /// TenantCloudTableFactory factory;
    ///
    /// var repository = await factory.GetTableContainerForTenantAsync(tenantProvider.Root, new TableStorageTableDefinition("somecontainer"));
    /// </code>
    /// </para>
    /// <para>
    /// If you create containers in this way (rather than just newing them up) then your application can easily be multitented
    /// by ensuring that you always pass the Tenant through your stack, and just default to tenantProvider.Root at the top level.
    /// </para>
    /// <para>
    /// Note that it will be possible for code that obtains a CloudTable in this way to use the resulting object to access
    /// the CloudTableClient and thus access other tables in the same account. As such these objects should only ever be
    /// handed to trusted code.
    /// </para>
    /// <para>
    /// Note also that because we have not wrapped the resulting CloudTable in a class of our own, we cannot automatically
    /// implement key rotation.
    /// </para>
    /// </remarks>
    internal class TenantCloudTableFactory : ITenantCloudTableFactory
    {
        private const string DevelopmentStorageConnectionString = "UseDevelopmentStorage=true";

        private readonly ConcurrentDictionary<object, Task<CloudTableClient>> clients = new ConcurrentDictionary<object, Task<CloudTableClient>>();
        private readonly ConcurrentDictionary<object, Task<CloudTable>> containers = new ConcurrentDictionary<object, Task<CloudTable>>();
        private readonly TenantCloudTableFactoryOptions? options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantCloudTableFactory"/> class.
        /// </summary>
        /// <param name="options">Configuration for the TenantCloudTableFactory.</param>
        public TenantCloudTableFactory(TenantCloudTableFactoryOptions? options = null)
        {
            this.options = options;
        }

        /// <summary>
        /// Creates a tenant-specific version of a storage container definition.
        /// </summary>
        /// <param name="tenant">The tenant for which to build the definition.</param>
        /// <param name="containerDefinition">The standard single-tenant version of the definition.</param>
        /// <returns>A table definition unique to the tenant.</returns>
        public static TableStorageTableDefinition BuildTableStorageTableDefinitionForTenant(ITenant tenant, TableStorageTableDefinition containerDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(containerDefinition));
            }

            return new TableStorageTableDefinition(BuildTenantSpecificContainerName(tenant, containerDefinition.TableName));
        }

        /// <summary>
        /// Gets the cache key for a tenant table.
        /// </summary>
        /// <param name="tenantTableStorageTableDefinition">The definition of the tenant table.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(TableStorageTableDefinition tenantTableStorageTableDefinition)
        {
            if (tenantTableStorageTableDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(tenantTableStorageTableDefinition));
            }

            return $"{tenantTableStorageTableDefinition.TableName}";
        }

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(TableStorageConfiguration storageConfiguration)
        {
            if (storageConfiguration is null)
            {
                throw new System.ArgumentNullException(nameof(storageConfiguration));
            }

            return string.IsNullOrEmpty(storageConfiguration.AccountName) ? "storageConfiguration-developmentStorage" : $"storageConfiguration-{storageConfiguration.AccountName}";
        }

        /// <summary>
        /// Get a table for a tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to retrieve the container.</param>
        /// <param name="containerDefinition">The details of the container to create.</param>
        /// <returns>The container instance for the tenant.</returns>
        /// <remarks>
        /// This caches container instances to ensure that a singleton is used for all request for the same tenant and container definition.
        /// </remarks>
        public Task<CloudTable> GetTableForTenantAsync(ITenant tenant, TableStorageTableDefinition containerDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(containerDefinition));
            }

            TableStorageTableDefinition tenantedTableStorageTableDefinition = BuildTableStorageTableDefinitionForTenant(tenant, containerDefinition);
            object key = GetKeyFor(tenantedTableStorageTableDefinition);

            return this.containers.GetOrAdd(
                key,
                async _ => await this.CreateTenantCloudTable(tenant, containerDefinition, tenantedTableStorageTableDefinition).ConfigureAwait(false));
        }

        /// <summary>
        /// Create the repository instance.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="tenantedTableStorageTableDefinition">The repository definition, adapted for the tenant.</param>
        /// <param name="configuration">The repository configuration.</param>
        /// <returns>A <see cref="Task"/> with completes with the instance of the document repository for the tenant.</returns>
        protected async Task<CloudTable> CreateCloudTableInstanceAsync(ITenant tenant, TableStorageTableDefinition tenantedTableStorageTableDefinition, TableStorageConfiguration configuration)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (tenantedTableStorageTableDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(tenantedTableStorageTableDefinition));
            }

            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            string tenantedTableName = string.IsNullOrWhiteSpace(configuration.TableName)
                ? tenantedTableStorageTableDefinition.TableName
                : (configuration.DisableTenantIdPrefix
                    ? configuration.TableName!
                    : BuildTenantSpecificContainerName(tenant, configuration.TableName!));

            // Get the cloud table client for the specified configuration.
            object accountCacheKey = GetKeyFor(configuration);

            CloudTableClient tableClient = await this.clients.GetOrAdd(
                accountCacheKey,
                _ => this.CreateCloudTableClientAsync(configuration)).ConfigureAwait(false);

            // Now get the container and create it if it doesn't already exist.
            CloudTable container = tableClient.GetTableReference(HashAndEncodeContainerName(tenantedTableStorageTableDefinition.TableName));

            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            return container;
        }

        private static string HashAndEncodeContainerName(string containerName)
        {
            byte[] byteContents = Encoding.UTF8.GetBytes(containerName);
            using var hash = new SHA1CryptoServiceProvider();
            byte[] hashedBytes = hash.ComputeHash(byteContents);
            string hexString = TenantExtensions.ByteArrayToHexViaLookup32(hashedBytes);

            // Table names can't start with a number, so prefix all names with a letter
            return "t" + hexString;
        }

        private static string BuildTenantSpecificContainerName(ITenant tenant, string container)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (container is null)
            {
                throw new System.ArgumentNullException(nameof(container));
            }

            return $"{tenant.Id.ToLowerInvariant()}-{container}";
        }

        private async Task<CloudTableClient> CreateCloudTableClientAsync(TableStorageConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            CloudStorageAccount account;

            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            if (string.IsNullOrEmpty(configuration.AccountName) || configuration.AccountName!.Equals(DevelopmentStorageConnectionString))
            {
                account = CloudStorageAccount.DevelopmentStorageAccount;
            }
            else if (string.IsNullOrWhiteSpace(configuration.AccountKeySecretName))
            {
                account = CloudStorageAccount.Parse(configuration.AccountName);
            }
            else
            {
                string accountKey = await this.GetAccountKeyAsync(configuration).ConfigureAwait(false);
                var credentials = new StorageCredentials(configuration.AccountName, accountKey);
                account = new CloudStorageAccount(credentials, configuration.AccountName, null, true);
            }

            return account.CreateCloudTableClient();
        }

        private async Task<string> GetAccountKeyAsync(TableStorageConfiguration storageConfiguration)
        {
            if (storageConfiguration is null)
            {
                throw new System.ArgumentNullException(nameof(storageConfiguration));
            }

            var azureServiceTokenProvider = new AzureServiceTokenProvider(this.options?.AzureServicesAuthConnectionString);
            using var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            Microsoft.Azure.KeyVault.Models.SecretBundle accountKey = await keyVaultClient.GetSecretAsync($"https://{storageConfiguration.KeyVaultName}.vault.azure.net/secrets/{storageConfiguration.AccountKeySecretName}").ConfigureAwait(false);
            return accountKey.Value;
        }

        private async Task<CloudTable> CreateTenantCloudTable(ITenant tenant, TableStorageTableDefinition containerDefinition, TableStorageTableDefinition tenantedTableStorageTableDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(containerDefinition));
            }

            if (tenantedTableStorageTableDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(tenantedTableStorageTableDefinition));
            }

            TableStorageConfiguration? configuration = tenant.GetTableStorageConfiguration(containerDefinition);

            return await this.CreateCloudTableInstanceAsync(tenant, tenantedTableStorageTableDefinition, configuration!).ConfigureAwait(false);
        }
    }
}
