// <copyright file="ContainerNameBuilders.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.Tenancy;

    /// <summary>
    /// Methods for building container names for use in tenanted storage.
    /// </summary>
    public static class ContainerNameBuilders
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
            return MakeUniqueSafeBlobContainerName(tenant.Id.ToLowerInvariant(), contextName);
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
    }
}