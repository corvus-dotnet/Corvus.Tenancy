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
        /// <param name="settingsProvider">The json serializer settings provider.</param>
        public Tenant(IJsonSerializerSettingsProvider settingsProvider)
        {
            if (settingsProvider is null)
            {
                throw new System.ArgumentNullException(nameof(settingsProvider));
            }

            this.Properties = new PropertyBag(settingsProvider.Instance);
        }

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public PropertyBag Properties
        {
            get;
        }

        /// <inheritdoc/>
        [JsonIgnore]
        public string ETag { get; set; }
    }
}