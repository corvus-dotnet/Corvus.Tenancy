// <copyright file="ITenantStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Corvus.Tenancy.Exceptions;

    /// <summary>
    /// Provides the ability to explore the hierarchy of a tenant store, and update tenant details.
    /// </summary>
    public interface ITenantStore : ITenantProvider
    {
        /// <summary>
        /// Gets the child tenants for a given tenant.
        /// </summary>
        /// <param name="tenantId">The id of the tenant for which to get the direct children.</param>
        /// <param name="limit">The maximum number of children to get in a single request.</param>
        /// <param name="continuationToken">A continuation token to continue reading the next batch.</param>
        /// <returns>The list of tenants who are children of that tenant.</returns>
        Task<TenantCollectionResult> GetChildrenAsync(string tenantId, int limit = 20, string? continuationToken = null);

        /// <summary>
        /// Creates a child tenant for a parent.
        /// </summary>
        /// <param name="parentTenantId">The id of the tenant in which to create the child tenant.</param>
        /// <param name="name">The name of the child tenant.</param>
        /// <returns>The tenant that was created.</returns>
        Task<ITenant> CreateChildTenantAsync(string parentTenantId, string name);

        /// <summary>
        /// Creates a child tenant for a parent using a well known identifier as the basis of the new tenant's Id.
        /// </summary>
        /// <param name="parentTenantId">The id of the tenant in which to create the child tenant.</param>
        /// <param name="wellKnownChildTenantGuid">The well known identifier to use when constructing the new tenant's Id.</param>
        /// <param name="name">The name of the child tenant.</param>
        /// <returns>The tenant that was created.</returns>
        /// <exception cref="TenantNotFoundException">
        /// No tenant matching <paramref name="parentTenantId"/> was found.
        /// </exception>
        /// <exception cref="TenantConflictException">
        /// A tenant with the id <paramref name="wellKnownChildTenantGuid"/> already exists.
        /// </exception>
        Task<ITenant> CreateWellKnownChildTenantAsync(string parentTenantId, Guid wellKnownChildTenantGuid, string name);

        /// <summary>
        /// Updates a tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to update.</param>
        /// <param name="name">
        /// The name to set for this tenant, or null if the caller wants only to add, modify, or
        /// remove properties.
        /// </param>
        /// <param name="propertiesToSetOrAdd">
        /// Key and value pairs to update in or add to the tenant's <see cref="ITenant.Properties"/>,
        /// or null if the caller wants only to set the name or remove properties.
        /// </param>
        /// <param name="propertiesToRemove">
        /// The keys of the properties to remove from the tenant's <see cref="ITenant.Properties"/>,
        /// or null if the caller only wants to set the name or add or modify properties.
        /// </param>
        /// <returns>The updated tenant.</returns>
        /// <exception cref="ArgumentNullException">
        /// At least one of <c>name</c>, <c>propertiesToSetOrAdd</c>, or <c>propertiesToRemove</c>
        /// must be non-null. If all are null, the call would have no effect, so we throw this
        /// exception.
        /// </exception>
        /// <remarks>Note that this will update the ETag of the tenant.</remarks>
        Task<ITenant> UpdateTenantAsync(
            string tenantId,
            string? name = null,
            IEnumerable<KeyValuePair<string, object>>? propertiesToSetOrAdd = null,
            IEnumerable<string>? propertiesToRemove = null);

        /// <summary>
        /// Deletes the given tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A <see cref="Task"/> which completes once the tenant is deleted.</returns>
        Task DeleteTenantAsync(string tenantId);
    }
}