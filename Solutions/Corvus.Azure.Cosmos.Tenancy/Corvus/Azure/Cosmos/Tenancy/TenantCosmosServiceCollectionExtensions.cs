// <copyright file="TenantCosmosServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    using System;
    using System.Linq;
    using Corvus.Extensions.Cosmos;
    using Corvus.Tenancy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Adds installer methods for tenanted storage-related components.
    /// </summary>
    internal static class TenantCosmosServiceCollectionExtensions
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
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureRootTenant is null)
            {
                throw new ArgumentNullException(nameof(configureRootTenant));
            }

            if (services.Any(s => typeof(ITenantCosmosContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRootTenant();
            services.AddTransient<ICosmosConfiguration, CosmosConfiguration>();
            services.AddSingleton<ITenantCosmosContainerFactory>(s =>
            {
                var result = new TenantCosmosContainerFactory(s.GetRequiredService<IConfigurationRoot>(), s.GetRequiredService<ICosmosClientBuilderFactory>());
                configureRootTenant(s, s.GetRequiredService<RootTenant>());
                return result;
            });
            return services;
        }
    }
}