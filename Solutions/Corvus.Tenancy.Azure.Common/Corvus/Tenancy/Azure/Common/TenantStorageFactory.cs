// <copyright file="TenantStorageFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Azure.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Corvus.Tenancy;

    using global::Azure;
    using global::Azure.Core;
    using global::Azure.Identity;
    using global::Azure.Security.KeyVault.Secrets;

    /// <summary>
    /// Common logic for tenanted storage container factories.
    /// </summary>
    /// <typeparam name="TContainer">
    /// The type of storage container (e.g., a blob container, a CosmosDB collection, or a SQL
    /// database).
    /// </typeparam>
    /// <typeparam name="TDefinition">
    /// The type that identifies the particular container required.
    /// </typeparam>
    /// <typeparam name="TConfiguration">
    /// The type containing the information identifying a particular physical, tenant-specific
    /// instance of a container.
    /// </typeparam>
    public abstract class TenantStorageFactory<TContainer, TDefinition, TConfiguration>
    {
        private readonly ConcurrentDictionary<object, Task<TContainer>> containers = new ConcurrentDictionary<object, Task<TContainer>>();
        private readonly Random random = new Random();

        /// <summary>
        /// Get a storage container for a tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to retrieve the container.</param>
        /// <param name="containerDefinition">The details of the container to create.</param>
        /// <returns>The container instance for the tenant.</returns>
        /// <remarks>
        /// This caches container instances to ensure that a singleton is used for all request for the same tenant and container definition.
        /// </remarks>
        public async Task<TContainer> GetContainerForTenantAsync(ITenant tenant, TDefinition containerDefinition)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new ArgumentNullException(nameof(containerDefinition));
            }

            TDefinition tenantedContainerDefinition = this.BuildContainerDefinitionForTenant(tenant, containerDefinition);
            object key = this.GetCacheKeyForContainer(tenantedContainerDefinition);

            Task<TContainer> result = this.containers.GetOrAdd(
                key,
                async _ => await this.CreateContainerAsync(tenant, containerDefinition, tenantedContainerDefinition).ConfigureAwait(false));

            if (result.IsFaulted)
            {
                // If a task has been created in the previous statement, it won't have completed yet. Therefore if it's
                // faulted, that means it was added as part of a previous request to this method, and subsequently
                // failed. As such, we will remove the item from the dictionary, and attempt to create a new one to
                // return. If removing the value fails, that's likely because it's been removed by a different thread,
                // so we will ignore that and just attempt to create and return a new value anyway.
                this.containers.TryRemove(key, out Task<TContainer> _);

                // Wait for a short and random time, to reduce the potential for large numbers of spurious container
                // recreation that could happen if multiple threads are trying to rectify the failure simultanously.
                await Task.Delay(this.random.Next(150, 250)).ConfigureAwait(false);

                result = this.containers.GetOrAdd(
                    key,
                    _ => this.CreateContainerAsync(tenant, containerDefinition, tenantedContainerDefinition));
            }

            return await result.ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a secret from Azure Key Vault.
        /// </summary>
        /// <param name="azureServicesAuthConnectionString">
        /// The connection string determining the credentials with which to connect to Azure Storage.
        /// </param>
        /// <param name="keyVaultName">
        /// The name of the key vault.
        /// </param>
        /// <param name="secretName">
        /// The name of the secret in the key vault.
        /// </param>
        /// <returns>
        /// A task producing the secret.
        /// </returns>
        protected async Task<string> GetKeyVaultSecretAsync(
            string? azureServicesAuthConnectionString,
            string keyVaultName,
            string secretName)
        {
            // Irritatingly, v12 of the Azure SDK has done away with the AppAuthentication connection
            // strings that AzureServiceTokenProvider used to support, making it very much harder to
            // allow an application to switch between different modes of authentication via configuration.
            // This code supports some of the ones we often use.
            const string appIdPattern = "RunAs=App;AppId=(?<AppId>[A-Fa-f0-9]{8}(?:-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12});TenantId=(?<TenantId>[A-Fa-f0-9]{8}(?:-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12});AppKey=(?<AppKey>[^;]*)";
            TokenCredential keyVaultCredentials = (azureServicesAuthConnectionString?.Trim() ?? string.Empty) switch
            {
#pragma warning disable SA1122 // Use string.Empty for empty strings - StyleCop analyzer 1.1.118 doesn't understand patterns; it *has* to be "" here
                "" => new DefaultAzureCredential(),
#pragma warning restore SA1122 // Use string.Empty for empty strings

                "RunAs=Developer;DeveloperTool=AzureCli" => new AzureCliCredential(),
                "RunAs=Developer;DeveloperTool=VisualStudio" => new VisualStudioCredential(),
                "RunAs=App" => new ManagedIdentityCredential(),

                string s when Regex.Match(s, appIdPattern) is Match m && m.Success =>
                    new ClientSecretCredential(m.Groups["TenantId"].Value, m.Groups["AppId"].Value, m.Groups["AppKey"].Value),

                _ => throw new InvalidOperationException($"AzureServicesAuthConnectionString configuration value '{azureServicesAuthConnectionString}' is not supported in this version of Corvus Tenancy")
            };

            var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
            var keyVaultClient = new SecretClient(keyVaultUri, keyVaultCredentials);

            Response<KeyVaultSecret> accountKeyResponse = await keyVaultClient.GetSecretAsync(secretName).ConfigureAwait(false);
            return accountKeyResponse.Value.Value;
        }

        /// <summary>
        /// Gets the name from a container definition.
        /// </summary>
        /// <param name="definition">The container definition.</param>
        /// <returns>The name.</returns>
        protected abstract string GetContainerName(TDefinition definition);

        /// <summary>
        /// Gets a container definition from a tenant-specific name.
        /// </summary>
        /// <param name="tenantSpecificContainerName">The name.</param>
        /// <param name="tenant">The tenant for which the container is required.</param>
        /// <param name="nonTenantSpecificContainerDefinition">
        /// The original (non-tenant-specific) container definition from which the tenant-specific
        /// container name was derived.
        /// </param>
        /// <returns>The container definition.</returns>
        protected abstract TDefinition MakeDefinition(
            string tenantSpecificContainerName,
            ITenant tenant,
            TDefinition nonTenantSpecificContainerDefinition);

        /// <summary>
        /// Gets tenant-specific container configuration for a container definition.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="definition">The container definition.</param>
        /// <returns>The container configuration.</returns>
        protected abstract TConfiguration GetConfiguration(ITenant tenant, TDefinition definition);

        /// <summary>
        /// Gets the cache key for a tenanted container.
        /// </summary>
        /// <param name="tenantedContainerDefinition">The tenant-specific definition of the container.</param>
        /// <returns>The cache key.</returns>
        protected abstract string GetCacheKeyForContainer(TDefinition tenantedContainerDefinition);

        /// <summary>
        /// Create the container instance.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="tenantedContainerDefinition">The container definition, adapted for the tenant.</param>
        /// <param name="configuration">The container configuration.</param>
        /// <returns>A <see cref="Task"/> with completes with the instance of the container for the tenant.</returns>
        protected abstract Task<TContainer> CreateContainerAsync(
            ITenant tenant,
            TDefinition tenantedContainerDefinition,
            TConfiguration configuration);

        private static string BuildTenantSpecificContainerName(ITenant tenant, string container)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            return $"{tenant.Id.ToLowerInvariant()}-{container}";
        }

        /// <summary>
        /// Creates a tenant-specific version of a storage container definition.
        /// </summary>
        /// <param name="tenant">The tenant for which to build the definition.</param>
        /// <param name="containerDefinition">The standard single-tenant version of the definition.</param>
        /// <returns>A container definition unique to the tenant.</returns>
        private TDefinition BuildContainerDefinitionForTenant(
            ITenant tenant,
            TDefinition containerDefinition)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new ArgumentNullException(nameof(containerDefinition));
            }

            return this.MakeDefinition(
                BuildTenantSpecificContainerName(tenant, this.GetContainerName(containerDefinition)),
                tenant,
                containerDefinition);
        }

        private async Task<TContainer> CreateContainerAsync(
            ITenant tenant,
            TDefinition containerDefinition,
            TDefinition tenantedContainerDefinition)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (containerDefinition is null)
            {
                throw new ArgumentNullException(nameof(containerDefinition));
            }

            if (tenantedContainerDefinition is null)
            {
                throw new ArgumentNullException(nameof(tenantedContainerDefinition));
            }

            TConfiguration configuration = this.GetConfiguration(tenant, containerDefinition);

            if (configuration == null)
            {
                throw new InvalidOperationException($"{this.GetType().Name}.{nameof(this.GetConfiguration)} returned null");
            }

            return await this.CreateContainerAsync(tenant, tenantedContainerDefinition, configuration).ConfigureAwait(false);
        }
    }
}