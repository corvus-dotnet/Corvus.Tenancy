// <copyright file="AzureStorageNameHelper.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy.Internal
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Corvus.Tenancy;

    /// <summary>
    /// Helpers to convert plain text names for Azure storage items into guaranteed valid hashes.
    /// </summary>
    /// <remarks>
    /// There are various restrictions on entity names in Azure storage. For example, a blob container name can start
    /// with a letter or a number, but a table name has to start with a letter. Both blob container and table names
    /// can be a maximum of 63 characters long, and so on. As a result, it's desirable to have a mechanism for taking
    /// an "ideal world" table name and converting it into a name that's guaranteed to be safe to use. This class
    /// provides helper methods to do that.
    /// </remarks>
    public static class AzureStorageNameHelper
    {
        private static readonly Lazy<SHA1> HashProvider = new (() => SHA1.Create());

        /// <summary>
        /// Makes a plain text name safe to use as an Azure storage table name.
        /// </summary>
        /// <param name="tableName">The plain text name for the table.</param>
        /// <returns>The encoded name.</returns>
        public static string HashAndEncodeTableName(string tableName)
        {
            byte[] byteContents = Encoding.UTF8.GetBytes(tableName);
            byte[] hashedBytes = HashProvider.Value.ComputeHash(byteContents);
            string hexString = TenantExtensions.ByteArrayToHexViaLookup32(hashedBytes);

            // Table names can't start with a number, so prefix all names with a letter
            return "t" + hexString;
        }

        /// <summary>
        /// Make a plain text name safe to use as an Azure storage blob container name.
        /// </summary>
        /// <param name="containerName">The plain text name for the blob container.</param>
        /// <returns>The encoded name.</returns>
        public static string HashAndEncodeBlobContainerName(string containerName)
        {
            byte[] byteContents = Encoding.UTF8.GetBytes(containerName);
            byte[] hashedBytes = HashProvider.Value.ComputeHash(byteContents);
            return TenantExtensions.ByteArrayToHexViaLookup32(hashedBytes);
        }
    }
}