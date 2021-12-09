// <copyright file="TenantStoreExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for the ITenantProvider interface.
    /// </summary>
    public static class TenantStoreExtensions
    {
        /// <summary>
        /// Gets the tenants for a given set of tenant IDs.
        /// </summary>
        /// <param name="tenantProvider">The underlying tenant provider to use.</param>
        /// <param name="tenantIds">The ids of the tenants to retrieve.</param>
        /// <returns>A task that produces the specified tenants.</returns>
        public static async Task<ITenant[]> GetTenantsAsync(this ITenantProvider tenantProvider, IEnumerable<string> tenantIds)
        {
            ArgumentNullException.ThrowIfNull(tenantIds);

            IEnumerable<Task<ITenant>> getTenantTasks = tenantIds.Select(tenantId => tenantProvider.GetTenantAsync(tenantId));

            return await Task.WhenAll(getTenantTasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves all children of the specified tenant.
        /// </summary>
        /// <param name="tenantStore">The underlying tenant store to use.</param>
        /// <param name="tenantId">The Id of the parent tenant.</param>
        /// <returns>The list of child tenants.</returns>
        /// <remarks>
        /// This method will make as many calls to <see cref="ITenantStore.GetChildrenAsync(string, int, string)"/> as
        /// needed to retrieve all of the child tenants. If there is a possibility that there's a large number of child
        /// tenants and the underlying provider is likely to be making expensive calls to retrieve tenants, this method
        /// should be used with extreme caution.
        /// </remarks>
        public static IAsyncEnumerable<string> EnumerateAllChildrenAsync(this ITenantStore tenantStore, string tenantId)
        {
            ArgumentNullException.ThrowIfNull(tenantId);

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"{nameof(tenantId)} must not be empty", nameof(tenantId));
            }

            return EnumerateAllChildrenInternalAsync(tenantStore, tenantId);
        }

        /// <summary>
        /// Retrieves all child tenants of the specified tenant.
        /// </summary>
        /// <param name="tenantStore">The underlying tenant store to use.</param>
        /// <param name="tenantId">The Id of the parent tenant.</param>
        /// <returns>The list of child tenants.</returns>
        /// <remarks>
        /// This method will make as many calls to <see cref="ITenantStore.GetChildrenAsync(string, int, string)"/> as
        /// needed to retrieve all of the child tenants. If there is a possibility that there's a large number of child
        /// tenants and the underlying provider is likely to be making expensive calls to retrieve tenants, this method
        /// should be used with extreme caution.
        /// </remarks>
        public static IAsyncEnumerable<ITenant> EnumerateAllChildTenantsAsync(this ITenantStore tenantStore, string tenantId)
        {
            ArgumentNullException.ThrowIfNull(tenantId);

            return EnumerateAllChildTenantsInternalAsync(tenantStore, tenantId);
        }

        /// <summary>
        /// Internal method corresponding to <see cref="EnumerateAllChildrenAsync(ITenantStore, string)"/>. The public method
        /// verifies the parameters are valid and this method implements the enumeration.
        /// </summary>
        private static async IAsyncEnumerable<string> EnumerateAllChildrenInternalAsync(
            ITenantStore tenantStore,
            string tenantId)
        {
            string? continuationToken = null;
            const int limit = 100;

            do
            {
                TenantCollectionResult results = await tenantStore.GetChildrenAsync(
                    tenantId,
                    limit,
                    continuationToken).ConfigureAwait(false);

                foreach (string tenant in results.Tenants)
                {
                    yield return tenant;
                }

                continuationToken = results.ContinuationToken;
            }
            while (!string.IsNullOrEmpty(continuationToken));
        }

        /// <summary>
        /// Internal method corresponding to <see cref="EnumerateAllChildTenantsAsync(ITenantStore, string)"/>. The public method
        /// verifies the parameters are valid and this method implements the enumeration.
        /// </summary>
        private static async IAsyncEnumerable<ITenant> EnumerateAllChildTenantsInternalAsync(
            ITenantStore tenantStore,
            string tenantId)
        {
            string? continuationToken = null;
            const int limit = 100;

            do
            {
                TenantCollectionResult results = await tenantStore.GetChildrenAsync(
                    tenantId,
                    limit,
                    continuationToken).ConfigureAwait(false);

                foreach (string childTenantId in results.Tenants)
                {
                    yield return await tenantStore.GetTenantAsync(childTenantId).ConfigureAwait(false);
                }

                continuationToken = results.ContinuationToken;
            }
            while (!string.IsNullOrEmpty(continuationToken));
        }
    }
}
