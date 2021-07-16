// <copyright file="IStorageContextFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System.Threading.Tasks;

    /// <summary>
    /// A source of storage contexts.
    /// </summary>
    /// <typeparam name="TStorageContext">
    /// The type of storage context (e.g., a blob container, a CosmosDB collection, or a SQL
    /// connection string).
    /// </typeparam>
    /// <typeparam name="TConfiguration">
    /// The type containing the information identifying a particular physical, tenant-specific
    /// instance of a context.
    /// </typeparam>
    public interface IStorageContextFactory<TStorageContext, TConfiguration>
    {
        /// <summary>
        /// Get a storage context within a particular scope.
        /// </summary>
        /// <param name="scope">The scope (e.g. tenant) for which to retrieve the context.</param>
        /// <param name="contextName">The name identifying the context to create.</param>
        /// <returns>The context for the scope.</returns>
        /// <remarks>
        /// This caches context instances to ensure that a singleton is used for all request for
        /// the same tenant and context names. A consequence of this is that
        /// <typeparamref name="TStorageContext"/> must always be sharable. This is why it cannot
        /// be a SQL connection object - with SQL, an implementation of this interface can only
        /// return connection strings, which clients must then use to create the actual connection.
        /// </remarks>
        Task<TStorageContext> GetContextForTenantAsync(
               IStorageContextScope<TConfiguration> scope,
               string contextName);
    }
}