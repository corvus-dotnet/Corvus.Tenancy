// <copyright file="TenantTrackingTenantProviderDecorator.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Corvus.Tenancy;

    public class TenantTrackingTenantProviderDecorator : ITenantStore
    {
        private readonly ITenantStore decoratedProvider;

        public TenantTrackingTenantProviderDecorator(ITenantStore decoratedProvider)
        {
            this.decoratedProvider = decoratedProvider ?? throw new ArgumentNullException(nameof(decoratedProvider));
        }

        public List<ITenant> CreatedTenants { get; } = new List<ITenant>();

        public RootTenant Root => this.decoratedProvider.Root;

        public async Task<ITenant> CreateChildTenantAsync(string parentTenantId, string name)
        {
            ITenant newTenant = await this.decoratedProvider.CreateChildTenantAsync(parentTenantId, name).ConfigureAwait(false);
            this.CreatedTenants.Add(newTenant);
            return newTenant;
        }

        public async Task<ITenant> CreateWellKnownChildTenantAsync(string parentTenantId, Guid wellKnownChildTenantGuid, string name)
        {
            ITenant newTenant = await this.decoratedProvider.CreateWellKnownChildTenantAsync(
                parentTenantId,
                wellKnownChildTenantGuid,
                name).ConfigureAwait(false);

            this.CreatedTenants.Add(newTenant);
            return newTenant;
        }

        public Task DeleteTenantAsync(string tenantId) => this.decoratedProvider.DeleteTenantAsync(tenantId);

        public Task<TenantCollectionResult> GetChildrenAsync(string tenantId, int limit = 20, string? continuationToken = null)
            => this.decoratedProvider.GetChildrenAsync(tenantId, limit, continuationToken);

        public Task<ITenant> GetTenantAsync(string tenantId, string? eTag = null)
            => this.decoratedProvider.GetTenantAsync(tenantId, eTag);

        public Task<ITenant> UpdateTenantAsync(string tenantId, string? name, IEnumerable<KeyValuePair<string, object>>? propertiesToSetOrAdd = null, IEnumerable<string>? propertiesToRemove = null)
            => this.decoratedProvider.UpdateTenantAsync(tenantId, name, propertiesToSetOrAdd, propertiesToRemove);
    }
}