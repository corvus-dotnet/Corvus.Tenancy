// <copyright file="ICosmosConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    using Corvus.Extensions.Json;

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
    }
}