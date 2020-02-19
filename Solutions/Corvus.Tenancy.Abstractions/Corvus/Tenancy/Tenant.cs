// <copyright file="Tenant.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System;
    using Corvus.Extensions.Json;

    /// <summary>
    /// Describes a tenant in a multitenanted system.
    /// </summary>
    public class Tenant : ITenant
    {
        /// <summary>
        /// The registered content type for the tenant.
        /// </summary>
        public const string RegisteredContentType = "application/vnd.corvus.tenancy.tenant";

        private string? id;

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

            this.Id = string.Empty;
        }

        /// <inheritdoc/>
        public string Id
        {
            get => this.id ?? throw new InvalidOperationException("This tenant has not been supplied with an " + nameof(this.Id));
            set => this.id = value ?? throw new ArgumentNullException();
        }

        /// <inheritdoc/>
        public PropertyBag Properties
        {
            get; set;
        }

        /// <inheritdoc/>
        public string? ETag { get; set; }

        /// <inheritdoc/>
        public string ContentType
        {
            get { return RegisteredContentType; }
        }
    }
}