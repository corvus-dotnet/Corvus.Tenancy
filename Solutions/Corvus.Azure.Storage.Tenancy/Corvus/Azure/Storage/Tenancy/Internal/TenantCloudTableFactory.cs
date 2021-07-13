// <copyright file="TenantCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.Tenancy;
    using Corvus.Tenancy.Azure.Common;

    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for a <see cref="CloudTable"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="CloudTable"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant via the <see cref="TableStorageTenantExtensions.AddTableStorageConfiguration(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{string, object}}, TableStorageTableDefinition, TableStorageConfiguration)"/>.
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
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="TenancyTableStorageServiceCollectionExtensions.AddTenantCloudTableFactory(IServiceCollection, TenantCloudTableFactoryOptions)"/>.
    /// </para>
    /// <para>
    /// Now, whenever you want to obtain a table for a tenant, you simply call <see cref="GetTableForTenantAsync(ITenant, TableStorageTableDefinition)"/>, passing
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
    internal class TenantCloudTableFactory :
        TenantStorageFactory<CloudTable, TableStorageTableDefinition, TableStorageConfiguration>,
        ITenantCloudTableFactory
    {
        private const string DevelopmentStorageConnectionString = "UseDevelopmentStorage=true";

        private readonly ConcurrentDictionary<string, Task<CloudTableClient>> clients = new ConcurrentDictionary<string, Task<CloudTableClient>>();
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
        /// Get a table for a tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to retrieve the table.</param>
        /// <param name="tableDefinition">The details of the table to create.</param>
        /// <returns>The container instance for the tenant.</returns>
        /// <remarks>
        /// This caches table instances to ensure that a singleton is used for all request for the same tenant and table definition.
        /// </remarks>
        public Task<CloudTable> GetTableForTenantAsync(ITenant tenant, TableStorageTableDefinition tableDefinition)
        {
            return this.GetContainerForTenantAsync(tenant, tableDefinition);
        }

        /// <inheritdoc/>
        protected override async Task<CloudTable> CreateContainerAsync(
            ITenant tenant,
            TableStorageTableDefinition tenantedTableStorageTableDefinition,
            TableStorageConfiguration configuration)
        {
            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            string tenantedTableName = string.IsNullOrWhiteSpace(configuration.TableName)
                ? tenantedTableStorageTableDefinition.TableName
                : (configuration.DisableTenantIdPrefix
                    ? configuration.TableName!
                    : BuildTenantSpecificTableName(tenant, configuration.TableName!));

            // Get the cloud table client for the specified configuration.
            string accountCacheKey = GetCacheKeyForStorageAccount(configuration);

            CloudTableClient tableClient = await this.clients.GetOrAdd(
                accountCacheKey,
                _ => this.CreateCloudTableClientAsync(configuration)).ConfigureAwait(false);

            // Now get the container and create it if it doesn't already exist.
            CloudTable container = tableClient.GetTableReference(AzureStorageNameHelper.HashAndEncodeTableName(tenantedTableName));

            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            return container;
        }

        /// <inheritdoc/>
        protected override TableStorageTableDefinition MakeDefinition(
            string tenantSpecificContainerName,
            ITenant tenant,
            TableStorageTableDefinition nonTenantSpecificContainerDefinition)
            => new TableStorageTableDefinition(tenantSpecificContainerName);

        /// <inheritdoc/>
        protected override string GetContainerName(TableStorageTableDefinition definition)
            => definition.TableName;

        /// <inheritdoc/>
        protected override TableStorageConfiguration GetConfiguration(ITenant tenant, TableStorageTableDefinition definition)
         => tenant.GetTableStorageConfiguration(definition);

        /// <inheritdoc/>
        protected override string GetCacheKeyForContainer(TableStorageTableDefinition definition)
            => definition.TableName;

        private static string BuildTenantSpecificTableName(ITenant tenant, string tableName)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (tableName is null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            return $"{tenant.Id.ToLowerInvariant()}-{tableName}";
        }

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        private static string GetCacheKeyForStorageAccount(TableStorageConfiguration storageConfiguration)
        {
            if (storageConfiguration is null)
            {
                throw new ArgumentNullException(nameof(storageConfiguration));
            }

            return string.IsNullOrEmpty(storageConfiguration.AccountName) ? "storageConfiguration-developmentStorage" : $"storageConfiguration-{storageConfiguration.AccountName}";
        }

        private async Task<CloudTableClient> CreateCloudTableClientAsync(TableStorageConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
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
                string accountKey = await this.GetKeyVaultSecretAsync(
                    this.options?.AzureServicesAuthConnectionString,
                    configuration.KeyVaultName!,
                    configuration.AccountKeySecretName!).ConfigureAwait(false);
                var credentials = new StorageCredentials(configuration.AccountName, accountKey);
                account = new CloudStorageAccount(credentials, configuration.AccountName, null, true);
            }

            return account.CreateCloudTableClient();
        }
    }
}