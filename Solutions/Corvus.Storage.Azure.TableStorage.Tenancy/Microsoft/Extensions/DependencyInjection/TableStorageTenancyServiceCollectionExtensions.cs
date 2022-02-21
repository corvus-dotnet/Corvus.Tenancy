// <copyright file="TableStorageTenancyServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection;

using Corvus.Storage.Azure.TableStorage.Tenancy;
using Corvus.Storage.Azure.TableStorage.Tenancy.Internal;
using Corvus.Tenancy.Internal;

/// <summary>
/// DI service configuration applications with stores implemented on top of tenanted table storage.
/// </summary>
public static class TableStorageTenancyServiceCollectionExtensions
{
    /// <summary>
    /// Adds services that enable applications to use tenanted blob storage.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddTenantAzureTableFactory(
        this IServiceCollection services)
    {
        return services
            .AddRequiredTenancyServices()
            .AddAzureTableClientSourceFromDynamicConfiguration();
    }

    /// <summary>
    /// Adds services that enable applications that have used <c>Corvus.Tenancy</c> v2 to
    /// migrate to v3.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddAzureTableV2ToV3Transition(
        this IServiceCollection services)
    {
        return services.AddSingleton<ITableSourceWithTenantLegacyTransition, TableSourceWithTenantLegacyTransition>();
    }
}