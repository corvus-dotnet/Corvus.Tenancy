// <copyright file="CosmosTenancyServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection;

using Corvus.Storage.Azure.Cosmos.Tenancy;
using Corvus.Storage.Azure.Cosmos.Tenancy.Internal;
using Corvus.Tenancy.Internal;

/// <summary>
/// DI service configuration applications with stores implemented on top of tenanted Cosmos DB.
/// </summary>
public static class CosmosTenancyServiceCollectionExtensions
{
    /// <summary>
    /// Adds services that enable applications to use tenanted Cosmos DB storage.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddTenantCosmosConnectionFactory(
        this IServiceCollection services)
    {
        return services
            .AddRequiredTenancyServices()
            .AddCosmosContainerSourceFromDynamicConfiguration();
    }

    /// <summary>
    /// Adds services that enable applications that have used <c>Corvus.Tenancy</c> v2 to
    /// migrate to v3.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddCosmosContainerV2ToV3Transition(
        this IServiceCollection services)
    {
        return services.AddSingleton<ICosmosContainerSourceWithTenantLegacyTransition, CosmosContainerSourceWithTenantLegacyTransition>();
    }
}