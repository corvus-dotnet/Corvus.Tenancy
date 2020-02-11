// <copyright file="TenantCosmosServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy.Internal
{
    using System;
    using System.Linq;
    using Corvus.Extensions.Cosmos;
    using Corvus.Tenancy;
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
        /// <param name="getOptions">Function to get the configuration options.</param>
        /// <returns>The service collection.</returns>
        /// <remarks>
        /// This is typically called by <see cref="TenancyCosmosServiceCollectionExtensions.AddTenantCosmosContainerFactory(IServiceCollection, TenantCosmosContainerFactoryOptions)"/>.
        /// </remarks>
        public static IServiceCollection AddTenantCosmosContainerFactory(this IServiceCollection services, Action<IServiceProvider, ITenant> configureRootTenant, Func<IServiceProvider, TenantCosmosContainerFactoryOptions> getOptions)
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
            services.AddTransient<CosmosConfiguration, CosmosConfiguration>();
            services.AddSingleton<ITenantCosmosContainerFactory>(s =>
            {
                TenantCosmosContainerFactoryOptions options = getOptions(s);
                var result = new TenantCosmosContainerFactory(s.GetRequiredService<ICosmosClientBuilderFactory>(), options);
                configureRootTenant(s, s.GetRequiredService<RootTenant>());
                return result;
            });
            return services;
        }
    }
}