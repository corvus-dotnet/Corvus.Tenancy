// <copyright file="LegacyV2BlobConfigurationKeyNaming.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage.Tenancy;

/// <summary>
/// Returns the tenant property key names used by the V2 libraries for Azure Blob Storage
/// configuration.
/// </summary>
public static class LegacyV2BlobConfigurationKeyNaming
{
    /// <summary>
    /// Gets the key in the tenant storage properties that the V2 Corvus tenanted storage libraries
    /// would have used for the specified logical container name.
    /// </summary>
    /// <param name="logicalContainerName">The logical container name.</param>
    /// <returns>
    /// The tenant property key that the V2 libraries would have used for the settings for this
    /// logical container.
    /// </returns>
    public static string TenantPropertyKeyForLogicalContainer(string logicalContainerName)
        => $"StorageConfiguration__{logicalContainerName}";
}