// <copyright file="TenantSqlConnectionFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Sql.Tenancy.Internal
{
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    using Corvus.Tenancy;

    /// <summary>
    /// A factory for tenanted SQL connection strings.
    /// </summary>
    internal class TenantSqlConnectionFactory :
        TenantedStorageContextFactory<string, SqlConfiguration>,
        ITenantSqlConnectionFactory
    {
        /// <summary>
        /// Creates a <see cref="TenantSqlConnectionFactory"/>.
        /// </summary>
        /// <param name="options">Configuration settings.</param>
        public TenantSqlConnectionFactory(TenantSqlConnectionFactoryOptions? options = null)
            : base(new SqlConnectionFactory(options))
        {
        }

        /// <inheritdoc/>
        public async Task<SqlConnection> GetSqlConnectionForTenantAsync(ITenant tenant, string storageContextName)
        {
            // This class is slightly different from most of the other tenanted storage mechanisms because
            // the items it returns, SqlConnection, are not sharable. So the underling caching factory caches
            // just connection strings, and we wrap them in a brand new SqlConnection right at the last minute.
            string connectionString = await this.GetContextForTenantAsync(tenant, storageContextName).ConfigureAwait(false);
            return new SqlConnection(connectionString);
        }
    }
}