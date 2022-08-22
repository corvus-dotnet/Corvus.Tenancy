// <copyright file="CosmosContainerSourceWithTenantLegacyTransition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.Cosmos.Tenancy.Internal;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Corvus.Tenancy;

using Microsoft.Azure.Cosmos;

/// <summary>
/// Implementation of <see cref="ICosmosContainerSourceWithTenantLegacyTransition"/>.
/// </summary>
internal class CosmosContainerSourceWithTenantLegacyTransition : ICosmosContainerSourceWithTenantLegacyTransition
{
    private readonly ICosmosContainerSourceFromDynamicConfiguration cosmosContainerSource;

    /// <summary>
    /// Creates a <see cref="CosmosContainerSourceWithTenantLegacyTransition"/>.
    /// </summary>
    /// <param name="cosmosContainerSource">
    /// The underling non-legacy source.
    /// </param>
    public CosmosContainerSourceWithTenantLegacyTransition(
        ICosmosContainerSourceFromDynamicConfiguration cosmosContainerSource)
    {
        this.cosmosContainerSource = cosmosContainerSource;
    }

    /// <inheritdoc/>
    public async ValueTask<Container> GetContainerForTenantAsync(
        ITenant tenant,
        string v2ConfigurationKey,
        string v3ConfigurationKey,
        string? databaseName = null,
        string? containerName = null,
        string? partitionKeyPath = null,
        int? databaseThroughput = null,
        int? containerThroughput = null,
        CosmosClientOptions? cosmosClientOptions = null,
        CancellationToken cancellationToken = default)
    {
        bool v3ConfigWasAvailable = false;
        if (tenant.Properties.TryGet(v3ConfigurationKey, out CosmosContainerConfiguration v3Configuration))
        {
            v3ConfigWasAvailable = true;

            if (v3Configuration.Database is null && databaseName is not null)
            {
                v3Configuration = v3Configuration with { Database = databaseName };
            }

            if (v3Configuration.Container is null && containerName is not null)
            {
                v3Configuration = v3Configuration with { Container = containerName };
            }
        }
        else if (tenant.Properties.TryGet(v2ConfigurationKey, out LegacyV2CosmosContainerConfiguration legacyConfiguration))
        {
            v3Configuration = LegacyCosmosConfigurationConverter.FromV2ToV3(legacyConfiguration);

            string? logicalDatabaseName = v3Configuration.Database ?? databaseName;
            string? logicalContainerName = v3Configuration.Container ?? containerName;
            if (!legacyConfiguration.DisableTenantIdPrefix)
            {
                if (logicalDatabaseName is not null)
                {
                    v3Configuration.Database = CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(
                        tenant, logicalDatabaseName);
                }

                if (logicalContainerName is not null)
                {
                    v3Configuration.Container = CosmosTenantedContainerNaming.GetTenantSpecificContainerNameFor(
                        tenant, logicalContainerName);
                }
            }
            else
            {
                if (v3Configuration.Database is null && logicalDatabaseName is not null)
                {
                    v3Configuration.Database = logicalDatabaseName;
                }

                if (v3Configuration.Container is null && logicalContainerName is not null)
                {
                    v3Configuration.Container = logicalContainerName;
                }
            }
        }

        Container result = await this.cosmosContainerSource.GetStorageContextAsync(
            v3Configuration,
            cosmosClientOptions,
            cancellationToken)
            .ConfigureAwait(false);

        if (!v3ConfigWasAvailable)
        {
            await result.Database.Client.CreateDatabaseIfNotExistsAsync(
                v3Configuration.Database,
                databaseThroughput,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            await result.Database.CreateContainerIfNotExistsAsync(
                v3Configuration.Container,
                partitionKeyPath,
                containerThroughput,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc/>
    public async ValueTask<CosmosContainerConfiguration?> MigrateToV3Async(
        ITenant tenant,
        string v2ConfigurationKey,
        string v3ConfigurationKey,
        IEnumerable<string>? databaseNames,
        IEnumerable<(string ContainerName, string PartitionKeyPath)>? containers,
        string? partitionKeyPath,
        int? databaseThroughput = null,
        int? containerThroughput = null,
        CosmosClientOptions? cosmosClientOptions = null,
        CancellationToken cancellationToken = default)
    {
        if (tenant.Properties.TryGet(v3ConfigurationKey, out CosmosContainerConfiguration _))
        {
            return null;
        }

        if (!tenant.Properties.TryGet(v2ConfigurationKey, out LegacyV2CosmosContainerConfiguration legacyConfiguration))
        {
            throw new InvalidOperationException($"Tenant did not contain storage configuration under either {v2ConfigurationKey} or {v3ConfigurationKey}");
        }

        CosmosContainerConfiguration v3Configuration = LegacyCosmosConfigurationConverter.FromV2ToV3(legacyConfiguration);
        if (containers == null)
        {
            if (partitionKeyPath is null)
            {
                throw new ArgumentException($"{nameof(partitionKeyPath)} must not be null if {nameof(containers)} is null", nameof(partitionKeyPath));
            }

            if (legacyConfiguration.ContainerName is string containerNameFromConfig)
            {
                containers = new[] { (containerNameFromConfig, partitionKeyPath) };
            }
            else
            {
                throw new InvalidOperationException($"When the configuration does not specify a Container, you must supply a non-null {nameof(containers)}");
            }
        }

        foreach (string databaseName in databaseNames ?? new[] { v3Configuration.Database })
        {
            CosmosContainerConfiguration v3ConfigForThisDb = v3Configuration with
            {
                Database = legacyConfiguration.DisableTenantIdPrefix
                    ? databaseName
                    : CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenant, databaseName),
            };

            // We need to ensure the database exists first. We need a CosmosClient to do that, and the
            // most straightforward way to get one with correctly configured credentials is to build
            // it from the configuration. However, we might not have a container in the configuration,
            // so we build a modified config with a fake container name. That container is never used.
            // It's just a way to get to the CosmosClient so we can create the database.
            Container contextForDb = await this.cosmosContainerSource.GetStorageContextAsync(
                v3ConfigForThisDb with { Container = "fakecontainername" },
                cosmosClientOptions,
                cancellationToken)
                .ConfigureAwait(false);

            await contextForDb.Database.Client.CreateDatabaseIfNotExistsAsync(
                v3ConfigForThisDb.Database,
                databaseThroughput,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            foreach ((string rawContainerName, string partitionKeyPathForContainer) in containers)
            {
                string containerName = legacyConfiguration.DisableTenantIdPrefix
                    ? rawContainerName
                    : CosmosTenantedContainerNaming.GetTenantSpecificContainerNameFor(tenant, rawContainerName);
                CosmosContainerConfiguration configForContainer = v3ConfigForThisDb with
                {
                    Container = containerName,
                };

                await contextForDb.Database.CreateContainerIfNotExistsAsync(
                    configForContainer.Container,
                    partitionKeyPathForContainer,
                    containerThroughput,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        // If tenant id prefix generation was enabled in the V2 config, and the database and/or
        // container names were in config, we need to put the tenant-specific names in the result.
        // But we have to wait until after the loop above, because that loop also adds in the
        // prefixes (but has to be able to do it whether the names came from config, or from
        // collections being passed in), and if we adjust them before then, we end up creating
        // databases and containers with a double tenant prefix!
        if (!legacyConfiguration.DisableTenantIdPrefix)
        {
            if (legacyConfiguration.DatabaseName is not null)
            {
                v3Configuration.Database = CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(
                    tenant, legacyConfiguration.DatabaseName);
            }

            if (legacyConfiguration.ContainerName is not null)
            {
                v3Configuration.Container = CosmosTenantedContainerNaming.GetTenantSpecificContainerNameFor(
                    tenant, legacyConfiguration.ContainerName);
            }
        }

        return v3Configuration;
    }
}