// <copyright file="ITenantCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using Corvus.Tenancy;

    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// A factory for a <see cref="CloudTable"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="CloudTable"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant via <see cref="TableStorageTenantExtensions.AddTableStorageConfiguration(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{string, object}}, string, TableStorageConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the table factory in your container configuration.
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantCloudTableFactory(tenantCloudTableFactoryOptions);
    /// </code>
    /// <para>
    /// When provisioning a new tenant, you will need to ensure that containers corresponding to
    /// the context names you plan to use exist. You can use <see cref="ContainerNameBuilders.MakeUniqueSafeTableContainerName(ITenant, string)"/>
    /// to determine the right names to use for these.
    /// </para>
    /// <para>
    /// Now, whenever you want to obtain a table for a tenant, you simply call <see cref="ITenantedStorageContextFactory{TStorageContext}.GetContextForTenantAsync(Corvus.Tenancy.ITenant, string)"/>, passing
    /// it the tenant and the context name (the logical name of the table) you want to use.
    /// </para>
    /// <para>
    /// <code>
    /// TenantCloudTableFactory factory;
    ///
    /// var repository = await factory.GetContextForTenantAsync(tenant, "sometable");
    /// </code>
    /// </para>
    /// <para>
    /// If you create tables in this way (rather than just newing them up) then your application can easily be multitented
    /// by ensuring that you always pass the Tenant through your stack.
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
    public interface ITenantCloudTableFactory : ITenantedStorageContextFactory<CloudTable>
    {
    }
}