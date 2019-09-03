// <copyright file="ITenant.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using Corvus.Extensions.Json;

    /// <summary>
    /// Describes a tenant in a multitenanted system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Any solution that wishes to support tenancy (even if it starts as a single tenanted solution)
    /// should configure the <see cref="ITenantProvider.Root"/> with any properties necessary for the correct
    /// operation of your solution.
    /// </para>
    /// <para>
    /// Tenants can themselves own sub-tenants. This ownership relationship is derived from the path-based nature of the tenant <see cref="Id"/>.
    /// </para>
    /// </remarks>
    public interface ITenant
    {
        /// <summary>
        /// Gets the id of the tenant.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The id must be formed in a path format, from the root tenant to this tenant. It is recommend that you use guids
        /// to identify your tenant, though the ids need only be unique within the parent tenant.
        /// </para>
        /// <para>e.g. <c>RootTenant/22de5c9a-385d-48a0-8ea8-73dca9cdd0db/98c86eb9-dfbf-48ba-ba57-19561fc4114a</c>.</para>
        /// </remarks>
        string Id { get; }

        /// <summary>
        /// Gets the collection of properties for this tenant.
        /// </summary>
        PropertyBag Properties { get; }

        /// <summary>
        /// Gets or sets the ETag of the tenant.
        /// </summary>
        string ETag { get; set; }
    }
}