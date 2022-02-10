// <copyright file="CosmosStorageTenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.Cosmos.Tenancy;

using System;
using System.Collections.Generic;
using System.Linq;

using Corvus.Tenancy;

/// <summary>
/// Extension methods for managing cosmos storage configuration in tenants.
/// </summary>
public static class CosmosStorageTenantExtensions
{
    /// <summary>
    /// Get the configuration for the specified Cosmos container definition for a particular tenant.
    /// </summary>
    /// <param name="tenant">The tenant.</param>
    /// <param name="configurationKey">The key with which the configuration is stored.</param>
    /// <returns>The configuration for the Cosmos account for this tenant.</returns>
    public static CosmosContainerConfiguration GetCosmosConfiguration(this ITenant tenant, string configurationKey)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(configurationKey);

        if (tenant.Properties.TryGet(configurationKey, out CosmosContainerConfiguration? configuration))
        {
            return configuration;
        }

        throw new InvalidOperationException($"Tenant {tenant.Id} does not have a '{configurationKey}' property");
    }

    /// <summary>
    /// Creates Cosmos configuration for the specified container suitable for passing to
    /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
    /// </summary>
    /// <param name="values">Existing configuration values to which to append these.</param>
    /// <param name="configurationKey">The key to use when storing the value in the tenant.</param>
    /// <param name="configuration">The configuration to set.</param>
    /// <returns>
    /// Properties to pass to
    /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
    /// </returns>
    public static IEnumerable<KeyValuePair<string, object>> AddCosmosConfiguration(
        this IEnumerable<KeyValuePair<string, object>> values,
        string configurationKey,
        CosmosContainerConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(configurationKey);
        ArgumentNullException.ThrowIfNull(configuration);

        return values.Append(new KeyValuePair<string, object>(configurationKey, configuration));
    }
}