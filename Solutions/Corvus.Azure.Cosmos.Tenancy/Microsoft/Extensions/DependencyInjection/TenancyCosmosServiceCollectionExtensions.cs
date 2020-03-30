// <copyright file="TenancyCosmosServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Corvus.Azure.Cosmos.Tenancy;
    using Corvus.Azure.Cosmos.Tenancy.Internal;
    using Corvus.Extensions.Cosmos;

    /// <summary>
    /// Common configuration code for services with stores implemented on top of tenanted
    /// storage.
    /// </summary>
    public static class TenancyCosmosServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required by tenancy Cosmos based stores, and configures the default
        /// tenant's default Cosmos account settings based on configuration settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">Configuration for the TenantCosmosContainerFactory.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantCosmosContainerFactory(
            this IServiceCollection services,
            TenantCosmosContainerFactoryOptions options)
        {
            return services.AddTenantCosmosContainerFactory(_ => options);
        }

        /// <summary>
        /// Add components for constructing tenant-specific Cosmos containers.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <param name="getOptions">Function to get the configuration options.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddTenantCosmosContainerFactory(
            this IServiceCollection services,
            Func<IServiceProvider, TenantCosmosContainerFactoryOptions> getOptions)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (services.Any(s => typeof(ITenantCosmosContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRootTenant();
            services.AddCosmosClientExtensions();

            services.AddSingleton<ITenantCosmosContainerFactory>(s =>
            {
                TenantCosmosContainerFactoryOptions options = getOptions(s);
                return new TenantCosmosContainerFactory(s.GetRequiredService<ICosmosClientBuilderFactory>(), options);
            });
            return services;
        }
    }
}