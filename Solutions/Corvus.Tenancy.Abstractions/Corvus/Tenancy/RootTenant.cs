// <copyright file="RootTenant.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System;
    using Corvus.Extensions.Json;

    /// <summary>
    /// Describes a root tenant in a multitenanted system.
    /// </summary>
    public class RootTenant : Tenant
    {
        /// <summary>
        /// The root tenant ID.
        /// </summary>
        public static readonly string RootTenantId = Guid.Parse("AB5064F2-6816-4B78-B327-951C8B08F347").EncodeGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="RootTenant"/> class.
        /// </summary>
        /// <param name="serializerSettingsProvider">The Json Serializer Settings provider.</param>
        public RootTenant(IJsonSerializerSettingsProvider serializerSettingsProvider)
            : base(serializerSettingsProvider)
        {
            this.Id = RootTenantId;
        }
    }
}