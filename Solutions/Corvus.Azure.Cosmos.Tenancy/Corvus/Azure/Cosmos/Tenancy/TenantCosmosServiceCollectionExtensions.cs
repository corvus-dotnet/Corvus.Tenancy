// <copyright file="TenantCosmosServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    using System;
    using Corvus.Extensions.Cosmos;
    using Corvus.Tenancy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Adds installer methods for tenanted storage-related components.
    /// </summary>
    public static class TenantCosmosServiceCollectionExtensions
    {
        /// <summary>
        /// Add components for constructing tenant-specific Cosmos containers.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <param name="configureRootTenant">A function that configures the root tenant.</param>
        /// <returns>The service collection.</returns>
        /// <remarks>
        /// This is typically called by <see cref="TenancyCosmosServiceCollectionExtensions.AddTenantCosmosContainerFactory(IServiceCollection, IConfiguration)"/>.
        /// </remarks>
        public static IServiceCollection AddTenantCosmosContainerFactory(this IServiceCollection services, Action<IServiceProvider, ITenant> configureRootTenant)
        {
            services.AddTransient<ICosmosConfiguration, CosmosConfiguration>();
            services.AddSingleton(s =>
            {
                ITenantProvider tenantProvider = s.GetRequiredService<ITenantProvider>();
                var result = new TenantCosmosContainerFactory(s.GetRequiredService<IConfigurationRoot>(), s.GetRequiredService<ICosmosClientBuilderFactory>());
                configureRootTenant(s, tenantProvider.Root);
                return result;
            });
            return services;
        }
    }
}