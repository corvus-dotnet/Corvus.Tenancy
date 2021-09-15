// <copyright file="ITenantBlobContainerClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

// TODO: what's the right namespace structure?
// For non-tenanted storage, we'd go with Corvus.Storage.Azure.Blob. So this seems odd.
namespace Corvus.Azure.Storage.Tenancy
{
    using Corvus.Tenancy;

    using global::Azure.Storage.Blobs;

    /// <summary>
    /// A factory for a <see cref="BlobContainerClient"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="BlobContainerClient"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant via <see cref="BlobStorageTenantExtensions.AddBlobStorageConfiguration(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{string, object}}, string, BlobStorageConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the blob container factory in your container configuration.
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantBlobContainerClientFactory(tenantBlobContainerClientFactoryOptions);
    /// </code>
    /// <para>
    /// <code>
    /// ITenantBlobContainerClientFactory factory;
    ///
    /// BlobContainerClient client = await factory.GetBlobContainerForTenantAsync(tenantProvider.Root, new BlobStorageContainerDefinition("somecontainer"));
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
    public interface ITenantBlobContainerClientFactory : ITenantedStorageContextFactory<BlobContainerClient>
    {
    }
}