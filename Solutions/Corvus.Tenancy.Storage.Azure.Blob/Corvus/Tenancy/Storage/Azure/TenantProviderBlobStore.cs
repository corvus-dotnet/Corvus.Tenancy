// <copyright file="TenantProviderBlobStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Extensions;
    using Corvus.Extensions.Json;
    using Corvus.Tenancy.Exceptions;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    /// <summary>
    /// A store for tenant data.
    /// </summary>
    public class TenantProviderBlobStore
        : ITenantProvider
    {
        private const string LiveTenantsPrefix = "live/";
        private const string DeletedTenantsPrefix = "deleted/";
        private readonly IServiceProvider serviceProvider;
        private readonly ITenantCloudBlobContainerFactory tenantCloudBlobContainerFactory;
        private readonly JsonSerializerSettings serializerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantProviderBlobStore"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider from which to get a tenant.</param>
        /// <param name="tenant">The root tenant (registered as a singleton in the container).</param>
        /// <param name="tenantCloudBlobContainerFactory">The tenanted cloud blob container factory.</param>
        /// <param name="serializerSettingsProvider">The serializer settings provider for tenant serialization.</param>
        public TenantProviderBlobStore(IServiceProvider serviceProvider, RootTenant tenant, ITenantCloudBlobContainerFactory tenantCloudBlobContainerFactory, IJsonSerializerSettingsProvider serializerSettingsProvider)
        {
            if (serializerSettingsProvider is null)
            {
                throw new ArgumentNullException(nameof(serializerSettingsProvider));
            }

            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.Root = tenant ?? throw new ArgumentNullException(nameof(tenant));
            this.tenantCloudBlobContainerFactory = tenantCloudBlobContainerFactory ?? throw new ArgumentNullException(nameof(tenantCloudBlobContainerFactory));
            this.serializerSettings = serializerSettingsProvider.Instance;
        }

        /// <summary>
        /// Gets or sets the container definition for the tenant store.
        /// </summary>
        public BlobStorageContainerDefinition ContainerDefinition { get; set; } = new BlobStorageContainerDefinition("corvustenancy");

        /// <inheritdoc/>
        public ITenant Root { get; }

        /// <inheritdoc/>
        public async Task<ITenant> GetTenantAsync(string tenantId, string? etag = null)
        {
            if (tenantId is null)
            {
                throw new ArgumentNullException(nameof(tenantId));
            }

            if (tenantId == this.Root.Id)
            {
                return this.Root;
            }

            try
            {
                (_, CloudBlobContainer container) = await this.GetContainerAndTenantForChildTenantsOf(TenantExtensions.GetRequiredParentId(tenantId)).ConfigureAwait(false);
                return await this.GetTenantFromContainerAsync(tenantId, container, etag).ConfigureAwait(false);
            }
            catch (FormatException fex)
            {
                throw new TenantNotFoundException("Unsupported tenant ID", fex);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new TenantNotFoundException();
            }
        }

        /// <inheritdoc/>
        public async Task<TenantCollectionResult> GetChildrenAsync(string tenantId, int limit = 20, string? continuationToken = null)
        {
            if (tenantId is null)
            {
                throw new ArgumentNullException(nameof(tenantId));
            }

            try
            {
                (_, CloudBlobContainer container) = await this.GetContainerAndTenantForChildTenantsOf(tenantId).ConfigureAwait(false);

                BlobContinuationToken? blobContinuationToken = await GetBlobContinuationTokenAsync(continuationToken).ConfigureAwait(false);

                BlobResultSegment segment = await container.ListBlobsSegmentedAsync(LiveTenantsPrefix, true, BlobListingDetails.None, limit, blobContinuationToken, null, null).ConfigureAwait(false);
                return new TenantCollectionResult(segment.Results.Select(s => ((CloudBlockBlob)s).Name.Substring(LiveTenantsPrefix.Length)).ToList(), GenerateContinuationToken(segment.ContinuationToken));
            }
            catch (FormatException fex)
            {
                throw new TenantNotFoundException("Unsupported tenant ID", fex);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new TenantNotFoundException();
            }
        }

        /// <inheritdoc/>
        public async Task<ITenant> UpdateTenantAsync(ITenant tenant)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (tenant.Id == this.Root.Id)
            {
                return this.Root;
            }

            try
            {
                (_, CloudBlobContainer container) = await this.GetContainerAndTenantForChildTenantsOf(tenant.GetRequiredParentId()).ConfigureAwait(false);

                CloudBlockBlob blob = GetLiveTenantBlockBlobReference(tenant.Id, container);
                string text = JsonConvert.SerializeObject(tenant, this.serializerSettings);
                await blob.UploadTextAsync(text).ConfigureAwait(false);
                tenant.ETag = blob.Properties.ETag;
                return tenant;
            }
            catch (FormatException fex)
            {
                throw new TenantNotFoundException("Unsupported tenant ID", fex);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new TenantNotFoundException();
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                throw new TenantConflictException();
            }
        }

        /// <inheritdoc/>
        public async Task<ITenant> CreateChildTenantAsync(string parentTenantId, string name)
        {
            if (parentTenantId is null)
            {
                throw new ArgumentNullException(nameof(parentTenantId));
            }

            try
            {
                (ITenant parentTenant, CloudBlobContainer cloudBlobContainer) = await this.GetContainerAndTenantForChildTenantsOf(parentTenantId).ConfigureAwait(false);
                Tenant child = this.serviceProvider.GetRequiredService<Tenant>();
                child.Id = parentTenantId.CreateChildId();
                child.Name = name;

                // At the least, we need to copy the default blob storage settings from its parent
                // to support the tenant blob store provider. We would expect this to be overridden
                // by clients that wanted to establish their own settings.
                BlobStorageConfiguration? defaultStorageConfiguration = parentTenant.GetDefaultBlobStorageConfiguration();
                child.SetDefaultBlobStorageConfiguration(defaultStorageConfiguration!);
                if (parentTenant.HasStorageBlobConfiguration(this.ContainerDefinition))
                {
                    child.SetBlobStorageConfiguration(this.ContainerDefinition, parentTenant.GetBlobStorageConfiguration(this.ContainerDefinition) !);
                }

                CloudBlockBlob blob = GetLiveTenantBlockBlobReference(child.Id, cloudBlobContainer);

                string text = JsonConvert.SerializeObject(child, this.serializerSettings);
                await blob.UploadTextAsync(text).ConfigureAwait(false);
                child.ETag = blob.Properties.ETag;

                return child;
            }
            catch (FormatException fex)
            {
                throw new TenantNotFoundException("Unsupported tenant ID", fex);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new TenantNotFoundException();
            }
        }

        /// <inheritdoc/>
        public async Task DeleteTenantAsync(string tenantId)
        {
            if (tenantId is null)
            {
                throw new ArgumentNullException(nameof(tenantId));
            }

            if (tenantId == this.Root.Id)
            {
                throw new InvalidOperationException("You can not delete the root tenant.");
            }

            try
            {
                (_, CloudBlobContainer container) = await this.GetContainerAndTenantForChildTenantsOf(TenantExtensions.GetRequiredParentId(tenantId)).ConfigureAwait(false);
                CloudBlockBlob blob = GetLiveTenantBlockBlobReference(tenantId, container);
                string blobText = await blob.DownloadTextAsync().ConfigureAwait(false);
                CloudBlockBlob deletedBlob = container.GetBlockBlobReference(DeletedTenantsPrefix + tenantId);
                await deletedBlob.UploadTextAsync(blobText).ConfigureAwait(false);
                await blob.DeleteIfExistsAsync().ConfigureAwait(false);
            }
            catch (FormatException fex)
            {
                throw new TenantNotFoundException("Unsupported tenant ID", fex);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new TenantNotFoundException();
            }
        }

        /// <summary>
        /// Generates a URL-friendly continuation token from a BlobContinuationToken.
        /// </summary>
        /// <param name="continuationToken">The continuation token.</param>
        /// <returns>A <see cref="BlobContinuationToken"/> built from the continuation token.</returns>
        private static async Task<BlobContinuationToken?> GetBlobContinuationTokenAsync(string? continuationToken)
        {
            if (continuationToken == null)
            {
                return null;
            }

            var output = new BlobContinuationToken();
            using (var reader = XmlReader.Create(continuationToken.Base64UrlDecode().AsStream(Encoding.Unicode), new XmlReaderSettings { Async = true }))
            {
                await output.ReadXmlAsync(reader).ConfigureAwait(false);
            }

            return output;
        }

        /// <summary>
        /// Generates a URL-friendly continuation token from a BlobContinuationToken.
        /// </summary>
        /// <param name="continuationToken">The blob continuation token.</param>
        /// <returns>A URL-friendly string, or null if there are no further results.</returns>
        private static string? GenerateContinuationToken(BlobContinuationToken? continuationToken)
        {
            if (continuationToken == null)
            {
                return null;
            }

            var output = new StringBuilder();
            using (var writer = XmlWriter.Create(output, new XmlWriterSettings() { Indent = false }))
            {
                continuationToken.WriteXml(writer);
                writer.Flush();
            }

            return output.ToString().Base64UrlEncode();
        }

        private static CloudBlockBlob GetLiveTenantBlockBlobReference(string tenantId, CloudBlobContainer container)
        {
            return container.GetBlockBlobReference(LiveTenantsPrefix + tenantId);
        }

        /// <summary>
        /// Gets a blob container in which we store the child tenants of a given
        /// tenant.
        /// </summary>
        /// <param name="tenantId">The id of the parent tenant.</param>
        /// <returns>The parent tenant, and a blob container containing the child tenants of that parent tenant.</returns>
        private async Task<(ITenant, CloudBlobContainer)> GetContainerAndTenantForChildTenantsOf(string tenantId)
        {
            ITenant currentTenant = this.Root;

            // Get the repo for the root tenant
            CloudBlobContainer cloudBlobContainer = await this.GetCloudBlobContainer(currentTenant).ConfigureAwait(false);

            if (tenantId == RootTenant.RootTenantId)
            {
                return (this.Root, cloudBlobContainer);
            }

            // Skip the root id
            IEnumerable<string> ids = tenantId.GetParentTree();

            // Now, looping over all the other tenant IDs in the path
            foreach (string id in ids)
            {
                // Get the tenant from its parent's container
                currentTenant = await this.GetTenantFromContainerAsync(id, cloudBlobContainer, null).ConfigureAwait(false);

                // Then get the container for that tenant
                cloudBlobContainer = await this.GetCloudBlobContainer(currentTenant).ConfigureAwait(false);
            }

            // Finally, return the tenant repository which contains the children of the specified tenant
            return (currentTenant, cloudBlobContainer);
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

        private async Task<ITenant> GetTenantFromContainerAsync(string tenantId, CloudBlobContainer container, string? etag)
        {
            CloudBlockBlob blob = GetLiveTenantBlockBlobReference(tenantId, container);
            try
            {
                string text = await blob.DownloadTextAsync(Encoding.UTF8, string.IsNullOrEmpty(etag) ? null : AccessCondition.GenerateIfNoneMatchCondition(etag), null, null).ConfigureAwait(false);
                ITenant tenant = JsonConvert.DeserializeObject<ITenant>(text, this.serializerSettings);
                tenant.ETag = blob.Properties.ETag;
                return tenant;
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotModified)
            {
                throw new TenantNotModifiedException();
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new TenantNotFoundException();
            }
        }
    }
}