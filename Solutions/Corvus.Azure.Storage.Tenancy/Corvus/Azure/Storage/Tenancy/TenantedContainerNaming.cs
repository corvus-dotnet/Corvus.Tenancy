// <copyright file="TenantedContainerNaming.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.Tenancy;

    /// <summary>
    /// Methods for building container names for use in tenanted storage.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For compatibility, we need to make it straightforward to use the same container names as we
    /// would have done in V2 (and services already using this such as Marain.Tenancy must be able
    /// to work with stored data created with earlier versions.
    /// </para>
    /// <para>
    /// In the V2 world, we had definition types that had a mixture of logical names and some
    /// physical characteristics to use when auto-creating containers. And if the storage
    /// configuration located for that definition did not specify a container name, it would
    /// generate one automatically. In V3, we have moved away from definition types to plain
    /// logical names (which we now call "storage context names"). None of the providers create
    /// containers for you, and they require the configuration object to specify the exact
    /// container name to use - the providers no longer make up a tenanted container name for
    /// you. But the methods in this class provide ways to get the same tenanted logical names,
    /// and the corresponding physical container names, as would have been used automatically
    /// before.
    /// </para>
    /// </remarks>
    public static class TenantedContainerNaming
    {
        /// <summary>
        /// Create a name that is unique for the specified scope and context, and which meets the
        /// requirements for an Azure Storage blob container name.
        /// </summary>
        /// <param name="scope">
        /// The identifier for the scope (e.g. a lowercased <see cref="ITenant.Id"/>).
        /// </param>
        /// <param name="contextName">
        /// The name of the storage context.
        /// </param>
        /// <returns>
        /// A string that will always be the same for any particular pair of scope and contextName,
        /// and which is different from any string returned for any other combination of scope and
        /// contextName, and which conforms to the requirements for an Azure Storage blob container
        /// name.
        /// </returns>
        public static string MakeUniqueSafeBlobContainerName(
            string scope,
            string contextName)
        {
            return AzureStorageNameHelper.HashAndEncodeBlobContainerName($"{scope}-{contextName}");
        }

        /// <summary>
        /// Create a name that is unique for the specified tenant and context, and which meets the
        /// requirements for an Azure Storage blob container name.
        /// </summary>
        /// <param name="tenant">
        /// The tenant.
        /// </param>
        /// <param name="contextName">
        /// The name of the storage context.
        /// </param>
        /// <returns>
        /// A string that will always be the same for any particular pair of scope and contextName,
        /// and which is different from any string returned for any other combination of scope and
        /// contextName, and which conforms to the requirements for an Azure Storage blob container
        /// name.
        /// </returns>
        public static string MakeUniqueSafeBlobContainerName(
            ITenant tenant,
            string contextName)
        {
            return MakeUniqueSafeBlobContainerNameFromTenantId(tenant.Id, contextName);
        }

        /// <summary>
        /// Create a name that is unique for the specified tenant and context, and which meets the
        /// requirements for an Azure Storage blob container name.
        /// </summary>
        /// <param name="tenantId">
        /// The id of the tenant.
        /// </param>
        /// <param name="contextName">
        /// The name of the storage context.
        /// </param>
        /// <returns>
        /// A string that will always be the same for any particular pair of scope and contextName,
        /// and which is different from any string returned for any other combination of scope and
        /// contextName, and which conforms to the requirements for an Azure Storage blob container
        /// name.
        /// </returns>
        public static string MakeUniqueSafeBlobContainerNameFromTenantId(
            string tenantId,
            string contextName)
        {
            return MakeUniqueSafeBlobContainerName(tenantId.ToLowerInvariant(), contextName);
        }

        /// <summary>
        /// Create a name that is unique for the specified scope and context, and which meets the
        /// requirements for an Azure Storage table container name.
        /// </summary>
        /// <param name="scope">
        /// The identifier for the scope (e.g. a lowercased <see cref="ITenant.Id"/>).
        /// </param>
        /// <param name="contextName">
        /// The name of the storage context.
        /// </param>
        /// <returns>
        /// A string that will always be the same for any particular pair of scope and contextName,
        /// and which is different from any string returned for any other combination of scope and
        /// contextName, and which conforms to the requirements for an Azure Storage table container
        /// name.
        /// </returns>
        public static string MakeUniqueSafeTableContainerName(
            string scope,
            string contextName)
        {
            return AzureStorageNameHelper.HashAndEncodeTableName($"{scope}-{contextName}");
        }

        /// <summary>
        /// Create a name that is unique for the specified tenant and context, and which meets the
        /// requirements for an Azure Storage table container name.
        /// </summary>
        /// <param name="tenant">
        /// The tenant.
        /// </param>
        /// <param name="contextName">
        /// The name of the storage context.
        /// </param>
        /// <returns>
        /// A string that will always be the same for any particular pair of scope and contextName,
        /// and which is different from any string returned for any other combination of scope and
        /// contextName, and which conforms to the requirements for an Azure Storage table container
        /// name.
        /// </returns>
        public static string MakeUniqueSafeTableContainerName(
            ITenant tenant,
            string contextName)
        {
            return MakeUniqueSafeTableContainerName(tenant.Id.ToLowerInvariant(), contextName);
        }

        /// <summary>
        /// Builds a new <see cref="BlobStorageConfiguration"/> from an existing one, but with a
        /// tenant-specific Azure Blob Storage container name.
        /// </summary>
        /// <param name="configuration">
        /// The configuration on which to base the new configuration. The existing
        /// <see cref="BlobStorageConfiguration.Container"/> will be ignored, because the purpose
        /// of this method is to generate a new container name.
        /// </param>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="storageContextName">
        /// The name of the storage context.
        /// </param>
        /// <returns>
        /// A new configuration that preserves account and authentication detail, but with a new
        /// tenant-specific name based on the storage context name.
        /// </returns>
        public static BlobStorageConfiguration MakeTenantedConfiguration(
            BlobStorageConfiguration configuration,
            string tenantId,
            string storageContextName)
        {
            return new BlobStorageConfiguration
            {
                AccountKeySecretName = configuration.AccountKeySecretName,
                AccountName = configuration.AccountName,
                KeyVaultName = configuration.KeyVaultName,
                Container = TenantedContainerNaming.MakeUniqueSafeBlobContainerNameFromTenantId(tenantId, storageContextName),
            };
        }
    }
}