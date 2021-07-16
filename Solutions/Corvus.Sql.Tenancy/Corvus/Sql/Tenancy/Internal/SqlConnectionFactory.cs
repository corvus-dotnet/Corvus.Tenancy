// <copyright file="SqlConnectionFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Sql.Tenancy.Internal
{
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Text;
    using System.Threading.Tasks;

    using Corvus.Tenancy;
    using Corvus.Tenancy.Azure.Common;

    /// <summary>
    /// A factory for a <see cref="SqlConnection"/>.
    /// </summary>
    internal class SqlConnectionFactory :
        CachingStorageContextFactory<string, SqlConfiguration>
    {
        private const string DevelopmentStorageConnectionString = "Server=(localdb)\\mssqllocaldb;Database=testtenant;Trusted_Connection=True;MultipleActiveResultSets=true";
        private readonly TenantSqlConnectionFactoryOptions? options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlConnectionFactory"/> class.
        /// </summary>
        /// <param name="options">Configuration for the TenantBlobContainerClientFactory.</param>
        public SqlConnectionFactory(TenantSqlConnectionFactoryOptions? options = null)
        {
            this.options = options;
        }

        /// <inheritdoc/>
        protected override async Task<string> CreateContainerAsync(
            string contextName,
            SqlConfiguration configuration)
        {
            if (configuration.IsEmpty())
            {
                return DevelopmentStorageConnectionString;
            }
            else
            {
                if (!configuration.Validate())
                {
                    ThrowConfigurationException(configuration);
                }

                string connectionString;
                if (string.IsNullOrEmpty(configuration.ConnectionString))
                {
                    connectionString = await this.GetKeyVaultSecretAsync(
                        this.options?.AzureServicesAuthConnectionString,
                        configuration.KeyVaultName!,
                        configuration.ConnectionStringSecretName!).ConfigureAwait(false);
                }
                else
                {
                    if (!string.IsNullOrEmpty(configuration.KeyVaultName) || !string.IsNullOrEmpty(configuration.ConnectionStringSecretName))
                    {
                        ThrowConfigurationException(configuration);
                    }

                    // Null forgiving operator only necessary for as long as we target .NET Standard 2.0.
                    connectionString = configuration.ConnectionString!;
                }

                var connectionStringBuilder = new StringBuilder(connectionString);
                if (!string.IsNullOrEmpty(configuration.Database))
                {
                    if (configuration.IsLocalDatabase)
                    {
                        // Append the database name as initial catalog if available
                        DbConnectionStringBuilder.AppendKeyValuePair(connectionStringBuilder, "Database", configuration.Database);
                    }
                    else
                    {
                        // Append the database name as initial catalog if available
                        DbConnectionStringBuilder.AppendKeyValuePair(connectionStringBuilder, "Initial Catalog", configuration.Database);
                    }
                }

                return connectionStringBuilder.ToString();
            }
        }

        private static void ThrowConfigurationException(SqlConfiguration storageConfiguration)
        {
            throw new InvalidOperationException("You must provide either a ConnectionString or both a KeyVaultName and a ConnectionStringSecretName to configure your SqlConnection. " +
                $"You have provided ConnectionString=\"{storageConfiguration.ConnectionString}\", KeyVaultName=\"{storageConfiguration.KeyVaultName}\", ConnectionStringSecretName=\"{storageConfiguration.ConnectionStringSecretName}\"");
        }
   }
}