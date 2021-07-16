namespace Corvus.Tenancy.Specs.Bindings
{
    using System;

    public class StorageContextTestContext
    {
        public string ContextName { get; } = $"tenancyspecs{Guid.NewGuid()}";
    }
}
