// <copyright file="ITenantGremlinContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy
{
    using Corvus.Tenancy;

    using Gremlin.Net.Driver;

    /// <summary>
    /// A factory for a <see cref="GremlinClient"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="GremlinClient"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant via <see cref="GremlinStorageTenantExtensions.AddGremlinConfiguration(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{string, object}}, GremlinContainerDefinition, GremlinConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the Gremlin container factory in your container configuration.
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantGremlinContainerFactory(tenantGremlinContainerFactoryOptions);
    /// </code>
    /// <para>
    /// Now, whenever you want to obtain a gremlin client for a container for a tenant, you simply call <see cref="ITenantedStorageContextFactory{TStorageContext}.GetContextForTenantAsync(ITenant, string)"/>, passing
    /// it the tenant and the container definition you want to use.
    /// </para>
    /// <para>
    /// <code>
    /// ITenantGremlinContainerFactory factory;
    ///
    /// var client = await factory.GetBlobContainerForTenantAsync(tenant, new GremlinContainerDefinition("somedb", "somecontainer"));
    /// </code>
    /// </para>
    /// <para>
    /// If you create containers in this way (rather than just newing them up) then your application can easily be multitented
    /// by ensuring that you always pass the Tenant through your stack, and just default to tenantProvider.Root at the top level.
    /// </para>
    /// <para>
    /// Note that because we have not wrapped the resulting Container in a class of our own, we cannot automatically
    /// implement key rotation.
    /// </para>
    /// </remarks>
    public interface ITenantGremlinContainerFactory : ITenantedStorageContextFactory<GremlinClient>
    {
    }
}