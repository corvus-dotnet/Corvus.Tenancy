// <copyright file="TenancySqlServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Corvus.Sql.Tenancy;
    using Corvus.Sql.Tenancy.Internal;

    /// <summary>
    /// Common configuration code for services with stores implemented on top of tenanted
    /// storage.
    /// </summary>
    public static class TenancySqlServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required by tenanted SQL Server based stores.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">Configuration for the TenantBlobContainerClientFactory.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantSqlConnectionFactory(
            this IServiceCollection services,
            TenantSqlConnectionFactoryOptions options)
        {
            return services.AddTenantSqlConnectionFactory(_ => options);
        }

        /// <summary>
        /// Add components for constructing tenant-specific blob storage containers.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <param name="getOptions">Function to get the configuration options.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddTenantSqlConnectionFactory(
            this IServiceCollection services,
            Func<IServiceProvider, TenantSqlConnectionFactoryOptions> getOptions)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (services.Any(s => typeof(ITenantSqlConnectionFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddSingleton<ITenantSqlConnectionFactory>(s =>
            {
                TenantSqlConnectionFactoryOptions options = getOptions(s);

                return new TenantSqlConnectionFactory(options);
            });
            return services;
        }
    }
}