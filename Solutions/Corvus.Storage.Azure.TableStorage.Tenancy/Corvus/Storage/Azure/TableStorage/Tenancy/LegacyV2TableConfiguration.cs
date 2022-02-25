// <copyright file="LegacyV2TableConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.TableStorage.Tenancy;

/// <summary>
/// Enables this project to read configuration in the legacy v2 <c>TableStorageConfiguration</c>
/// format without imposing a dependency on the old v2 components.
/// </summary>
public class LegacyV2TableConfiguration
{
    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    /// <remarks>If the account key secret name is empty, then this should contain a complete connection string.</remarks>
    public string? AccountName { get; set; }

    /// <summary>
    /// Gets or sets the container name. If set, this overrides any name specified when calling
    /// <see cref="ITableSourceWithTenantLegacyTransition.GetTableClientFromTenantAsync(Corvus.Tenancy.ITenant, string, string, string?, global::Azure.Data.Tables.TableClientOptions?, CancellationToken)"/>.
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