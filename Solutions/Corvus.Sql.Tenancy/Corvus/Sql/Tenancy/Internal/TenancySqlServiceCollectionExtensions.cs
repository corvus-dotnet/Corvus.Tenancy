﻿// <copyright file="TenancySqlServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Sql.Tenancy.Internal
{
    using System;
    using System.Linq;
    using Corvus.Tenancy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Adds installer methods for tenanted storage-related components.
    /// </summary>
    internal static class TenancySqlServiceCollectionExtensions
    {
        /// <summary>
        /// Add components for constructing tenant-specific Sql containers.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <param name="configureRootTenant">A function that configures the root tenant.</param>
        /// <returns>The service collection.</returns>
        /// <remarks>
        /// This is typically called by <see cref="Microsoft.Extensions.DependencyInjection.TenancySqlServiceCollectionExtensions.AddTenantSqlConnectionFactory(IServiceCollection, IConfiguration)"/>.
        /// </remarks>
        public static IServiceCollection AddTenantSqlConnectionFactory(this IServiceCollection services, Action<IServiceProvider, ITenant> configureRootTenant)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureRootTenant is null)
            {
                throw new ArgumentNullException(nameof(configureRootTenant));
            }

            if (services.Any(s => typeof(ITenantSqlConnectionFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRootTenant();
            services.AddTransient<SqlConfiguration, SqlConfiguration>();
            services.AddSingleton<ITenantSqlConnectionFactory>(s =>
            {
                var result = new TenantSqlConnectionFactory(s.GetRequiredService<IConfigurationRoot>());
                configureRootTenant(s, s.GetRequiredService<RootTenant>());
                return result;
            });
            return services;
        }
    }
}