// <copyright file="BlobStorageConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    /// <summary>
    /// Encapsulates configuration for a storage account.
    /// </summary>
    public class BlobStorageConfiguration
    {
        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        /// <remarks>If the account key secret name is empty, then this should contain a complete connection string.</remarks>
        public string? AccountName { get; set; }

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        /// <remarks>
        /// This must be the actual container name, so it must conform to the naming rules imposed
        /// by Azure, and it must unique within the storage account for this configuration, and for
        /// any other configurations referring to the same storage account. You can use
        /// <see cref="TenantedContainerNaming.MakeUniqueSafeBlobContainerName(string, string)"/>
        /// to create a suitable string.
        /// </remarks>
        public string? Container { get; set; }

        /// <summary>
        /// Gets or sets the key value name.
        /// </summary>
        public string? KeyVaultName { get; set; }

        /// <summary>
        /// Gets or sets the account key secret mame.
        /// </summary>
        public string? AccountKeySecretName { get; set; }
    }
}
