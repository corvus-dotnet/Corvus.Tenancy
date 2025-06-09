// <copyright file="RootTenant.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System;
    using System.Collections.Generic;
    using Corvus.Json;

    /// <summary>
    /// Describes a root tenant in a multi-tenanted system.
    /// </summary>
    public class RootTenant : Tenant
    {
        /// <summary>
        /// The name of the root tenant.
        /// </summary>
        /// <remarks>
        /// This should not be used to check if an <see cref="ITenant"/> is the Root tenant - compare using <see cref="RootTenantId"/>.
        /// </remarks>
        public const string RootTenantName = "Root";

        /// <summary>
        /// The root tenant ID.
        /// </summary>
        /// <remarks>
        /// This is encoded to <c>f26450ab1668784bb327951c8b08f347</c>.
        /// </remarks>
        public static readonly string RootTenantId = Guid.Parse("AB5064F2-6816-4B78-B327-951C8B08F347").EncodeGuid();
        private readonly IPropertyBagFactory propertyBagFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RootTenant"/> class.
        /// </summary>
        /// <param name="propertyBagFactory">Enables property bag creation and update.</param>
        public RootTenant(IPropertyBagFactory propertyBagFactory)
            : base(RootTenantId, RootTenantName, propertyBagFactory.Create(PropertyBagValues.Empty))
        {
            this.propertyBagFactory = propertyBagFactory;
        }

        /// <summary>
        /// Updates the root tenant's properties. This is a special case because it supports synchronous updates.
        /// </summary>
        /// <param name="propertiesToSetOrAdd">
        /// Key and value pairs to update in or add to the tenant's <see cref="ITenant.Properties"/>,
        /// or null if the caller wants only to remove properties.
        /// </param>
        /// <param name="propertiesToRemove">
        /// The keys of the properties to remove from the tenant's <see cref="ITenant.Properties"/>,
        /// or null if the caller only wants to add or modify properties.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// At least one of <c>propertiesToSetOrAdd</c> or <c>propertiesToRemove</c> must be non-null.
        /// If both are null, the call would have no effect, so we throw this exception.
        /// </exception>
        /// <remarks>Note that this will update the ETag of the tenant.</remarks>
        public void UpdateProperties(
            IEnumerable<KeyValuePair<string, object>>? propertiesToSetOrAdd = null,
            IEnumerable<string>? propertiesToRemove = null)
        {
            this.Properties = this.propertyBagFactory.CreateModified(
                this.Properties,
                propertiesToSetOrAdd,
                propertiesToRemove);
        }
    }
}