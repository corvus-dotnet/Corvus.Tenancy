// <copyright file="TenantProviderBlobStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Extensions;
    using Corvus.Extensions.Json;
    using Corvus.Json;
    using Corvus.Tenancy.Exceptions;

    using global::Azure;
    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Blobs.Models;
    using global::Azure.Storage.Blobs.Specialized;

    using Newtonsoft.Json;

    /// <summary>
    /// A store for tenant data.
    /// </summary>
    public class TenantProviderBlobStore
        : ITenantStore
    {
        private const string LiveTenantsPrefix = "live/";
        private const string DeletedTenantsPrefix = "deleted/";
        private readonly ITenantBlobContainerClientFactory tenantBlobContainerClientFactory;
        private readonly JsonSerializerSettings serializerSettings;
        private readonly IPropertyBagFactory propertyBagFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantProviderBlobStore"/> class.
        /// </summary>
        /// <param name="tenant">The root tenant (registered as a singleton in the container).</param>
        /// <param name="propertyBagFactory">
        /// Enables creation of <see cref="IPropertyBag"/> instances when no existing serialized
        /// representation exists (i.e., when creating a new tenant), and building of modified property
        /// bags.
        /// </param>
        /// <param name="tenantBlobContainerClientFactory">The tenanted cloud blob container factory.</param>
        /// <param name="serializerSettingsProvider">The serializer settings provider for tenant serialization.</param>
        public TenantProviderBlobStore(
            RootTenant tenant,
            IPropertyBagFactory propertyBagFactory,
            ITenantBlobContainerClientFactory tenantBlobContainerClientFactory,
            IJsonSerializerSettingsProvider serializerSettingsProvider)
        {
            if (serializerSettingsProvider is null)
            {
                throw new ArgumentNullException(nameof(serializerSettingsProvider));
            }

            this.Root = tenant ?? throw new ArgumentNullException(nameof(tenant));
            this.tenantBlobContainerClientFactory = tenantBlobContainerClientFactory ?? throw new ArgumentNullException(nameof(tenantBlobContainerClientFactory));
            this.serializerSettings = serializerSettingsProvider.Instance;
            this.propertyBagFactory = propertyBagFactory;
        }

        /// <summary>
        /// Gets or sets the container definition for the tenant store.
        /// </summary>
        public static string ContainerStorageContextName { get; set; } = "corvustenancy";

        /// <inheritdoc/>
        public RootTenant Root { get; }

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
                (_, BlobContainerClient container) = await this.GetContainerAndTenantForChildTenantsOf(TenantExtensions.GetRequiredParentId(tenantId)).ConfigureAwait(false);
                return await this.GetTenantFromContainerAsync(tenantId, container, etag).ConfigureAwait(false);
            }
            catch (FormatException fex)
            {
                throw new TenantNotFoundException("Unsupported tenant ID", fex);
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
                (_, BlobContainerClient container) = await this.GetContainerAndTenantForChildTenantsOf(tenantId).ConfigureAwait(false);

                string? blobContinuationToken = DecodeUrlEncodedContinuationToken(continuationToken);

                AsyncPageable<BlobItem> pageable = container.GetBlobsAsync(prefix: LiveTenantsPrefix);
                IAsyncEnumerable<Page<BlobItem>> pages = pageable.AsPages(blobContinuationToken, limit);
                await using IAsyncEnumerator<Page<BlobItem>> page = pages.GetAsyncEnumerator();
                Page<BlobItem>? p = await page.MoveNextAsync()
                    ? page.Current
                    : null;
                IEnumerable<BlobItem> items = p?.Values ?? Enumerable.Empty<BlobItem>();

                ////BlobResultSegment segment = await container.GetBlobsAsync(
                ////    prefix: LiveTenantsPrefix,
                ////    true,   // Flat
                ////    BlobListingDetails.None,
                ////    limit,
                ////    blobContinuationToken,
                ////    null,
                ////    null).ConfigureAwait(false);
                return new TenantCollectionResult(items.Select(s => s.Name.Substring(LiveTenantsPrefix.Length)).ToList(), GenerateContinuationToken(p?.ContinuationToken));
            }
            catch (FormatException fex)
            {
                throw new TenantNotFoundException("Unsupported tenant ID", fex);
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                throw new TenantNotFoundException();
            }
        }

        /// <inheritdoc/>
        public async Task<ITenant> UpdateTenantAsync(
            string tenantId,
            string? name,
            IEnumerable<KeyValuePair<string, object>>? propertiesToSetOrAdd,
            IEnumerable<string>? propertiesToRemove)
        {
            if (name is null && propertiesToSetOrAdd is null && propertiesToRemove is null)
            {
                throw new ArgumentNullException(nameof(propertiesToSetOrAdd), $"{nameof(name)}, {nameof(propertiesToSetOrAdd)}, and {nameof(propertiesToRemove)} cannot all be null");
            }

            if (tenantId == this.Root.Id)
            {
                ((RootTenant)this.Root).UpdateProperties(propertiesToSetOrAdd, propertiesToRemove);
                return this.Root;
            }

            try
            {
                (_, BlobContainerClient container) = await this.GetContainerAndTenantForChildTenantsOf(TenantExtensions.GetRequiredParentId(tenantId)).ConfigureAwait(false);

                BlockBlobClient blob = GetLiveTenantBlockBlobReference(tenantId, container);

                Tenant tenant = await this.GetTenantFromContainerAsync(tenantId, container, null).ConfigureAwait(false);

                IPropertyBag updatedProperties = this.propertyBagFactory.CreateModified(
                    tenant.Properties,
                    propertiesToSetOrAdd,
                    propertiesToRemove);

                var updatedTenant = new Tenant(
                    tenant.Id,
                    name ?? tenant.Name,
                    updatedProperties);
                string text = JsonConvert.SerializeObject(updatedTenant, this.serializerSettings);
                using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
                Response<BlobContentInfo> uploadResponse = await blob.UploadAsync(contentStream, new BlobUploadOptions { Conditions = new BlobRequestConditions { IfMatch = new ETag(tenant.ETag!) } }).ConfigureAwait(false);
                tenant.ETag = uploadResponse.Value.ETag.ToString("G");
                return updatedTenant;
            }
            catch (FormatException fex)
            {
                throw new TenantNotFoundException("Unsupported tenant ID", fex);
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                throw new TenantNotFoundException();
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.Conflict)
            {
                throw new TenantConflictException();
            }
        }

        /// <inheritdoc/>
        public Task<ITenant> CreateChildTenantAsync(string parentTenantId, string name)
            => this.CreateWellKnownChildTenantAsync(parentTenantId, Guid.NewGuid(), name);

        /// <inheritdoc/>
        public async Task<ITenant> CreateWellKnownChildTenantAsync(
            string parentTenantId,
            Guid wellKnownChildTenantGuid,
            string name)
        {
            if (parentTenantId is null)
            {
                throw new ArgumentNullException(nameof(parentTenantId));
            }

            try
            {
                (ITenant parentTenant, BlobContainerClient blobContainerClient) = await this.GetContainerAndTenantForChildTenantsOf(parentTenantId).ConfigureAwait(false);

                // We need to copy blob storage settings for the Tenancy container definition from the parent to the new child
                // to support the tenant blob store provider. We would expect this to be overridden by clients that wanted to
                // establish their own settings.
                BlobStorageConfiguration tenancyStorageConfiguration = parentTenant.GetBlobStorageConfiguration(ContainerStorageContextName);
                IPropertyBag childProperties = this.propertyBagFactory.Create(values =>
                    values.AddBlobStorageConfiguration(ContainerStorageContextName, tenancyStorageConfiguration));
                var child = new Tenant(
                    parentTenantId.CreateChildId(wellKnownChildTenantGuid),
                    name,
                    childProperties);

                // As we create the new blob, we need to ensure there isn't already a tenant with the same Id. We do this by
                // providing an If-None-Match header passing a "*", which will cause a storage exception with a 409 status
                // code if a blob with the same Id already exists.
                BlockBlobClient blob = GetLiveTenantBlockBlobReference(child.Id, blobContainerClient);
                string text = JsonConvert.SerializeObject(child, this.serializerSettings);
                using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
                Response<BlobContentInfo> uploadResponse = await blob.UploadAsync(
                    contentStream,
                    new BlobUploadOptions { Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All } })
                    .ConfigureAwait(false);
                child.ETag = uploadResponse.Value.ETag.ToString("G");

                return child;
            }
            catch (FormatException fex)
            {
                throw new TenantNotFoundException("Unsupported tenant ID", fex);
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                throw new TenantNotFoundException();
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.Conflict)
            {
                // This exception is thrown because there's already a tenant with the same Id. This should never happen when
                // this method has been called from CreateChildTenantAsync as the Guid will have been generated and the
                // chances of it matching one previously generated are miniscule. However, it could happen when calling this
                // method directly with a wellKnownChildTenantGuid that's already in use. In this case, the fault is with
                // the client code - creating tenants with well known Ids is something one would expect to happen under
                // controlled conditions, so it's only likely that a conflict will occur when either the client code has made
                // a mistake or someone is actively trying to cause problems.
                throw new TenantConflictException(
                    $"A child tenant of '{parentTenantId}' with a well known Guid of '{wellKnownChildTenantGuid}' already exists.");
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
                (_, BlobContainerClient container) = await this.GetContainerAndTenantForChildTenantsOf(TenantExtensions.GetRequiredParentId(tenantId)).ConfigureAwait(false);
                BlockBlobClient blob = GetLiveTenantBlockBlobReference(tenantId, container);
                Response<BlobDownloadInfo> downloadResponse = await blob.DownloadAsync().ConfigureAwait(false);
                using var deletedBlobContent = new MemoryStream();
                await downloadResponse.Value.Content.CopyToAsync(deletedBlobContent).ConfigureAwait(false);
                deletedBlobContent.Position = 0;

                BlockBlobClient deletedBlob = container.GetBlockBlobClient(DeletedTenantsPrefix + tenantId);
                await deletedBlob.UploadAsync(deletedBlobContent).ConfigureAwait(false);
                await blob.DeleteIfExistsAsync().ConfigureAwait(false);
            }
            catch (FormatException fex)
            {
                throw new TenantNotFoundException("Unsupported tenant ID", fex);
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                throw new TenantNotFoundException();
            }
        }

        /// <summary>
        /// Decodes a URL-friendly continuation token back to its original form.
        /// </summary>
        /// <param name="continuationTokenInUrlForm">The continuation token.</param>
        /// <returns>A <see cref="string"/> built from the continuation token.</returns>
        private static string? DecodeUrlEncodedContinuationToken(string? continuationTokenInUrlForm) => continuationTokenInUrlForm?.Base64UrlDecode();

        /// <summary>
        /// Generates a URL-friendly continuation token from a continuation token provided by blob storage.
        /// </summary>
        /// <param name="continuationToken">The continuation token in the form blob storage supplied it.</param>
        /// <returns>A URL-friendly string, or null if <paramref name="continuationToken"/> was null.</returns>
        private static string? GenerateContinuationToken(string? continuationToken) => continuationToken?.Base64UrlEncode();

        private static BlockBlobClient GetLiveTenantBlockBlobReference(string tenantId, BlobContainerClient container)
        {
            return container.GetBlockBlobClient(LiveTenantsPrefix + tenantId);
        }

        /// <summary>
        /// Gets a blob container in which we store the child tenants of a given
        /// tenant.
        /// </summary>
        /// <param name="tenantId">The id of the parent tenant.</param>
        /// <returns>The parent tenant, and a blob container containing the child tenants of that parent tenant.</returns>
        private async Task<(ITenant, BlobContainerClient)> GetContainerAndTenantForChildTenantsOf(string tenantId)
        {
            ITenant currentTenant = this.Root;

            // Get the repo for the root tenant
            BlobContainerClient blobContainerClient = await this.GetBlobContainerClient(currentTenant).ConfigureAwait(false);

            if (tenantId == RootTenant.RootTenantId)
            {
                return (this.Root, blobContainerClient);
            }

            // Skip the root id
            IEnumerable<string> ids = tenantId.GetParentTree();

            // Now, looping over all the other tenant IDs in the path
            foreach (string id in ids)
            {
                // Get the tenant from its parent's container
                currentTenant = await this.GetTenantFromContainerAsync(id, blobContainerClient, null).ConfigureAwait(false);

                // Then get the container for that tenant
                blobContainerClient = await this.GetBlobContainerClient(currentTenant).ConfigureAwait(false);
            }

            // Finally, return the tenant repository which contains the children of the specified tenant
            return (currentTenant, blobContainerClient);
        }

        /// <summary>
        /// Gets a tenant document repository for a parent tenant, with or without key rotation.
        /// </summary>
        /// <param name="parentTenant">The tenant for which to get the child tenant repository.</param>
        /// <returns>A <see cref="Task"/> which completes with the document repository for children of the specified tenant.</returns>
        private Task<BlobContainerClient> GetBlobContainerClient(ITenant parentTenant)
        {
            return this.tenantBlobContainerClientFactory.GetContextForTenantAsync(parentTenant, ContainerStorageContextName);
        }

        private async Task<Tenant> GetTenantFromContainerAsync(string tenantId, BlobContainerClient container, string? etag)
        {
            BlockBlobClient blob = GetLiveTenantBlockBlobReference(tenantId, container);

            // Can't use DownloadContentAsync because of https://github.com/Azure/azure-sdk-for-net/issues/22598
            Response<BlobDownloadStreamingResult> response = await blob.DownloadStreamingAsync(
                conditions: string.IsNullOrEmpty(etag) ? null : new BlobRequestConditions { IfNoneMatch = new ETag(etag!) })
                .ConfigureAwait(false);

            int status = response.GetRawResponse().Status;
            if (status == 304)
            {
                throw new TenantNotModifiedException();
            }
            else if (status == 404)
            {
                throw new TenantNotFoundException();
            }

            // Note: it is technically possible to use System.Text.Json to work directly from
            // the UTF-8 data, which is more efficient than decoding to a .NET UTF-16 string
            // first. However, we have to do this for the time being because we are in the world of
            // IJsonSerializerSettingsProvider, where all serialization options are managed in
            // terms of JSON.NET.
            using BlobDownloadStreamingResult blobDownloadStreamingResult = response.Value;
            BinaryData data = await BinaryData.FromStreamAsync(blobDownloadStreamingResult.Content).ConfigureAwait(false);
            string text = data.ToString();

            Tenant tenant = JsonConvert.DeserializeObject<Tenant>(text, this.serializerSettings);
            tenant.ETag = response.Value.Details.ETag.ToString("G");
            return tenant;
        }
    }
}