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
    using Corvus.Json;
    using Corvus.Tenancy.Exceptions;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Blob;
    using Newtonsoft.Json;

    /// <summary>
    /// A store for tenant data.
    /// </summary>
    public class TenantProviderBlobStore
        : ITenantStore
    {
        private const string LiveTenantsPrefix = "live/";
        private const string DeletedTenantsPrefix = "deleted/";
        private readonly ITenantCloudBlobContainerFactory tenantCloudBlobContainerFactory;
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
        /// <param name="tenantCloudBlobContainerFactory">The tenanted cloud blob container factory.</param>
        /// <param name="serializerSettingsProvider">The serializer settings provider for tenant serialization.</param>
        public TenantProviderBlobStore(
            RootTenant tenant,
            IPropertyBagFactory propertyBagFactory,
            ITenantCloudBlobContainerFactory tenantCloudBlobContainerFactory,
            IJsonSerializerSettingsProvider serializerSettingsProvider)
        {
            ArgumentNullException.ThrowIfNull(tenant);
            ArgumentNullException.ThrowIfNull(propertyBagFactory);
            ArgumentNullException.ThrowIfNull(tenantCloudBlobContainerFactory);
            ArgumentNullException.ThrowIfNull(serializerSettingsProvider);

            this.Root = tenant;
            this.tenantCloudBlobContainerFactory = tenantCloudBlobContainerFactory;
            this.serializerSettings = serializerSettingsProvider.Instance;
            this.propertyBagFactory = propertyBagFactory;
        }

        /// <summary>
        /// Gets or sets the container definition for the tenant store.
        /// </summary>
        public static BlobStorageContainerDefinition ContainerDefinition { get; set; } = new BlobStorageContainerDefinition("corvustenancy");

        /// <inheritdoc/>
        public RootTenant Root { get; }

        /// <inheritdoc/>
        public async Task<ITenant> GetTenantAsync(string tenantId, string? etag = null)
        {
            ArgumentNullException.ThrowIfNull(tenantId);

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
            ArgumentNullException.ThrowIfNull(tenantId);

            try
            {
                (_, CloudBlobContainer container) = await this.GetContainerAndTenantForChildTenantsOf(tenantId).ConfigureAwait(false);

                BlobContinuationToken? blobContinuationToken = await GetBlobContinuationTokenAsync(continuationToken).ConfigureAwait(false);

                BlobResultSegment segment = await container.ListBlobsSegmentedAsync(LiveTenantsPrefix, true, BlobListingDetails.None, limit, blobContinuationToken, null, null).ConfigureAwait(false);
                return new TenantCollectionResult(segment.Results.Select(s => ((CloudBlockBlob)s).Name[LiveTenantsPrefix.Length..]).ToList(), GenerateContinuationToken(segment.ContinuationToken));
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
                (_, CloudBlobContainer container) = await this.GetContainerAndTenantForChildTenantsOf(TenantExtensions.GetRequiredParentId(tenantId)).ConfigureAwait(false);

                CloudBlockBlob blob = GetLiveTenantBlockBlobReference(tenantId, container);

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
                await blob.UploadTextAsync(text).ConfigureAwait(false);
                tenant.ETag = blob.Properties.ETag;
                return updatedTenant;
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
        public Task<ITenant> CreateChildTenantAsync(string parentTenantId, string name)
            => this.CreateWellKnownChildTenantAsync(parentTenantId, Guid.NewGuid(), name);

        /// <inheritdoc/>
        public async Task<ITenant> CreateWellKnownChildTenantAsync(
            string parentTenantId,
            Guid wellKnownChildTenantGuid,
            string name)
        {
            ArgumentNullException.ThrowIfNull(parentTenantId);

            try
            {
                (ITenant parentTenant, CloudBlobContainer cloudBlobContainer) = await this.GetContainerAndTenantForChildTenantsOf(parentTenantId).ConfigureAwait(false);

                // We need to copy blob storage settings for the Tenancy container definition from the parent to the new child
                // to support the tenant blob store provider. We would expect this to be overridden by clients that wanted to
                // establish their own settings.
                BlobStorageConfiguration tenancyStorageConfiguration = parentTenant.GetBlobStorageConfiguration(ContainerDefinition);
                IPropertyBag childProperties = this.propertyBagFactory.Create(values =>
                    values.AddBlobStorageConfiguration(ContainerDefinition, tenancyStorageConfiguration));
                var child = new Tenant(
                    parentTenantId.CreateChildId(wellKnownChildTenantGuid),
                    name,
                    childProperties);

                // As we create the new blob, we need to ensure there isn't already a tenant with the same Id. We do this by
                // providing an If-None-Match header passing a "*", which will cause a storage exception with a 409 status
                // code if a blob with the same Id already exists.
                CloudBlockBlob blob = GetLiveTenantBlockBlobReference(child.Id, cloudBlobContainer);
                string text = JsonConvert.SerializeObject(child, this.serializerSettings);
                await blob.UploadTextAsync(
                    text,
                    null,
                    AccessCondition.GenerateIfNoneMatchCondition("*"),
                    null,
                    null).ConfigureAwait(false);
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
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                // This exception is thrown because there's already a tenant with the same Id. This should never happen when
                // this method has been called from CreateChildTenantAsync as the Guid will have been generated and the
                // chances of it matching one previously generated are miniscule. However, it could happen when calling this
                // method directly with a wellKnownChildTenantGuid that's already in use. In this case, the fault is with
                // the client code - creating tenants with well known Ids is something one would expect to happen under
                // controlled conditions, so it's only likely that a conflict will occur when either the client code has made
                // a mistake or someone is actively trying to cause problems.
                throw new ArgumentException(
                    $"A child tenant of '{parentTenantId}' with a well known Guid of '{wellKnownChildTenantGuid}' already exists.",
                    nameof(wellKnownChildTenantGuid));
            }
        }

        /// <inheritdoc/>
        public async Task DeleteTenantAsync(string tenantId)
        {
            ArgumentNullException.ThrowIfNull(tenantId);

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
            return this.tenantCloudBlobContainerFactory.GetBlobContainerForTenantAsync(parentTenant, ContainerDefinition);
        }

        private async Task<Tenant> GetTenantFromContainerAsync(string tenantId, CloudBlobContainer container, string? etag)
        {
            CloudBlockBlob blob = GetLiveTenantBlockBlobReference(tenantId, container);
            try
            {
                string text = await blob.DownloadTextAsync(Encoding.UTF8, string.IsNullOrEmpty(etag) ? null : AccessCondition.GenerateIfNoneMatchCondition(etag), null, null).ConfigureAwait(false);
                Tenant tenant = JsonConvert.DeserializeObject<Tenant>(text, this.serializerSettings);
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