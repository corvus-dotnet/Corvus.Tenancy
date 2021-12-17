// <copyright file="AzureBlobStorageNameHelper.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage.Tenancy
{
    using System;
    using System.Security.Cryptography;

    using Corvus.Tenancy;

    /// <summary>
    /// Helpers to convert plain text names for Azure Blob Storage container into guaranteed valid
    /// hashes.
    /// </summary>
    /// <remarks>
    /// There are various restrictions on blob container names in Azure storage. For example, a
    /// name can start with a letter or a number and can be a maximum of 63 characters long, and so
    /// on. As a result, it's desirable to have a mechanism for taking an "ideal world" container
    /// name and converting it into a name that's guaranteed to be safe to use. This class provides
    /// helper methods to do that.
    /// </remarks>
    [Obsolete("This just calls AzureStorageBlobContainerNaming.HashAndEncodeBlobContainerName in the Corvus.Storage library. Use AzureStorageBlobTenantedContainerNaming if you require tenanted name generation")]
    public static class AzureBlobStorageNameHelper
    {
        /// <summary>
        /// Make a plain text name safe to use as an Azure storage blob container name.
        /// </summary>
        /// <param name="containerName">The plain text name for the blob container.</param>
        /// <returns>The encoded name.</returns>
        public static string HashAndEncodeBlobContainerName(string containerName)
        {
            return AzureStorageBlobContainerNaming.HashAndEncodeBlobContainerName(containerName);
        }
    }
}