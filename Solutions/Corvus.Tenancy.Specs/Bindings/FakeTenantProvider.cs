// <copyright file="FakeTenantProvider.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Corvus.Json;
    using Corvus.Tenancy;
    using Corvus.Tenancy.Exceptions;

    internal class FakeTenantProvider : ITenantStore
    {
        public FakeTenantProvider(IPropertyBagFactory propertyBagFactory)
        {
            this.Root = new RootTenant(propertyBagFactory);
        }

        public List<Update> TenantUpdates { get; } = new ();

        public RootTenant Root { get; }

        public Task<ITenant> CreateChildTenantAsync(string parentTenantId, string name)
        {
#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
            throw new NotImplementedException();
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
        }

        public Task<ITenant> CreateWellKnownChildTenantAsync(
            string parentTenantId,
            Guid wellKnownChildTenantGuid,
            string name)
        {
#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
            throw new NotImplementedException();
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
        }

        public Task DeleteTenantAsync(string tenantId)
        {
#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
            throw new NotImplementedException();
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
        }

        public Task<TenantCollectionResult> GetChildrenAsync(string tenantId, int limit = 20, string? continuationToken = null)
        {
#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
            throw new NotImplementedException();
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
        }

        public Task<ITenant> GetTenantAsync(string tenantId, string? etag = null)
        {
            if (tenantId != RootTenant.RootTenantId)
            {
                throw new TenantNotFoundException();
            }

            return Task.FromResult<ITenant>(this.Root);
        }

        public Task<ITenant> UpdateTenantAsync(
            string tenantId,
            string? name = null,
            IEnumerable<KeyValuePair<string, object>>? propertiesToSetOrAdd = null,
            IEnumerable<string>? propertiesToRemove = null)
        {
            this.TenantUpdates.Add(new Update(tenantId, name, propertiesToSetOrAdd, propertiesToRemove));

            return Task.FromResult(default(ITenant) !);
        }

        // TODO: When we're able to use C#9 or later, this would be better as a record.
        public class Update
        {
            public Update(
                string tenantId,
                string? name,
                IEnumerable<KeyValuePair<string, object>>? propertiesToSetOrAdd,
                IEnumerable<string>? propertiesToRemove)
            {
                this.TenantId = tenantId;
                this.Name = name;
                this.PropertiesToSetOrAdd = propertiesToSetOrAdd;
                this.PropertiesToRemove = propertiesToRemove;
            }

            public string TenantId { get; }

            public string? Name { get; }

            public IEnumerable<KeyValuePair<string, object>>? PropertiesToSetOrAdd { get; }

            public IEnumerable<string>? PropertiesToRemove { get; }
        }
    }
}