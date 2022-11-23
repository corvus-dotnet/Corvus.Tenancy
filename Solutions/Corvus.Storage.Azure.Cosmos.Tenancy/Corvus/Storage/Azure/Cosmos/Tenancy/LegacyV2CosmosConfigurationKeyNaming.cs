// <copyright file="LegacyV2CosmosConfigurationKeyNaming.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.Cosmos.Tenancy;

/// <summary>
/// Returns the tenant property key names used by the V2 libraries for Cosmos DB configuration.
/// </summary>
public static class LegacyV2CosmosConfigurationKeyNaming
{
    /// <summary>
    /// Gets the key in the tenant storage properties that the V2 Corvus tenanted storage libraries
    /// would have used for the specified logical database and container names.
    /// </summary>
    /// <param name="logicalDatabaseName">The logical database name.</param>
    /// <param name="logicalContainerName">The logical container name.</param>
    /// <returns>
    /// The tenant property key that the V2 libraries would have used for the settings for this
    /// logical container.
    /// </returns>
    public static string? TenantPropertyKeyForLogicalDatabaseAndContainer(
        string logicalDatabaseName,
        string logicalContainerName)
        => $"StorageConfiguration__{logicalDatabaseName}__{logicalContainerName}";
}