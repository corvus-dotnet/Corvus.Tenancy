// <copyright file="TenantProviderBlobStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Endjin.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Extensions.Json;
    using Corvus.Tenancy;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Blob;
    using Newtonsoft.Json;

    /// <summary>
    /// A store for tenant data.
    /// </summary>
    public class TenantProviderBlobStore
        : ITenantProvider
    {
        private static readonly BlobRequestOptions DefaultRequestOptions = new BlobRequestOptions();
        private static readonly OperationContext DefaultOperationContext = new OperationContext();

        private readonly ITenantCloudBlobContainerFactory tenantCloudBlobContainerFactory;
        private readonly JsonSerializerSettings serializerSettings;
        private readonly JsonSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantProviderBlobStore"/> class.
        /// </summary>
        /// <param name="tenantCloudBlobContainerFactory">The tenanted cloud blob container factory.</param>
        /// <param name="serializerSettingsProvider">The serializer settings provider for tenant serialization.</param>
        public TenantProviderBlobStore(ITenantCloudBlobContainerFactory tenantCloudBlobContainerFactory, IJsonSerializerSettingsProvider serializerSettingsProvider)
        {
            this.tenantCloudBlobContainerFactory = tenantCloudBlobContainerFactory;
            this.serializerSettings = serializerSettingsProvider.Instance;
            this.serializer = JsonSerializer.Create(this.serializerSettings);
        }

        /// <summary>
        /// Gets or sets the container definition for the tenant store.
        /// </summary>
        public BlobStorageContainerDefinition ContainerDefinition { get; set; } = new BlobStorageContainerDefinition("endjintenancy");

        /// <inheritdoc/>
        public ITenant Root { get; } = new Tenant { Id = TenantExtensions.RootTenantId };

        /// <inheritdoc/>
        public async Task<ITenant> GetTenantAsync(string tenantId)
        {
            if (tenantId == this.Root.Id)
            {
                return this.Root;
            }

            CloudBlobContainer container = await this.GetContainerForChildTenantsOf(tenantId.GetParentId()).ConfigureAwait(false);
            return await this.GetTenantFromContainerAsync(tenantId, container).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<TenantCollectionResult> GetChildrenAsync(string tenantId, int limit = 20, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<ITenant> UpdateTenantAsync(ITenant tenant)
        {
            if (tenant.Id == this.Root.Id)
            {
                return tenant;
            }

            CloudBlobContainer container = await this.GetContainerForChildTenantsOf(tenant.GetParentId()).ConfigureAwait(false);

            CloudBlockBlob blob = container.GetBlockBlobReference(tenant.Id);

            using (CloudBlobStream stream = await blob.OpenWriteAsync(
                new AccessCondition { IfMatchETag = tenant.ETag },
                DefaultRequestOptions,
                DefaultOperationContext).ConfigureAwait(false))
            using (var writer = new StreamWriter(stream))
            {
                this.serializer.Serialize(writer, tenant);
            }

            tenant.ETag = blob.Properties.ETag;
            return tenant;
        }

        /// <inheritdoc/>
        public Task<ITenant> CreateChildTenantAsync(string parentTenantId)
        {
            ////CloudBlobContainer cloudBlobContainer = await this.GetContainerForChildTenantsOf(parentTenantId).ConfigureAwait(false);
            ////var child = new Tenant( { Id = Path.Combine(parentTenantId, Guid.NewGuid().ToString()) };
            ////ResourceResponse<Document> result = await repository.InsertAsync(child).ConfigureAwait(false);
            ////return result.Resource.As<ITenant>();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a blob container in which we store the child tenants of a given
        /// tenant.
        /// </summary>
        /// <param name="tenantId">The id of the parent tenant.</param>
        /// <returns>A blob container containing the child tenants of that parent tenant.</returns>
        private async Task<CloudBlobContainer> GetContainerForChildTenantsOf(string tenantId)
        {
            // Skip the root id
            IEnumerable<string> ids = tenantId.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Skip(1);

            ITenant currentTenant = this.Root;

            // Get the repo for the root tenant
            CloudBlobContainer cloudBlobContainer = await this.GetCloudBlobContainer(currentTenant).ConfigureAwait(false);

            // And set the current tenant ID to the root tenant id
            var currentTenantId = new StringBuilder(currentTenant.Id);

            // Now, looping over all the other tenant IDs in the path
            foreach (string id in ids)
            {
                // Add the next tenant's ID
                currentTenantId.Append("/");
                currentTenantId.Append(id);

                // Get the tenant from its parent's container
                currentTenant = await this.GetTenantFromContainerAsync(currentTenantId.ToString(), cloudBlobContainer).ConfigureAwait(false);

                // Then get the container for that tenant
                cloudBlobContainer = await this.GetCloudBlobContainer(currentTenant).ConfigureAwait(false);
            }

            // Finally, return the tenant repository which contains the children of the specified tenant
            return cloudBlobContainer;
        }

        /// <summary>
        /// Gets a tenant document repository for a parent tenant, with or without key rotation.
        /// </summary>
        /// <param name="parentTenant">The tenant for which to get the child tenant repository.</param>
        /// <returns>A <see cref="Task"/> which completes with the document repository for children of the specified tenant.</returns>
        private Task<CloudBlobContainer> GetCloudBlobContainer(ITenant parentTenant)
        {
            return this.tenantCloudBlobContainerFactory.GetBlobContainerForTenantAsync(parentTenant, this.ContainerDefinition);
        }

        private async Task<ITenant> GetTenantFromContainerAsync(string tenantId, CloudBlobContainer container)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(tenantId);

            using (Stream stream = await blob.OpenReadAsync(
                null,
                DefaultRequestOptions,
                DefaultOperationContext).ConfigureAwait(false))
            using (var jsonReader = new JsonTextReader(new StreamReader(stream)))
            {
                return this.serializer.Deserialize(jsonReader) as ITenant;
            }
        }
    }
}
