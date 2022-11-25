// <copyright file="LegacyV2SqlConfigurationKeyNaming.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Sql.Tenancy;

/// <summary>
/// Returns the tenant property key names used by the V2 libraries for SQL configuration.
/// </summary>
public static class LegacyV2SqlConfigurationKeyNaming
{
    /// <summary>
    /// Gets the key in the tenant storage properties that the V2 Corvus tenanted storage libraries
    /// would have used for the specified logical database name.
    /// </summary>
    /// <param name="logicalDatabaseName">The logical database name.</param>
    /// <returns>
    /// The tenant property key that the V2 libraries would have used for the settings for this
    /// logical container.
    /// </returns>
    public static string? TenantPropertyKeyForLogicalDatabase(string logicalDatabaseName)
        => $"StorageConfiguration__{logicalDatabaseName}";
}