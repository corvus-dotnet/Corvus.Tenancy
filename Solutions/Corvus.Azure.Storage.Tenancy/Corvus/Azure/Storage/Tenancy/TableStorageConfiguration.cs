// <copyright file="TableStorageConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    /// <summary>
    /// Encapsulates configuration for a storage account.
    /// </summary>
    public class TableStorageConfiguration
    {
        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        /// <remarks>If the account key secret name is empty, then this should contain a complete connection string.</remarks>
        public string? AccountName { get; set; }

        /// <summary>
        /// Gets or sets the container name. If set, this overrides the name specified in
        /// <see cref="TableStorageTableDefinition.TableName"/>.
        /// </summary>
        public string? TableName { get; set; }

        /// <summary>
        /// Gets or sets the key value name.
        /// </summary>
        public string? KeyVaultName { get; set; }

        /// <summary>
        /// Gets or sets the account key secret mame.
        /// </summary>
        public string? AccountKeySecretName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable the tenant ID prefix.
        /// </summary>
        public bool DisableTenantIdPrefix { get; set; }
    }
}
