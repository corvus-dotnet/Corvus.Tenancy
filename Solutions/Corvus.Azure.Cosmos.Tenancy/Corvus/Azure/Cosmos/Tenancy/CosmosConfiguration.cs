// <copyright file="CosmosConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    using System;
    using Corvus.Extensions.Json;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    /// <summary>
    /// Encapsulates configuration for a container in a specific Cosmos account.
    /// </summary>
    public class CosmosConfiguration : ICosmosConfiguration
    {
        /// <summary>
        /// The registered content type for the client configuration.
        /// </summary>
        public const string RegisteredContentType = "application/vnd.corvus.azure.cosmos.tenancy.cosmosconfiguration";

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosConfiguration"/> class.
        /// </summary>
        public CosmosConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosConfiguration"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for the configuration.</param>
        public CosmosConfiguration(IServiceProvider serviceProvider)
        {
            this.CosmosContainerDefinition = new CosmosContainerDefinition();
            IJsonSerializerSettingsProvider serializerSettingsProvider = serviceProvider.GetService<IJsonSerializerSettingsProvider>();
            JsonSerializerSettings serializerSettings = serializerSettingsProvider?.Instance ?? JsonConvert.DefaultSettings?.Invoke();
            this.Properties = new PropertyBag(serializerSettings);
        }

        /// <inheritdoc/>
        public string AccountUri { get; set; }

        /// <inheritdoc/>
        public CosmosContainerDefinition CosmosContainerDefinition { get; set; }

        /// <inheritdoc/>
        public PropertyBag Properties { get; set; }
    }
}