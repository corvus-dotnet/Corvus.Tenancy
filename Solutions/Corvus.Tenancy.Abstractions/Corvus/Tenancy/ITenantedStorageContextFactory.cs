// <copyright file="ITenantedStorageContextFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System.Threading.Tasks;

    /// <summary>
    /// A source of tenanted storage contexts.
    /// </summary>
    /// <typeparam name="TStorageContext">
    /// The type of storage context (e.g., a blob container, a CosmosDB collection, or a SQL
    /// connection string).
    /// </typeparam>
    public interface ITenantedStorageContextFactory<TStorageContext>
    {
        /// <summary>
        /// Get a storage context within a particular tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to retrieve the context.</param>
        /// <param name="contextName">The name identifying the context to create.</param>
        /// <returns>The tenanted context.</returns>
        /// <remarks>
        /// This caches context instances to ensure that a singleton is used for all request for
        /// the same tenant and context names. A consequence of this is that
        /// <typeparamref name="TStorageContext"/> must always be sharable. This is why it cannot
        /// be a SQL connection object - with SQL, an implementation of this interface can only
        /// return connection strings, which clients must then use to create the actual connection.
        /// </remarks>
        Task<TStorageContext> GetContextForTenantAsync(
            ITenant tenant,
            string contextName);
    }
}