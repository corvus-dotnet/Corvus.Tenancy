// <copyright file="CosmosContainerSourceFromDynamicConfigurationExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.Cosmos.Tenancy;

using System;
using System.Threading.Tasks;

using Corvus.Tenancy;

using Microsoft.Azure.Cosmos;

/// <summary>
/// Defines extension methods to <see cref="ICosmosContainerSourceFromDynamicConfiguration"/>
/// enabling access to tenanted storage.
/// </summary>
public static class CosmosContainerSourceFromDynamicConfigurationExtensions
{
    /// <summary>
    /// Gets a Cosmos DB container using configuration settings in a tenant.
    /// </summary>
    /// <param name="source">
    /// Provides the ability to obtain a <see cref="Container"/> for a
    /// <see cref="CosmosContainerConfiguration"/>.
    /// </param>
    /// <param name="tenant">
    /// The tenant in which the configuration settings are stored.
    /// </param>
    /// <param name="configurationKey">
    /// The key under which the configuration settings are stored in the tenant.
    /// </param>
    /// <param name="containerName">
    /// The name of the Cosmos container required. (If the configuration specifies a container,
    /// you can leave this as null. If it does not specify a container, then this must be set.)
    /// </param>
    /// <param name="cosmosClientOptions">
    /// Optional options to pass when creating the <see cref="CosmosClient"/>.
    /// </param>
    /// <returns>
    /// A task that produces a <see cref="Container"/> providing access to the container to use
    /// for this tenant.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the tenant does not contain a configuration entry for <paramref name="configurationKey"/>.
    /// </exception>
    public static async ValueTask<Container> GetContainerForTenantAsync(
        this ICosmosContainerSourceFromDynamicConfiguration source,
        ITenant tenant,
        string configurationKey,
        string? containerName = null,
        CosmosClientOptions? cosmosClientOptions = null)
    {
        CosmosContainerConfiguration configuration = tenant.GetCosmosConfiguration(configurationKey);

        if (configuration.Container == null && containerName == null)
        {
            throw new ArgumentNullException(
                nameof(containerName),
                $"If the configuration in the tenant does not specify a Container, the {nameof(containerName)} argument must not be null");
        }

        if (!string.IsNullOrEmpty(containerName))
        {
            configuration = configuration with
            {
                Container = containerName,
            };
        }

        return await source.GetStorageContextAsync(configuration, cosmosClientOptions).ConfigureAwait(false);
    }
}