// <copyright file="Tenant.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System.Diagnostics;
    using Corvus.Json;

    /// <summary>
    /// Describes a tenant in a multitenanted system.
    /// </summary>
    [DebuggerDisplay("{name} ({id})")]
    public class Tenant : ITenant
    {
        /// <summary>
        /// The registered content type for the tenant.
        /// </summary>
        public const string RegisteredContentType = "application/vnd.corvus.tenancy.tenant";

        /// <summary>
        /// Initializes a new instance of the <see cref="Tenant"/> class.
        /// </summary>
        /// <param name="id">The <see cref="Id"/>.</param>
        /// <param name="name">The <see cref="Name"/>.</param>
        /// <param name="properties">The <see cref="Properties"/>.</param>
        public Tenant(string id, string name, IPropertyBag properties)
        {
            if (id is null)
            {
                throw new System.ArgumentNullException(nameof(id));
            }

            if (name is null)
            {
                throw new System.ArgumentNullException(nameof(name));
            }

            if (properties is null)
            {
                throw new System.ArgumentNullException(nameof(properties));
            }

            this.Id = id;
            this.Name = name;
            this.Properties = properties;
        }

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IPropertyBag Properties { get; protected set; }

        /// <inheritdoc/>
        public string? ETag { get; set; }

        /// <inheritdoc/>
        public string ContentType => RegisteredContentType;
    }
}