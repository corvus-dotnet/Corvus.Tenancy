// <copyright file="TenantedStorageContextFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System.Threading.Tasks;

    /// <summary>
    /// A tenanted storage context factory, implemented as an adapter over an
    /// <see cref="IStorageContextFactory{TStorageContext, TConfiguration}"/>.
    /// </summary>
    /// <typeparam name="TStorageContext">
    /// The type of storage context (e.g., a blob container, a CosmosDB collection, or a SQL
    /// connection string).
    /// </typeparam>
    /// <typeparam name="TConfiguration">
    /// The type containing the information identifying a particular physical, tenant-specific
    /// instance of a container.
    /// </typeparam>
    public class TenantedStorageContextFactory<TStorageContext, TConfiguration>
    {
        private readonly IStorageContextFactory<TStorageContext, TConfiguration> underlyingFactory;

        /// <summary>
        /// Creates a <see cref="TenantedStorageContextFactory{TStorageContext, TConfiguration}"/>.
        /// </summary>
        /// <param name="underlyingFactory">
        /// The underlying factory.
        /// </param>
        public TenantedStorageContextFactory(
            IStorageContextFactory<TStorageContext, TConfiguration> underlyingFactory)
        {
            this.underlyingFactory = underlyingFactory;
        }

        /// <summary>
        /// Get a named storage context for a particular tenant..
        /// </summary>
        /// <param name="tenant">The tenant for which to retrieve the container.</param>
        /// <param name="contextName">The details of the container to create.</param>
        /// <returns>The container instance for the tenant.</returns>
        /// <remarks>
        /// This caches container instances to ensure that a singleton is used for all request for the same tenant and container definition.
        /// </remarks>
        public Task<TStorageContext> GetContextForTenantAsync(
               ITenant tenant,
               string contextName)
        {
            return this.underlyingFactory.GetContextForTenantAsync(new TenantScope(tenant), contextName);
        }

        private class TenantScope : IStorageContextScope<TConfiguration>
        {
            private readonly ITenant tenant;

            public TenantScope(ITenant tenant)
            {
                this.tenant = tenant;
            }

            public string CreateCacheKeyForContext(string storageContextName)
            {
                return $"{this.tenant.Id.ToLowerInvariant()}-{storageContextName}";
            }

            public TConfiguration GetConfigurationForContext(string storageContextName)
            {
                return this.tenant.GetStorageContextConfiguration<TConfiguration>(storageContextName);
            }
        }
    }
}
