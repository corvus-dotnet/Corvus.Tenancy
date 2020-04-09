namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using System.Threading.Tasks;
    using Corvus.Tenancy;
    using Corvus.Tenancy.Exceptions;

    internal class FakeTenantProvider : ITenantProvider
    {
        public FakeTenantProvider(RootTenant rootTenant)
        {
            this.Root = rootTenant;
        }

        public ITenant Root { get; }

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

            return Task.FromResult(this.Root);
        }

        public Task<ITenant> UpdateTenantAsync(ITenant tenant)
        {
#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
            throw new NotImplementedException();
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
        }
    }
}