// <copyright file="TenantCloudBlobContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.Tenancy;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Auth;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for a <see cref="CloudBlobContainer"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="CloudBlobContainer"/> for a specific
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
    /// serviceCollection.AddTenantCloudBlobContainerFactory();
    /// serviceCollection.AddTenantConfigurationAccountKeyProvider();
    /// </code>
    /// <para>
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="TenancyBlobStorageServiceCollectionExtensions.AddTenantCloudBlobContainerFactory(Microsoft.Extensions.DependencyInjection.IServiceCollection, TenantCloudBlobContainerFactoryOptions)"/>.
    /// </para>
    /// <para>
    /// Now, whenever you want to obtain a blob container for a tenant, you simply call <see cref="TenantCloudBlobContainerFactory.GetBlobContainerForTenantAsync(ITenant, BlobStorageContainerDefinition)"/>, passing
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
    internal class TenantCloudBlobContainerFactory : ITenantCloudBlobContainerFactory
    {
        private const string DevelopmentStorageConnectionString = "UseDevelopmentStorage=true";

        private readonly ConcurrentDictionary<object, Task<CloudBlobClient>> clients = new();
        private readonly ConcurrentDictionary<object, Task<CloudBlobContainer>> containers = new();
        private readonly TenantCloudBlobContainerFactoryOptions? options;
        private readonly Random random = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantCloudBlobContainerFactory"/> class.
        /// </summary>
        /// <param name="options">Configuration for the TenantCloudBlobContainerFactory.</param>
        public TenantCloudBlobContainerFactory(TenantCloudBlobContainerFactoryOptions? options = null)
        {
            this.options = options;
        }

        /// <summary>
        /// Creates a tenant-specific version of a storage container definition.
        /// </summary>
        /// <param name="tenant">The tenant for which to build the definition.</param>
        /// <param name="containerDefinition">The standard single-tenant version of the definition.</param>
        /// <returns>A blob container definition unique to the tenant.</returns>
        public static BlobStorageContainerDefinition BuildBlobStorageContainerDefinitionForTenant(ITenant tenant, BlobStorageContainerDefinition containerDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(containerDefinition));
            }

            return new BlobStorageContainerDefinition(BuildTenantSpecificContainerName(tenant, containerDefinition.ContainerName));
        }

        /// <summary>
        /// Gets the cache key for a tenant blob container.
        /// </summary>
        /// <param name="tenantBlobStorageContainerDefinition">The definition of the tenant blob container.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(BlobStorageContainerDefinition tenantBlobStorageContainerDefinition)
        {
            if (tenantBlobStorageContainerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(tenantBlobStorageContainerDefinition));
            }

            return $"{tenantBlobStorageContainerDefinition.ContainerName}";
        }

        /// <summary>
        /// Gets the cache key for a storage account client.
        /// </summary>
        /// <param name="storageConfiguration">The configuration of the tenant storage account.</param>
        /// <returns>The cache key.</returns>
        public static object GetKeyFor(BlobStorageConfiguration storageConfiguration)
        {
            if (storageConfiguration is null)
            {
                throw new System.ArgumentNullException(nameof(storageConfiguration));
            }

            return string.IsNullOrEmpty(storageConfiguration.AccountName) ? "storageConfiguration-developmentStorage" : $"storageConfiguration-{storageConfiguration.AccountName}";
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
        public async Task<CloudBlobContainer> GetBlobContainerForTenantAsync(ITenant tenant, BlobStorageContainerDefinition containerDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(containerDefinition));
            }

            BlobStorageContainerDefinition tenantedBlobStorageContainerDefinition = BuildBlobStorageContainerDefinitionForTenant(tenant, containerDefinition);
            object key = GetKeyFor(tenantedBlobStorageContainerDefinition);

            Task<CloudBlobContainer> result = this.containers.GetOrAdd(
                key,
                _ => this.CreateTenantCloudBlobContainer(tenant, containerDefinition, tenantedBlobStorageContainerDefinition));

            if (result.IsFaulted)
            {
                // If a task has been created in the previous statement, it won't have completed yet. Therefore if it's
                // faulted, that means it was added as part of a previous request to this method, and subsequently
                // failed. As such, we will remove the item from the dictionary, and attempt to create a new one to
                // return. If removing the value fails, that's likely because it's been removed by a different thread,
                // so we will ignore that and just attempt to create and return a new value anyway.
                this.containers.TryRemove(key, out Task<CloudBlobContainer>? _);

                // Wait for a short and random time, to reduce the potential for large numbers of spurious container
                // recreation that could happen if multiple threads are trying to rectify the failure simultanously.
                await Task.Delay(this.random.Next(150, 250)).ConfigureAwait(false);

                result = this.containers.GetOrAdd(
                    key,
                    _ => this.CreateTenantCloudBlobContainer(tenant, containerDefinition, tenantedBlobStorageContainerDefinition));
            }

            return await result.ConfigureAwait(false);
        }

        /// <summary>
        /// Create the repository instance.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="tenantedBlobStorageContainerDefinition">The repository definition, adapted for the tenant.</param>
        /// <param name="configuration">The repository configuration.</param>
        /// <returns>A <see cref="Task"/> with completes with the instance of the document repository for the tenant.</returns>
        protected async Task<CloudBlobContainer> CreateCloudBlobContainerInstanceAsync(ITenant tenant, BlobStorageContainerDefinition tenantedBlobStorageContainerDefinition, BlobStorageConfiguration configuration)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (tenantedBlobStorageContainerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(tenantedBlobStorageContainerDefinition));
            }

            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
            string tenantedContainerName = string.IsNullOrWhiteSpace(configuration.Container)
                ? tenantedBlobStorageContainerDefinition.ContainerName
                : (configuration.DisableTenantIdPrefix
                    ? configuration.Container!
                    : BuildTenantSpecificContainerName(tenant, configuration.Container!));

            // Get the cloud blob client for the specified configuration.
            object accountCacheKey = GetKeyFor(configuration);

            CloudBlobClient blobClient = await this.clients.GetOrAdd(
                accountCacheKey,
                _ => this.CreateCloudBlobClientAsync(configuration)).ConfigureAwait(false);

            // Now get the container and create it if it doesn't already exist.
            CloudBlobContainer container = blobClient.GetContainerReference(AzureStorageNameHelper.HashAndEncodeBlobContainerName(tenantedContainerName));

            BlobContainerPublicAccessType accessType = configuration.AccessType ?? tenantedBlobStorageContainerDefinition.AccessType;

            await container.CreateIfNotExistsAsync(accessType, null, null).ConfigureAwait(false);

            return container;
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

        private async Task<CloudBlobClient> CreateCloudBlobClientAsync(BlobStorageConfiguration configuration)
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

            return account.CreateCloudBlobClient();
        }

        private async Task<string> GetAccountKeyAsync(BlobStorageConfiguration storageConfiguration)
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

        private async Task<CloudBlobContainer> CreateTenantCloudBlobContainer(ITenant tenant, BlobStorageContainerDefinition containerDefinition, BlobStorageContainerDefinition tenantedBlobStorageContainerDefinition)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(containerDefinition));
            }

            if (tenantedBlobStorageContainerDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(tenantedBlobStorageContainerDefinition));
            }

            BlobStorageConfiguration? configuration = tenant.GetBlobStorageConfiguration(containerDefinition);

            return await this.CreateCloudBlobContainerInstanceAsync(tenant, tenantedBlobStorageContainerDefinition, configuration!).ConfigureAwait(false);
        }
    }
}