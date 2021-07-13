// <copyright file="TenantBlobContainerClientFactory.cs" company="Endjin Limited">
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

    using global::Azure.Storage;
    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Blobs.Models;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for a <see cref="BlobContainerClient"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="BlobContainerClient"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant the <see cref="BlobStorageTenantExtensions.AddBlobStorageConfiguration(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{string, object}}, BlobStorageContainerDefinition, BlobStorageConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the blob container factory and the configuration account key provider in your container configuration (assuming you have added a standard ConfigurationRoot to your solution).
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantBlobContainerClientFactory();
    /// serviceCollection.AddTenantConfigurationAccountKeyProvider();
    /// </code>
    /// <para>
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="TenancyBlobStorageServiceCollectionExtensions.AddTenantBlobContainerClientFactory(Microsoft.Extensions.DependencyInjection.IServiceCollection, TenantBlobContainerClientFactoryOptions)"/>.
    /// </para>
    /// <para>
    /// Now, whenever you want to obtain a blob container for a tenant, you simply call <see cref="TenantBlobContainerClientFactory.GetBlobContainerForTenantAsync(ITenant, BlobStorageContainerDefinition)"/>, passing
    /// it the tenant and the container definition you want to use.
    /// </para>
    /// <para>
    /// <code>
    /// TenantBlobContainerClientFactory factory;
    ///
    /// var repository = await factory.GetBlobContainerForTenantAsync(tenantProvider.Root, new BlobStorageContainerDefinition("somecontainer"));
    /// </code>
    /// </para>
    /// <para>
    /// If you create containers in this way (rather than just newing them up) then your application can easily be multitented
    /// by ensuring that you always pass the Tenant through your stack, and just default to tenantProvider.Root at the top level.
    /// </para>
    /// <para>
    /// Note that it will be possible for code that obtains a BlobContainerClient in this way to use the resulting object to access
    /// the CloudBlobClient and thus access other blob contains in the same container. As such these objects should only ever be
    /// handed to trusted code.
    /// </para>
    /// <para>
    /// Note also that because we have not wrapped the resulting BlobContainerClient in a class of our own, we cannot automatically
    /// implement key rotation.
    /// </para>
    /// </remarks>
    internal class TenantBlobContainerClientFactory :
        TenantStorageFactory<BlobContainerClient, BlobStorageContainerDefinition, BlobStorageConfiguration>,
        ITenantBlobContainerClientFactory
    {
        private const string DevelopmentStorageConnectionString = "UseDevelopmentStorage=true";

        private readonly ConcurrentDictionary<object, Task<BlobServiceClient>> clients = new ConcurrentDictionary<object, Task<BlobServiceClient>>();
        private readonly TenantBlobContainerClientFactoryOptions? options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantBlobContainerClientFactory"/> class.
        /// </summary>
        /// <param name="options">Configuration for the TenantBlobContainerClientFactory.</param>
        public TenantBlobContainerClientFactory(TenantBlobContainerClientFactoryOptions? options = null)
        {
            this.options = options;
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
        public Task<BlobContainerClient> GetBlobContainerForTenantAsync(ITenant tenant, BlobStorageContainerDefinition containerDefinition)
        {
            return this.GetContainerForTenantAsync(tenant, containerDefinition);
        }

        /// <inheritdoc/>
        protected override async Task<BlobContainerClient> CreateContainerAsync(
            ITenant tenant,
            BlobStorageContainerDefinition tenantedContainerDefinition,
            BlobStorageConfiguration configuration)
        {
            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            string tenantedContainerName = string.IsNullOrWhiteSpace(configuration.Container)
                ? tenantedContainerDefinition.ContainerName
                : (configuration.DisableTenantIdPrefix
                    ? configuration.Container!
                    : BuildTenantSpecificContainerName(tenant, configuration.Container!));

            // Get the cloud blob client for the specified configuration.
            object accountCacheKey = GetKeyFor(configuration);

            BlobServiceClient blobClient = await this.clients.GetOrAdd(
                accountCacheKey,
                _ => this.CreateBlockBlobClientAsync(configuration)).ConfigureAwait(false);

            // Now get the container and create it if it doesn't already exist.
            BlobContainerClient container = blobClient.GetBlobContainerClient(AzureStorageNameHelper.HashAndEncodeBlobContainerName(tenantedContainerName));

            PublicAccessType accessType = configuration.AccessType ?? tenantedContainerDefinition.AccessType;

            await container.CreateIfNotExistsAsync(accessType, null, null).ConfigureAwait(false);

            return container;
        }

        /// <inheritdoc/>
        protected override BlobStorageConfiguration GetConfiguration(
            ITenant tenant,
            BlobStorageContainerDefinition definition)
            => tenant.GetBlobStorageConfiguration(definition);

        /// <inheritdoc/>
        protected override string GetContainerName(BlobStorageContainerDefinition definition)
            => definition.ContainerName;

        /// <inheritdoc/>
        protected override string GetCacheKeyForContainer(BlobStorageContainerDefinition tenantBlobStorageContainerDefinition)
            => tenantBlobStorageContainerDefinition.ContainerName;

        /// <inheritdoc/>
        protected override BlobStorageContainerDefinition MakeDefinition(
            string tenantSpecificContainerName,
            ITenant tenant,
            BlobStorageContainerDefinition nonTenantSpecificContainerDefinition)
            => new BlobStorageContainerDefinition(tenantSpecificContainerName);

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        private static object GetKeyFor(BlobStorageConfiguration storageConfiguration)
        {
            if (storageConfiguration is null)
            {
                throw new System.ArgumentNullException(nameof(storageConfiguration));
            }

            return string.IsNullOrEmpty(storageConfiguration.AccountName) ? "storageConfiguration-developmentStorage" : $"storageConfiguration-{storageConfiguration.AccountName}";
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

        private async Task<BlobServiceClient> CreateBlockBlobClientAsync(BlobStorageConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            BlobServiceClient client;

            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            if (string.IsNullOrEmpty(configuration.AccountName) || configuration.AccountName!.Equals(DevelopmentStorageConnectionString))
            {
                client = new BlobServiceClient(DevelopmentStorageConnectionString);
            }
            else if (string.IsNullOrWhiteSpace(configuration.AccountKeySecretName))
            {
                // As the documentation for BlobStorageConfiguration.AccountName says:
                //  "If the account key secret name is empty, then this should contain
                //   a complete connection string."
                client = new BlobServiceClient(configuration.AccountName);
            }
            else
            {
                string accountKey = await this.GetKeyVaultSecretAsync(
                    this.options?.AzureServicesAuthConnectionString,
                    configuration.KeyVaultName!,
                    configuration.AccountKeySecretName!).ConfigureAwait(false);
                var credentials = new StorageSharedKeyCredential(configuration.AccountName, accountKey);
                client = new BlobServiceClient(
                    new Uri($"https://{configuration.AccountName}.blob.core.windows.net"),
                    credentials);
            }

            return client;
        }
    }
}
