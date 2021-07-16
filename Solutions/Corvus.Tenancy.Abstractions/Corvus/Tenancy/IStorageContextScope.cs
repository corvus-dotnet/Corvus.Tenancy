// <copyright file="IStorageContextScope.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    /// <summary>
    /// Defines the scope in which context-specific storage can be retrieved from an
    /// <see cref="IStorageContextFactory{TStorageContext, TConfiguration}"/>.
    /// </summary>
    /// <typeparam name="TConfiguration">
    /// The configuration type that this scope knows how to return.
    /// </typeparam>
    public interface IStorageContextScope<TConfiguration>
    {
        /// <summary>
        /// Produces a unique cache key based on this scope and a particular storage context name.
        /// </summary>
        /// <param name="storageContextName">
        /// The name identifying the storage context required.
        /// </param>
        /// <returns>
        /// A name that is unique to the particular combination of scope and name.
        /// </returns>
        string CreateCacheKeyForContext(string storageContextName);

        /// <summary>
        /// Gets the configuration for a named storage context.
        /// </summary>
        /// <param name="storageContextName">
        /// The name identifying the storage context required.
        /// </param>
        /// <returns>
        /// The configuration for this storage context in this scope.
        /// </returns>
        TConfiguration GetConfigurationForContext(string storageContextName);
    }
}