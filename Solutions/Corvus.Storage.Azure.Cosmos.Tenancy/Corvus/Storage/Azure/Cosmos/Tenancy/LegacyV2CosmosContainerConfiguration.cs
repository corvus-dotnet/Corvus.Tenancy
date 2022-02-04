// <copyright file="LegacyV2CosmosContainerConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.Cosmos.Tenancy;

/// <summary>
/// Enables this project to read configuration in the legacy v2 <c>CosmosConfiguration</c>
/// format without imposing a dependency on the old v2 components.
/// </summary>
/// <remarks>
/// This will be converted to an equivalent <see cref="CosmosContainerConfiguration"/>.
/// </remarks>
public class LegacyV2CosmosContainerConfiguration
{
    /// <summary>
    /// Gets or sets the account URI.
    /// </summary>
    public string? AccountUri { get; set; }

    /// <summary>
    /// Gets or sets the name of the key vault in which the account secret is stored.
    /// </summary>
    public string? KeyVaultName { get; set; }

    /// <summary>
    /// Gets or sets the account key secret mame.
    /// </summary>
    public string? AccountKeySecretName { get; set; }

    /// <summary>
    /// Gets or sets a property defined by the legacy config but which is apparently unused.
    /// </summary>
    public string? AccountKeyConfigurationKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to disable the tenant ID prefix.
    /// </summary>
    public bool DisableTenantIdPrefix { get; set; }

    /// <summary>
    /// Gets or sets the database name. If set, this overrides any name specified when calling
    /// <see cref="ICosmosContainerSourceWithTenantLegacyTransition.GetContainerForTenantAsync"/>.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the container name. If set, this overrides any name specified when calling
    /// <see cref="ICosmosContainerSourceWithTenantLegacyTransition.GetContainerForTenantAsync"/>.
    /// </summary>
    public string? ContainerName { get; set; }

    /// <summary>
    /// Gets or sets the partition key path. If set, this overrides any name specified when calling
    /// <see cref="ICosmosContainerSourceWithTenantLegacyTransition.GetContainerForTenantAsync"/>.
    /// </summary>
    public string? PartitionKeyPath { get; set; }
}