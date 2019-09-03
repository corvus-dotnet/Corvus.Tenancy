// <copyright file="IStorageConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using Corvus.Extensions.Json;

    /// <summary>
    /// Encapsulates configuration for a storage account.
    /// </summary>
    public interface IStorageConfiguration
    {
        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        /// <remarks>
        /// If this is left empty, then the local storage emulator will be used.
        /// </remarks>
        string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the blob storage-specific configuration elements.
        /// </summary>
        BlobStorageConfiguration BlobStorageConfiguration { get; set; }

        /// <summary>
        /// Gets the collection of properties for this configuration.
        /// </summary>
        PropertyBag Properties { get; }
    }
}