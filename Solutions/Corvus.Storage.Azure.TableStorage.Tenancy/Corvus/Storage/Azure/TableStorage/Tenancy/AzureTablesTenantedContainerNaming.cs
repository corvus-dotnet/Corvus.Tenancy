// <copyright file="AzureTablesTenantedContainerNaming.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.TableStorage.Tenancy;

using Corvus.Tenancy;

/// <summary>
/// Converts plain text names for Azure Tables container into tenant-specific names and, if
/// required, into hashed names that meet Azure Table Storage's naming requirements for tenants
/// while still being unique.
/// </summary>
/// <remarks>
/// <para>
/// There are various restrictions on table names in Azure storage. For example, a name can start
/// with a number, followed by a mix of letters and numbers, and can be a maximum of 63 characters
/// long, and so on. As a result, it's desirable to have a mechanism for taking an "ideal world"
/// container name and converting it into a name that's guaranteed to be safe to use. This class
/// meets those requirements.
/// </para>
/// <para>
/// Note that although Cosmos DB also offers a table API that is nominally compatible with Azure
/// Storage tables, it has fewer restrictions on names. We do not currently offer a method that
/// takes advantage of that, because all table names valid for Azure Storage are also for tables
/// in Cosmos DB.
/// </para>
/// </remarks>
public static class AzureTablesTenantedContainerNaming
{
    /// <summary>
    /// Make a container name safe to use as an Azure Storage table name, and which is unique for
    /// this combination of tenant and logical container name.
    /// </summary>
    /// <param name="tenant">The tenant for which to generate a name.</param>
    /// <param name="tableName">The plain text name for the table.</param>
    /// <returns>The encoded name.</returns>
    public static string GetHashedTenantedTableNameFor(ITenant tenant, string tableName)
    {
        string tenantedUnhashedContainerName = GetTenantedLogicalTableNameFor(tenant, tableName);
        return AzureTableNaming.HashAndEncodeTableName(tenantedUnhashedContainerName);
    }

    /// <summary>
    /// Create a tenant-specific logical name for an Azure storage blob container name.
    /// </summary>
    /// <param name="tenant">The tenant for which to generate a name.</param>
    /// <param name="tableName">The plain text name for the table.</param>
    /// <returns>The encoded name.</returns>
    public static string GetTenantedLogicalTableNameFor(ITenant tenant, string tableName)
        => $"{tenant.Id.ToLowerInvariant()}-{tableName}";
}