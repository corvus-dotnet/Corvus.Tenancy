// <copyright file="TableSourceWithTenantLegacyTransition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.TableStorage.Tenancy.Internal;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Corvus.Storage.Azure.TableStorage;
using Corvus.Tenancy;

using global::Azure.Data.Tables;

/// <summary>
/// Implementation of <see cref="ITableSourceWithTenantLegacyTransition"/>.
/// </summary>
internal class TableSourceWithTenantLegacyTransition : ITableSourceWithTenantLegacyTransition
{
    private readonly ITableSourceFromDynamicConfiguration tableSource;

    /// <summary>
    /// Creates a <see cref="TableSourceWithTenantLegacyTransition"/>.
    /// </summary>
    /// <param name="tableSource">
    /// The underling non-legacy source.
    /// </param>
    public TableSourceWithTenantLegacyTransition(
        ITableSourceFromDynamicConfiguration tableSource)
    {
        this.tableSource = tableSource;
    }

    /// <inheritdoc/>
    public async ValueTask<TableClient> GetTableClientFromTenantAsync(
        ITenant tenant,
        string v2ConfigurationKey,
        string v3ConfigurationKey,
        string? tableName = null,
        TableClientOptions? tableClientOptions = null,
        CancellationToken cancellationToken = default)
    {
        bool v3ConfigWasAvailable = false;
        if (tenant.Properties.TryGet(v3ConfigurationKey, out TableConfiguration v3Configuration))
        {
            v3ConfigWasAvailable = true;
            v3Configuration = AddTableNameIfNotInConfig(v3Configuration, tenant, tableName);
        }
        else if (tenant.Properties.TryGet(v2ConfigurationKey, out LegacyV2TableConfiguration legacyConfiguration))
        {
            v3Configuration = V3ConfigurationFromLegacy(tenant, tableName, legacyConfiguration);
        }
        else
        {
            throw new InvalidOperationException("Tenant did not contain blob storage configuration under specified v2 or v3 keys");
        }

        if (v3Configuration.TableName == null)
        {
            throw new InvalidOperationException($"When the configuration does not specify a TableName, you must supply a non-null {nameof(tableName)}");
        }

        TableClient result = await this.tableSource.GetStorageContextAsync(
            v3Configuration,
            tableClientOptions,
            cancellationToken)
            .ConfigureAwait(false);

        // If the settings say to create a new V3 config if there wasn't already one, it's important
        // that we don't do this until after successfully creating the container, because in the world
        // of V3, apps are supposed to create containers before they create the relevant configuration.
        // (There's a wrinkle here: if an application is using multiple logical containers per
        // tenant, then what are they supposed to do? Should we offer a "create all the containers"
        // option?)
        if (!v3ConfigWasAvailable)
        {
                await result.CreateIfNotExistsAsync(
                    cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc/>
    public async ValueTask<TableConfiguration?> MigrateToV3Async(
        ITenant tenant,
        string v2ConfigurationKey,
        string v3ConfigurationKey,
        IEnumerable<string>? tableNames,
        TableClientOptions? tableClientOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (tenant.Properties.TryGet(v3ConfigurationKey, out TableConfiguration _))
        {
            return null;
        }

        if (!tenant.Properties.TryGet(v2ConfigurationKey, out LegacyV2TableConfiguration legacyConfiguration))
        {
            throw new InvalidOperationException("Tenant did not contain blob storage configuration under specified v2 or v3 keys");
        }

        if (tableNames == null)
        {
            if (legacyConfiguration.TableName is string tableNameFromConfig)
            {
                tableNames = new[] { tableNameFromConfig };
            }
            else
            {
                throw new InvalidOperationException($"When the configuration does not specify a TableName, you must supply a non-null {nameof(tableNames)}");
            }
        }

        string? logicalTableName = null;
        int tableCount = 0;
        foreach (string rawTableName in tableNames)
        {
            tableCount += 1;
            logicalTableName = rawTableName;

            TableConfiguration thisConfig = V3ConfigurationFromLegacy(tenant, rawTableName, legacyConfiguration);

            TableClient result = await this.tableSource.GetStorageContextAsync(
                thisConfig,
                tableClientOptions,
                cancellationToken)
                .ConfigureAwait(false);
            await result.CreateIfNotExistsAsync(
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        // In cases where the legacy configuration had no TableName property, and we were
        // passed a tableNames containing exactly one name, we can set the TableName
        // in the V3 config. But if there were multiple logical table names, we don't
        // want to set the TableName in the V3 config because the application is likely
        // plugging in specific table names at runtime.
        if (tableCount > 1)
        {
            logicalTableName = null;
        }

        return V3ConfigurationFromLegacy(tenant, logicalTableName, legacyConfiguration);
    }

    private static TableConfiguration V3ConfigurationFromLegacy(
        ITenant tenant,
        string? tableName,
        LegacyV2TableConfiguration legacyConfiguration)
    {
        TableConfiguration v3Configuration = LegacyTableConfigurationConverter.FromV2ToV3(legacyConfiguration);
        if (legacyConfiguration.TableName is not null)
        {
            v3Configuration = v3Configuration with
            {
                TableName = string.IsNullOrWhiteSpace(legacyConfiguration.TableName)
                    ? tableName is null ? null : AzureTableNaming.HashAndEncodeTableName(tableName)
                        : AzureTableNaming.HashAndEncodeTableName(
                            legacyConfiguration.DisableTenantIdPrefix
                            ? legacyConfiguration.TableName
                            : AzureTablesTenantedNaming.GetTenantedLogicalTableNameFor(tenant, legacyConfiguration.TableName)),
            };
        }

        return AddTableNameIfNotInConfig(
            v3Configuration,
            tenant,
            tableName);
    }

    private static TableConfiguration AddTableNameIfNotInConfig(
        TableConfiguration configuration,
        ITenant tenant,
        string? tableName)
    {
        return configuration.TableName is null && tableName is not null
            ? configuration with
            {
                TableName = AzureTablesTenantedNaming.GetHashedTenantedTableNameFor(
                    tenant,
                    tableName ?? throw new InvalidOperationException("When the configuration does not specify a TableName, you must supply a non-null logical table name")),
            }
            : configuration;
    }
}