// <copyright file="SqlConnectionSourceFromDynamicConfigurationExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Sql.Tenancy;

using System.Data.SqlClient;

using Corvus.Tenancy;

/// <summary>
/// Defines extension methods to <see cref="ISqlConnectionFromDynamicConfiguration"/>
/// enabling access to tenanted storage.
/// </summary>
public static class SqlConnectionSourceFromDynamicConfigurationExtensions
{
    /// <summary>
    /// Gets a SQL connection using configuration settings in a tenant.
    /// </summary>
    /// <param name="source">
    /// Provides the ability to obtain a <see cref="SqlConnection"/> for a
    /// <see cref="SqlDatabaseConfiguration"/>.
    /// </param>
    /// <param name="tenant">
    /// The tenant in which the configuration settings are stored.
    /// </param>
    /// <param name="configurationKey">
    /// The key under which the configuration settings are stored in the tenant.
    /// </param>
    /// <returns>
    /// A task that produces a <see cref="SqlConnection"/> providing access to the SQL database to
    /// use for this tenant.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the tenant does not contain a configuration entry for <paramref name="configurationKey"/>.
    /// </exception>
    public static async ValueTask<SqlConnection> GetSqlConnectionForTenantAsync(
        this ISqlConnectionFromDynamicConfiguration source,
        ITenant tenant,
        string configurationKey)
    {
        SqlDatabaseConfiguration configuration = tenant.GetSqlDatabaseConfiguration(configurationKey);
        return await source.GetStorageContextAsync(configuration, null).ConfigureAwait(false);
    }
}