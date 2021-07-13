// <copyright file="ITenantCosmosContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    using System.Threading.Tasks;
    using Corvus.Tenancy;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// A factory for a <see cref="Container"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="Container"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant via <see cref="CosmosStorageTenantExtensions.AddCosmosConfiguration(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{string, object}}, CosmosContainerDefinition, CosmosConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the Cosmos container factory in your container configuration.
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantCosmosContainerFactory(tenantCosmosContainerFactoryOptions);
    /// </code>
    /// <para>
    /// Now, whenever you want to obtain a blob container for a tenant, you simply call <see cref="GetContainerForTenantAsync(ITenant, CosmosContainerDefinition)"/>, passing
    /// it the tenant and the container definition you want to use.
    /// </para>
    /// <para>
    /// <code>
    /// TenantCosmosContainerFactory factory;
    ///
    /// Container container = await factory.GetContainerForTenantAsync(tenant, new CosmosContainerDefinition("somedb", "somecontainer"));
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
    public interface ITenantCosmosContainerFactory
    {
        /// <summary>
        /// Get a Cosmos container for a tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to retrieve the container.</param>
        /// <param name="containerDefinition">The details of the container to create.</param>
        /// <returns>The container instance for the tenant.</returns>
        /// <remarks>
        /// This caches container instances to ensure that a singleton is used for all request for the same tenant and container definition.
        /// </remarks>
        Task<Container> GetContainerForTenantAsync(ITenant tenant, CosmosContainerDefinition containerDefinition);
    }
}