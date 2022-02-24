// <copyright file="TableStorageTenancyExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.TableStorage.Tenancy;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Corvus.Tenancy;

using global::Azure.Data.Tables;

/// <summary>
/// Extension methods providing tenanted access to table storage.
/// </summary>
public static class TableStorageTenancyExtensions
{
    /// <summary>
    /// Creates repository configuration properties suitable for passing to
    /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
    /// </summary>
    /// <param name="values">Existing configuration values to which to append these.</param>
    /// <param name="key">The key to use for the property.</param>
    /// <param name="configuration">The configuration to set.</param>
    /// <returns>
    /// Properties to pass to
    /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
    /// </returns>
    public static IEnumerable<KeyValuePair<string, object>> AddTableStorageConfiguration(
        this IEnumerable<KeyValuePair<string, object>> values,
        string key,
        TableConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(configuration);

        return values.Append(new KeyValuePair<string, object>(key, configuration));
    }

    /// <summary>
    /// Get the configuration for the specified blob container definition for a particular tenant.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="key">The key of the tenancy property containing the settings.</param>
    /// <returns>The configuration for the storage account for this tenant.</returns>
    public static TableConfiguration GetTableStorageConfiguration(
        this ITenant tenant,
        string key)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(key);

        if (tenant.Properties.TryGet(key, out TableConfiguration? configuration))
        {
            return configuration;
        }

        throw new InvalidOperationException($"Tenant {tenant.Id} does not contain a property '{key}'");
    }

    /// <summary>
    /// Gets a <see cref="TableClient"/> using <see cref="TableConfiguration"/>
    /// stored in a tenant.
    /// </summary>
    /// <param name="tableSource">
    /// The <see cref="ITableSourceFromDynamicConfiguration"/> that provides the underlying
    /// ability to supply a <see cref="TableClient"/> for a
    /// <see cref="TableConfiguration"/>.
    /// </param>
    /// <param name="tenant">
    /// The tenant containing the <see cref="TableConfiguration"/>.
    /// </param>
    /// <param name="configurationKey">
    /// The key identifying the <see cref="ITenant.Properties"/> entry containing the
    /// <see cref="TableConfiguration"/> to use.
    /// </param>
    /// <param name="tableName">
    /// An optional table name to use. If this is null, the table name specified in the
    /// <see cref="TableConfiguration"/> will be used. In cases where multiple
    /// containers are in use, it's common to have one <see cref="TableConfiguration"/>
    /// with a null <see cref="TableConfiguration.TableName"/>, and to specify the
    /// container name required when asking for a <see cref="TableClient"/>.
    /// </param>
    /// <returns>
    /// A value task that produces a <see cref="TableClient"/>.
    /// </returns>
    public static async ValueTask<TableClient> GetTableClientFromTenantAsync(
        this ITableSourceFromDynamicConfiguration tableSource,
        ITenant tenant,
        string configurationKey,
        string? tableName = null)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(configurationKey);

        TableConfiguration configuration = tenant.GetTableStorageConfiguration(configurationKey);

        if (tableName is not null)
        {
            configuration = configuration with
            {
                TableName = tableName,
            };
        }

        return await tableSource.GetStorageContextAsync(configuration).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a <see cref="TableClient"/> using <see cref="TableConfiguration"/>
    /// stored in a tenant.
    /// </summary>
    /// <param name="tableSource">
    /// The <see cref="ITableSourceFromDynamicConfiguration"/> that provides the underlying
    /// ability to supply a <see cref="TableClient"/> for a
    /// <see cref="TableConfiguration"/>.
    /// </param>
    /// <param name="tenant">
    /// The tenant containing the <see cref="TableConfiguration"/>.
    /// </param>
    /// <param name="configurationKey">
    /// The key identifying the <see cref="ITenant.Properties"/> entry containing the
    /// <see cref="TableConfiguration"/> to use.
    /// </param>
    /// <param name="tableName">
    /// An optional container name to use. If this is null, the container name specified in the
    /// <see cref="TableConfiguration"/> will be used. In cases where multiple
    /// containers are in use, it's common to have one <see cref="TableConfiguration"/>
    /// with a null <see cref="TableConfiguration.TableName"/>, and to specify the
    /// container name required when asking for a <see cref="TableClient"/>.
    /// </param>
    /// <returns>
    /// A value task that produces a <see cref="TableClient"/>.
    /// </returns>
    public static async ValueTask<TableClient> GetReplacementForFailedTableClientFromTenantAsync(
        this ITableSourceFromDynamicConfiguration tableSource,
        ITenant tenant,
        string configurationKey,
        string? tableName = null)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(configurationKey);

        TableConfiguration configuration = tenant.GetTableStorageConfiguration(configurationKey);

        if (tableName is not null)
        {
            configuration = configuration with
            {
                TableName = tableName,
            };
        }

        return await tableSource.GetReplacementForFailedStorageContextAsync(configuration).ConfigureAwait(false);
    }
}