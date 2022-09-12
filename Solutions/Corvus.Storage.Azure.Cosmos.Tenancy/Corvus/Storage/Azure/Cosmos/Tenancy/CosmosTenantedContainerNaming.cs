// <copyright file="CosmosTenantedContainerNaming.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.Cosmos.Tenancy;

using Corvus.Tenancy;

/// <summary>
/// Converts plain text names for Azure Cosmos DB database and container into tenant-specific names.
/// </summary>
public static class CosmosTenantedContainerNaming
{
    /// <summary>
    /// Make a Cosmos database name that is unique for this combination of tenant and logical
    /// container name.
    /// </summary>
    /// <param name="tenant">The tenant for which to generate a name.</param>
    /// <param name="databaseName">The plain text name for the Cosmos database.</param>
    /// <returns>The encoded name.</returns>
    public static string GetTenantSpecificDatabaseNameFor(ITenant tenant, string databaseName)
        => GetTenantSpecificDatabaseNameFor(tenant.Id, databaseName);

    /// <summary>
    /// Make a Cosmos database name that is unique for this combination of tenant and logical
    /// container name.
    /// </summary>
    /// <param name="tenantId">The id of the tenant for which to generate a name.</param>
    /// <param name="databaseName">The plain text name for the Cosmos database.</param>
    /// <returns>The encoded name.</returns>
    public static string GetTenantSpecificDatabaseNameFor(string tenantId, string databaseName)
        => $"{tenantId.ToLowerInvariant()}-{databaseName}";

    /// <summary>
    /// Make a container name that is unique for this combination of tenant and logical container
    /// name.
    /// </summary>
    /// <param name="tenant">The tenant for which to generate a name.</param>
    /// <param name="containerName">The plain text name for the Cosmos container.</param>
    /// <returns>The encoded name.</returns>
    public static string GetTenantSpecificContainerNameFor(ITenant tenant, string containerName)
        => GetTenantSpecificContainerNameFor(tenant.Id, containerName);

    /// <summary>
    /// Make a container name that is unique for this combination of tenant and logical container
    /// name.
    /// </summary>
    /// <param name="tenantId">The id of the tenant for which to generate a name.</param>
    /// <param name="containerName">The plain text name for the Cosmos container.</param>
    /// <returns>The encoded name.</returns>
    public static string GetTenantSpecificContainerNameFor(string tenantId, string containerName)
        => $"{tenantId.ToLowerInvariant()}-{containerName}";
}