// <copyright file="IRecreatableTenantCosmosContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    using System.Threading.Tasks;
    using Corvus.Tenancy;

    /// <summary>
    /// An interface implemented by factories that provide a mechansim
    /// to recreate the container on demand.
    /// </summary>
    public interface IRecreatableTenantCosmosContainerFactory : ITenantCosmosContainerFactory
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
        Task<RecreatableContainer> GetRecreatableContainerForTenantAsync(ITenant tenant, CosmosContainerDefinition containerDefinition);
    }
}
