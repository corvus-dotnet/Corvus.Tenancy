// <copyright file="ICosmosContainerSourceWithTenantLegacyTransition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.Cosmos.Tenancy;
using System.Collections.Generic;
using System.Threading.Tasks;

using Corvus.Tenancy;

using Microsoft.Azure.Cosmos;

/// <summary>
/// Provides <see cref="Container"/> instances based on configuration stored in tenant properties,
/// with support for migration from V2 to V3 of <c>Corvus.Tenancy</c>.
/// </summary>
public interface ICosmosContainerSourceWithTenantLegacyTransition
{
    /// <summary>
    /// Gets a <see cref="Container"/> based on configuration stored in a tenant.
    /// </summary>
    /// <param name="tenant">The tenant containing the configuration.</param>
    /// <param name="v2ConfigurationKey">
    /// The key identifying the <see cref="ITenant.Properties"/> entry containing legacy v2
    /// configuration, which will be used if there is no v3 configuration present.
    /// </param>
    /// <param name="v3ConfigurationKey">
    /// The key identifying the <see cref="ITenant.Properties"/> entry which, if present, will
    /// contain the<see cref="CosmosContainerConfiguration"/> to use.
    /// </param>
    /// <param name="databaseName">
    /// An optional database name to use. If this is null, the container name specified in the
    /// configuration will be used. In cases where multiple databases are in use, it's common
    /// to have one configuration entry with a null container name, and to specify the
    /// container name required when asking for a <see cref="Container"/>.
    /// </param>
    /// <param name="containerName">
    /// An optional container name to use. If this is null, the container name specified in the
    /// configuration will be used. In cases where multiple containers are in use, it's common
    /// to have one configuration entry with a null container name, and to specify the
    /// container name required when asking for a <see cref="Container"/>.
    /// </param>
    /// <param name="partitionKeyPath">
    /// An optional partition key to use. If this is null, the partition key specified in the
    /// configuration will be used.
    /// </param>
    /// <param name="databaseThroughput">
    /// The database throughput to configure if this method ends up creating the database.
    /// </param>
    /// <param name="containerThroughput">
    /// The container throughput to configure if this method ends up creating the container.
    /// </param>
    /// <param name="cosmosClientOptions">
    /// Optional Cosmos client parameters to be passed when calling the underlying
    /// <see cref="IStorageContextSourceFromDynamicConfiguration{TStorageContext, TConfiguration, TConnectionOptions}.GetStorageContextAsync(TConfiguration, TConnectionOptions, CancellationToken)"/>
    /// method to create the container client.
    /// </param>
    /// <param name="cancellationToken">
    /// May enable the operation to be cancelled.
    /// </param>
    /// <returns>
    /// A value task that produces a <see cref="Container"/>.
    /// </returns>
    ValueTask<Container> GetContainerForTenantAsync(
        ITenant tenant,
        string v2ConfigurationKey,
        string v3ConfigurationKey,
        string? databaseName = null,
        string? containerName = null,
        string? partitionKeyPath = null,
        int? databaseThroughput = null,
        int? containerThroughput = null,
        CosmosClientOptions? cosmosClientOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Makes the necessary preparations for upgrading from v2 to v3 configuration.
    /// </summary>
    /// <param name="tenant">The tenant containing the configuration.</param>
    /// <param name="v2ConfigurationKey">
    /// The key identifying the <see cref="ITenant.Properties"/> entry that will containing the
    /// legacy v2 configuration, when it is present. (An application may call this method for
    /// each of its tenants, in which case it's possible that some of them have only v3
    /// configuration. We do not treat this as an error.)
    /// </param>
    /// <param name="v3ConfigurationKey">
    /// The key identifying the <see cref="ITenant.Properties"/> entry that will containing the
    /// legacy v3 configuration, when it is present. If this is present, this method will make
    /// no changes and will return a null configuration.
    /// </param>
    /// <param name="databaseNames">
    /// In cases where the configuration does not specify the container name (typically because
    /// the application uses the same configuration to access several different databases)
    /// this lists all of the containers the application uses. This method will ensure that
    /// tenant-specific databases exist for each name before returning. In cases where the
    /// configuration does specify the database name, this should be null.
    /// </param>
    /// <param name="containers">
    /// In cases where the configuration does not specify the container name (typically because
    /// the application uses the same configuration to access several different containers)
    /// this lists all of the containers the application uses, along with the partition key for
    /// that container. This method will ensure that tenant-specific containers exist for each name
    /// before returning. In cases where the configuration does specify the container name, this
    /// should be null.
    /// </param>
    /// <param name="partitionKeyPath">
    /// In cases where the configuration does not specify the partition key path, but does specify
    /// a container name, this determines the partition key to be used. This should be null if
    /// <paramref name="containers"/> is not null, because in cases where a list of container
    /// names is supplied, that list also includes the per-container partition key path.
    /// </param>
    /// <param name="databaseThroughput">
    /// The database throughput to configure if this method ends up creating the database.
    /// </param>
    /// <param name="containerThroughput">
    /// The container throughput to configure if this method ends up creating the container.
    /// </param>
    /// <param name="cosmosClientOptions">
    /// Optional Cosmos client parameters to be passed when calling the underlying
    /// <see cref="IStorageContextSourceFromDynamicConfiguration{TStorageContext, TConfiguration, TConnectionOptions}.GetStorageContextAsync(TConfiguration, TConnectionOptions, CancellationToken)"/>
    /// method to create the container client.
    /// </param>
    /// <param name="cancellationToken">
    /// May enable the operation to be cancelled.
    /// </param>
    /// <returns>
    /// A value task that produces null if a configuration with the key specified in
    /// <paramref name="v3ConfigurationKey"/> was present in the tenant. Otherwise,
    /// returns a <see cref="Container"/> that contains v3 configuration
    /// equivalent to the existing v2 configuration. If no configuration with
    /// the key specified in <paramref name="v2ConfigurationKey"/> was present in the tenant,
    /// this produces null. (TODO: should it throw.)
    /// </returns>
    ValueTask<CosmosContainerConfiguration?> MigrateToV3Async(
        ITenant tenant,
        string v2ConfigurationKey,
        string v3ConfigurationKey,
        IEnumerable<string>? databaseNames,
        IEnumerable<(string ContainerName, string PartitionKeyPath)>? containers,
        string? partitionKeyPath,
        int? databaseThroughput = null,
        int? containerThroughput = null,
        CosmosClientOptions? cosmosClientOptions = null,
        CancellationToken cancellationToken = default);
}