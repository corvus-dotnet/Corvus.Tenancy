// <copyright file="ITenantProvider.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System.Threading.Tasks;
    using Corvus.Tenancy.Exceptions;

    /// <summary>
    /// Provides access to tenant information by id.
    /// </summary>
    public interface ITenantProvider
    {
        /// <summary>
        /// Gets the root tenant.
        /// </summary>
        RootTenant Root { get; }

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
    }
}