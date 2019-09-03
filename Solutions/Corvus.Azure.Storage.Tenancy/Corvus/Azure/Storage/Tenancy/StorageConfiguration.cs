// <copyright file="StorageConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using System;
    using Corvus.Extensions.Json;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    /// <summary>
    /// Encapsulates configuration for a storage account.
    /// </summary>
    public class StorageConfiguration : IStorageConfiguration
    {
        /// <summary>
        /// The registered content type for the client configuration.
        /// </summary>
        public const string RegisteredContentType = "application/vnd.corvus.azure.storage.tenancy.storageconfiguration";

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageConfiguration"/> class.
        /// </summary>
        public StorageConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageConfiguration"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for the configuration.</param>
        public StorageConfiguration(IServiceProvider serviceProvider)
        {
            this.BlobStorageConfiguration = new BlobStorageConfiguration();
            IJsonSerializerSettingsProvider serializerSettingsProvider = serviceProvider.GetService<IJsonSerializerSettingsProvider>();
            JsonSerializerSettings serializerSettings = serializerSettingsProvider?.Instance ?? JsonConvert.DefaultSettings?.Invoke();
            this.Properties = new PropertyBag(serializerSettings);
        }

        /// <inheritdoc/>
        public string AccountName { get; set; }

        /// <inheritdoc/>
        public BlobStorageConfiguration BlobStorageConfiguration { get; set; }

        /// <inheritdoc/>
        public PropertyBag Properties { get; set; }
    }
}
