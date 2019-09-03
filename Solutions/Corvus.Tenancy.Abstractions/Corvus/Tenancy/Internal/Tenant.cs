// <copyright file="Tenant.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using Corvus.Extensions.Json;
    using Newtonsoft.Json;

    /// <summary>
    /// Describes a tenant in a multitenanted system.
    /// </summary>
    public class Tenant : ITenant
    {
        /// <summary>
        /// The registered content type for the tenant.
        /// </summary>
        public const string RegisteredContentType = "application/vnd.corvus.tenancy.tenant";

        /// <summary>
        /// Initializes a new instance of the <see cref="Tenant"/> class.
        /// </summary>
        public Tenant()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tenant"/> class.
        /// </summary>
        /// <param name="properties">The property bag for the tenant.</param>
        public Tenant(PropertyBag properties)
        {
            this.Properties = properties;
        }

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public PropertyBag Properties
        {
            get;
            set;
        }

        /// <inheritdoc/>
        [JsonIgnore]
        public string ETag { get; set; }
    }
}
