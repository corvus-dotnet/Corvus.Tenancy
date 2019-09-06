// <copyright file="ICosmosConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    using Corvus.ContentHandling;
    using Corvus.Extensions.Json;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// Encapsulates configuration for a storage account.
    /// </summary>
    public interface ICosmosConfiguration
    {
        /// <summary>
        /// Gets or sets the account URI.
        /// </summary>
        /// <remarks>
        /// If this is left empty, then the local storage emulator will be used.
        /// </remarks>
        string AccountUri { get; set; }

        /// <summary>
        /// Gets or sets overrides for Cosmos container defintion, specific to this configuration.
        /// </summary>
        CosmosContainerDefinition CosmosContainerDefinition { get; set; }

        /// <summary>
        /// Gets the collection of properties for this configuration.
        /// </summary>
        PropertyBag Properties { get; }

        /// <summary>
        /// Gets the content type for the <see cref="ContentFactory"/> pattern.
        /// </summary>
        string ContentType { get; }
    }
}