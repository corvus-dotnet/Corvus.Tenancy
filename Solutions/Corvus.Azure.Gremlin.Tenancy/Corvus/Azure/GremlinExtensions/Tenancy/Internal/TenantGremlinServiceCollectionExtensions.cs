// <copyright file="TenantGremlinServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy.Internal
{
    using System;
    using System.Linq;
    using Corvus.Tenancy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Adds installer methods for tenanted storage-related components.
    /// </summary>
    internal static class TenantGremlinServiceCollectionExtensions
    {
        /// <summary>
        /// Add components for constructing tenant-specific Gremlin containers.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <param name="configureRootTenant">A function that configures the root tenant.</param>
        /// <param name="options">Configuration for the TenantGremlinContainerFactory.</param>
        /// <returns>The service collection.</returns>
        /// <remarks>
        /// This is typically called by <see cref="TenancyGremlinServiceCollectionExtensions.AddTenantGremlinContainerFactory(IServiceCollection, TenantGremlinContainerFactoryOptions)"/>.
        /// </remarks>
        public static IServiceCollection AddTenantGremlinContainerFactory(this IServiceCollection services, Action<IServiceProvider, ITenant> configureRootTenant, TenantGremlinContainerFactoryOptions options)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureRootTenant is null)
            {
                throw new ArgumentNullException(nameof(configureRootTenant));
            }

            if (services.Any(s => typeof(ITenantGremlinContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRootTenant();
            services.AddTransient<GremlinConfiguration, GremlinConfiguration>();
            services.AddSingleton<ITenantGremlinContainerFactory>(s =>
            {
                var result = new TenantGremlinContainerFactory(options);
                configureRootTenant(s, s.GetRequiredService<RootTenant>());
                return result;
            });
            return services;
        }
    }
}