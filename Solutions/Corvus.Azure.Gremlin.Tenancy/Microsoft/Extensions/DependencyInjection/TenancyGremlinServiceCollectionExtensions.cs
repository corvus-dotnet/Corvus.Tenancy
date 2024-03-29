﻿// <copyright file="TenancyGremlinServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Corvus.Azure.GremlinExtensions.Tenancy;
    using Corvus.Azure.GremlinExtensions.Tenancy.Internal;
    using Corvus.Tenancy.Internal;

    /// <summary>
    /// Common configuration code for services with stores implemented on top of tenanted
    /// storage.
    /// </summary>
    public static class TenancyGremlinServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required by tenancy Gremlin based stores.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">Configuration for the TenantGremlinContainerFactory.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantGremlinContainerFactory(
            this IServiceCollection services,
            TenantGremlinContainerFactoryOptions options)
        {
            return services.AddTenantGremlinContainerFactory(_ => options);
        }

        /// <summary>
        /// Add components for constructing tenant-specific Gremlin containers.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <param name="getOptions">Function to get the configuration options.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddTenantGremlinContainerFactory(
            this IServiceCollection services,
            Func<IServiceProvider, TenantGremlinContainerFactoryOptions> getOptions)
        {
            ArgumentNullException.ThrowIfNull(services);

            if (services.Any(s => typeof(ITenantGremlinContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRequiredTenancyServices();

            services.AddSingleton<ITenantGremlinContainerFactory>(s =>
            {
                TenantGremlinContainerFactoryOptions options = getOptions(s);
                return new TenantGremlinContainerFactory(options);
            });
            return services;
        }
    }
}