// <copyright file="SqlConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Sql.Tenancy
{
    /// <summary>
    /// Encapsulates configuration for a database in a specific SQL Server instance.
    /// </summary>
    public class SqlConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlConfiguration"/> class.
        /// </summary>
        public SqlConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the name of the key vault in which the connection string for the server is stored.
        /// </summary>
        public string KeyVaultName { get; set; }

        /// <summary>
        /// Gets or sets the secret name for the connection string that connects to the server.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This should be a connection to a given server, and will <em>not</em> include the <c>InitialCatalog=blah</c> value, as this will be set by the <see cref="SqlConnectionDefinition.Database"/> property.
        /// </para>
        /// <para>If this property is set, then the KeyVaultName should also be set.</para>
        /// </remarks>
        public string ConnectionStringSecretName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable the tenant ID prefix.
        /// </summary>
        public bool DisableTenantIdPrefix { get; set; }

        /// <summary>
        /// Gets or sets the base connection string for the server.
        /// </summary>
        /// <remarks>
        /// This is used in test scenarios to set an explicit base connection string. In production, you would typically use the keyvault connection string.
        /// </remarks>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a local database.
        /// </summary>
        public bool IsLocalDatabase { get; set; }

        /// <summary>
        /// Gets or sets the database name. If set, this overrides the value
        /// specified in <see cref="SqlConnectionDefinition.Database"/>.
        /// </summary>
        /// <remarks>
        /// This is used to append the <c>InitialCatalog</c> or <c>Database</c> property of the server connection string supplied.
        /// </remarks>
        public string Database { get; set; }

        /// <summary>
        /// Validates the connection string.
        /// </summary>
        /// <returns>True if the connection string is valid.</returns>
        /// <remarks>
        /// The configuration must either have no <see cref="ConnectionString"/>, <see cref="KeyVaultName"/> or <see cref="ConnectionStringSecretName"/> set,
        /// OR a <see cref="ConnectionString"/> but no <see cref="KeyVaultName"/> or <see cref="ConnectionStringSecretName"/>, OR
        /// a <see cref="KeyVaultName"/> and a <see cref="ConnectionStringSecretName"/> but no <see cref="ConnectionString"/>.
        /// </remarks>
        public bool Validate()
        {
            return
                this.IsEmpty() ||
                (!this.HasIncompleteKeyVaultDetailsAndNoExplicitConnectionString() &&
                !this.HasExplicitConnectionStringAndAtLeastPartialKeyVaultName());
        }

        /// <summary>
        /// Determines if the configuration is empty.
        /// </summary>
        /// <returns>True if the configuration has no <see cref="ConnectionString"/>, <see cref="KeyVaultName"/> or <see cref="ConnectionStringSecretName"/> set.</returns>
        internal bool IsEmpty()
        {
            return string.IsNullOrEmpty(this.ConnectionString) &&
                string.IsNullOrEmpty(this.KeyVaultName) &&
                string.IsNullOrEmpty(this.ConnectionStringSecretName);
        }

        private bool HasExplicitConnectionStringAndAtLeastPartialKeyVaultName()
        {
            return !string.IsNullOrEmpty(this.ConnectionString) &&
                    (!string.IsNullOrEmpty(this.KeyVaultName) ||
                     !string.IsNullOrEmpty(this.ConnectionStringSecretName));
        }

        private bool HasIncompleteKeyVaultDetailsAndNoExplicitConnectionString()
        {
            return string.IsNullOrEmpty(this.ConnectionString) &&
                    (string.IsNullOrEmpty(this.KeyVaultName) ||
                     string.IsNullOrEmpty(this.ConnectionStringSecretName));
        }
    }
}