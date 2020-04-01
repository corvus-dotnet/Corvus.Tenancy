// <copyright file="ITenantGremlinContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy
{
    using System.Threading.Tasks;
    using Corvus.Tenancy;
    using Gremlin.Net.Driver;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A factory for a <see cref="GremlinClient"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="GremlinClient"/> for a specific
    /// <see cref="ITenant"/>. It uses a KeyVault to get the storage account key for the tenant, and the
    /// configuration comes from the tenant via <see cref="GremlinStorageTenantExtensions.SetGremlinConfiguration(ITenant, GremlinContainerDefinition, GremlinConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the Gremlin container factory and the configuration account key provider in your container configuration (assuming you have added a standard ConfigurationRoot to your solution).
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantGremlinContainerFactory();
    /// </code>
    /// <para>
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="TenancyGremlinServiceCollectionExtensions.AddTenantGremlinContainerFactory(Microsoft.Extensions.DependencyInjection.IServiceCollection, TenantGremlinContainerFactoryOptions)"/>.
    /// </para>
    /// <para>
    /// Now, whenever you want to obtain a gremlin client for a container for a tenant, you simply call <see cref="ITenantGremlinContainerFactory.GetClientForTenantAsync(ITenant, GremlinContainerDefinition)"/>, passing
    /// it the tenant and the container definition you want to use.
    /// </para>
    /// <para>
    /// <code>
    /// TenantCloudBlobContainerFactory factory;
    ///
    /// var repository = await factory.GetBlobContainerForTenantAsync(tenantProvider.Root, new BlobStorageContainerDefinition("somecontainer"));
    /// </code>
    /// </para>
    /// <para>
    /// If you create containers in this way (rather than just newing them up) then your application can easily be multitented
    /// by ensuring that you always pass the Tenant through your stack, and just default to tenantProvider.Root at the top level.
    /// </para>
    /// <para>
    /// Note that it will be possible for code that obtains a Container in this way to use the resulting object to access
    /// the CloudBlobClient and thus access other blob contains in the same container. As such these objects should only ever be
    /// handed to trusted code.
    /// </para>
    /// <para>
    /// Note also that because we have not wrapped the resulting Container in a class of our own, we cannot automatically
    /// implement key rotation.
    /// </para>
    /// </remarks>
    public interface ITenantGremlinContainerFactory
    {
        /// <summary>
        /// Gets a gremlin client for a particular tenant and container definition.
        /// </summary>
        /// <param name="tenant">The tenant for which to get the client.</param>
        /// <param name="containerDefinition">The definition of the gremlin container.</param>
        /// <returns>A <see cref="Task{TResult}"/> which, when complete, returns the gremlin client.</returns>
        Task<GremlinClient> GetClientForTenantAsync(ITenant tenant, GremlinContainerDefinition containerDefinition);
    }
}