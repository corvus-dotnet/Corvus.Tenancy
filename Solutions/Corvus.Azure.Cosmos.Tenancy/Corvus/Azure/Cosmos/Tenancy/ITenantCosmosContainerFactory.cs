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
    /// configuration comes from the tenant via the <see cref="CosmosStorageTenantExtensions.SetDefaultCosmosConfiguration(ITenant, ICosmosConfiguration)"/>
    /// and <see cref="CosmosStorageTenantExtensions.SetCosmosConfiguration(ITenant, CosmosContainerDefinition, ICosmosConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and a default configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the Cosmos container factory and the configuration account key provider in your container configuration (assuming you have added a standard ConfigurationRoot to your solution).
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantCosmosContainerFactory();
    /// serviceCollection.AddTenantConfigurationAccountKeyProvider();
    /// </code>
    /// <para>
    /// Then, also as part of your startup, you can configure the Root tenant with some standard configuration. Note that this will typically be done through the container initialization extension method <see cref="TenantCosmosServiceCollectionExtensions.AddTenantCosmosContainerFactory(Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Action{System.IServiceProvider, ITenant})"/>.
    /// </para>
    /// <para>
    /// Now, whenever you want to obtain a blob container for a tenant, you simply call <see cref="ITenantCosmosContainerFactory.GetContainerForTenantAsync(ITenant, CosmosContainerDefinition)"/>, passing
    /// it the tenant and the container definition you want to use.
    /// </para>
    /// <para>
    /// <code>
    /// TenantCloudBlobContainerFactory factory;
    ///
    /// var repository = await factory.GetBlobContainerForTenantAsync(Tenant.Root, new BlobStorageContainerDefinition("somecontainer"));
    /// </code>
    /// </para>
    /// <para>
    /// If you create containers in this way (rather than just newing them up) then your application can easily be multitented
    /// by ensuring that you always pass the Tenant through your stack, and just default to Tenant.Root at the top level.
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