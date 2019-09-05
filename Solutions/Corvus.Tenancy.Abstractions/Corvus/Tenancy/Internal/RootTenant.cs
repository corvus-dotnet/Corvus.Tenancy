// <copyright file="RootTenant.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using Corvus.Extensions.Json;

    /// <summary>
    /// Describes a root tenant in a multitenanted system.
    /// </summary>
    public class RootTenant : Tenant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RootTenant"/> class.
        /// </summary>
        /// <param name="serializerSettingsProvider">The Json Serializer Settings provider.</param>
        public RootTenant(IJsonSerializerSettingsProvider serializerSettingsProvider)
            : base(serializerSettingsProvider)
        {
            this.Id = "Root";
        }
    }
}