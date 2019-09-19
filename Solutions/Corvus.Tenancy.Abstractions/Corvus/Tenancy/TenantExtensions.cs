// <copyright file="TenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;

namespace Corvus.Tenancy
{
    /// <summary>
    /// Extensions for the <see cref="ITenant"/>.
    /// </summary>
    public static class TenantExtensions
    {
        /// <summary>
        /// Gets the id of the parent of a tenant.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <returns>The id of the parent of the specified tenant, or null if this is the <see cref="ITenantProvider.Root"/> tenant.</returns>
        public static string GetParentId(this ITenant tenant)
        {
            if (tenant is null)
            {
                throw new System.ArgumentNullException(nameof(tenant));
            }

            return GetParentId(tenant.Id);
        }

        /// <summary>
        /// Creates an ID for a child from the parent Id and a unique ID.
        /// </summary>
        /// <param name="parentId">The full parent ID.</param>
        /// <param name="childGuid">The unique child ID.</param>
        /// <returns>The combined ID for the child.</returns>
        public static string CreateChildId(this string parentId, string childGuid = null)
        {
            return parentId + "_" + (childGuid ?? Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Gets the id of the parent of a tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>The ID of the parent of the specified tenant, or null if this is the <see cref="ITenantProvider.Root"/> tenant ID.</returns>
        public static string GetParentId(this string tenantId)
        {
            if (tenantId is null)
            {
                throw new System.ArgumentNullException(nameof(tenantId));
            }

            if (tenantId == RootTenant.RootTenantId)
            {
                return null;
            }

            return tenantId.Substring(0, tenantId.LastIndexOf('_'));
        }
    }
}
