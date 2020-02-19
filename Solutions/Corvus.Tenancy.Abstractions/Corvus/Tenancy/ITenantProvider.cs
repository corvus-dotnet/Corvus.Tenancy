// <copyright file="ITenantProvider.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System.Threading.Tasks;
    using Corvus.Tenancy.Exceptions;

    /// <summary>
    /// Provides tenant information.
    /// </summary>
    public interface ITenantProvider
    {
        /// <summary>
        /// Gets the root tenant.
        /// </summary>
        ITenant Root { get; }

        /// <summary>
        /// Gets the tenant for a given tenant ID.
        /// </summary>
        /// <param name="tenantId">The id of the tenant for which to get the parent.</param>
        /// <param name="eTag">
        /// An optional ETag. If the etag matches, this method will throw <see cref="TenantNotModifiedException"/>.
        /// </param>
        /// <returns>A task that produces the specified tenant.</returns>
        /// <exception cref="TenantNotModifiedException">
        /// Thrown if the stored tenant has the same etag as was passed.
        /// </exception>
        /// <exception cref="TenantNotFoundException">
        /// Thrown if no tenant with the specified id can be found.
        /// </exception>
        Task<ITenant> GetTenantAsync(string tenantId, string? eTag = null);

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
        /// <returns>The tenant that was created.</returns>
        Task<ITenant> CreateChildTenantAsync(string parentTenantId);

        /// <summary>
        /// Updates a tenant.
        /// </summary>
        /// <param name="tenant">The tenant to update.</param>
        /// <returns>The updated tenant.</returns>
        /// <remarks>Note that this will update the ETag of the tenant.</remarks>
        Task<ITenant> UpdateTenantAsync(ITenant tenant);

        /// <summary>
        /// Deletes the given tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A <see cref="Task"/> which completes once the tenant is deleted.</returns>
        Task DeleteTenantAsync(string tenantId);
    }
}
