// <copyright file="AzureStorageBlobTenantedContainerNaming.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage.Tenancy
{
    using Corvus.Tenancy;

    /// <summary>
    /// Converts plain text names for Azure Blob Storage container into tenant-specific names and,
    /// if required, into hashed names that meet Azure Blob Storage's naming requirements for
    /// tenants while still being unique.
    /// </summary>
    /// <remarks>
    /// There are various restrictions on blob container names in Azure storage. For example, a
    /// name can start with a letter or a number and can be a maximum of 63 characters long, and so
    /// on. As a result, it's desirable to have a mechanism for taking an "ideal world" container
    /// name and converting it into a name that's guaranteed to be safe to use. This class meets
    /// those requirements.
    /// </remarks>
    public static class AzureStorageBlobTenantedContainerNaming
    {
        /// <summary>
        /// Make a container name safe to use as an Azure storage blob container name, and which
        /// is unique for this combination of tenant and logical container name.
        /// </summary>
        /// <param name="tenant">The tenant for which to generate a name.</param>
        /// <param name="containerName">The plain text name for the blob container.</param>
        /// <returns>The encoded name.</returns>
        public static string GetHashedTenantedBlobContainerNameFor(ITenant tenant, string containerName)
        {
            string tenantedUnhashedContainerName = GetTenantedLogicalBlobContainerNameFor(tenant, containerName);
            return AzureStorageBlobContainerNaming.HashAndEncodeBlobContainerName(tenantedUnhashedContainerName);
        }

        /// <summary>
        /// Create a tenant-specific logical name for an Azure storage blob container name.
        /// </summary>
        /// <param name="tenant">The tenant for which to generate a name.</param>
        /// <param name="containerName">The plain text name for the blob container.</param>
        /// <returns>The encoded name.</returns>
        public static string GetTenantedLogicalBlobContainerNameFor(ITenant tenant, string containerName)
            => $"{tenant.Id.ToLowerInvariant()}-{containerName}";
    }
}