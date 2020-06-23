// <copyright file="ITenantCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using System.Threading.Tasks;
    using Corvus.Tenancy;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Azure.Storage.Blob;

    /// <summary>
    /// A factory for a <see cref="CloudTable"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="CloudTable"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant via <see cref="TableStorageTenantExtensions.AddTableStorageConfiguration(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{string, object}}, TableStorageTableDefinition, TableStorageConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the table factory and the configuration account key provider in your table configuration (assuming you have added a standard ConfigurationRoot to your solution).
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantCloudTableFactory();
    /// serviceCollection.AddTenantConfigurationAccountKeyProvider();
    /// </code>
    /// <para>
    /// <code>
    /// TenantCloudTableFactory factory;
    ///
    /// var repository = await factory.GetTableForTenantAsync(tenantProvider.Root, new TableStorageContainerDefinition("sometable"));
    /// </code>
    /// </para>
    /// <para>
    /// If you create tables in this way (rather than just newing them up) then your application can easily be multitented
    /// by ensuring that you always pass the Tenant through your stack, and just default to tenantProvider.Root at the top level.
    /// </para>
    /// <para>
    /// Note that it will be possible for code that obtains a CloudTable in this way to use the resulting object to access
    /// the CloudTableClient and thus access other tables contained in the same account. As such these objects should only ever be
    /// handed to trusted code.
    /// </para>
    /// <para>
    /// Note also that because we have not wrapped the resulting CloudTable in a class of our own, we cannot automatically
    /// implement key rotation.
    /// </para>
    /// </remarks>
    public interface ITenantCloudTableFactory
    {
        /// <summary>
        /// Get a blob table for a tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to retrieve the table.</param>
        /// <param name="tableDefinition">The details of the table to create.</param>
        /// <returns>The table instance for the tenant.</returns>
        /// <remarks>
        /// This caches table instances to ensure that a singleton is used for all request for the same tenant and table definition.
        /// </remarks>
        Task<CloudTable> GetTableForTenantAsync(ITenant tenant, TableStorageTableDefinition tableDefinition);
    }
}