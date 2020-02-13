// <copyright file="TenancySqlServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Corvus.Sql.Tenancy;
    using Corvus.Sql.Tenancy.Internal;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Common configuration code for services with stores implemented on top of tenanted
    /// storage.
    /// </summary>
    public static class TenancySqlServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required by tenanted SQL Server based stores, and configures the default
        /// tenant's default SQL Server connection based on configuration settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">Configuration for the TenantCloudBlobContainerFactory.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantSqlConnectionFactory(
            this IServiceCollection services,
            TenantSqlConnectionFactoryOptions options)
        {
            return services.AddTenantSqlConnectionFactory(_ => options);
        }

        /// <summary>
        /// Adds services required by tenanted SQL Server based stores, and configures the default
        /// tenant's default SQL Server connection based on configurationb settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="getOptions">Function to get the configuration options.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantSqlConnectionFactory(
            this IServiceCollection services,
            Func<IServiceProvider, TenantSqlConnectionFactoryOptions> getOptions)
        {
            if (services.Any(s => typeof(ITenantSqlConnectionFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRootTenant();

            services.AddTenantSqlConnectionFactory(
                (sp, rootTenant) =>
            {
                TenantSqlConnectionFactoryOptions options = getOptions(sp);

                if (options is null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                if (options.RootTenantSqlConfiguration is null)
                {
                    throw new ArgumentNullException(nameof(options.RootTenantSqlConfiguration));
                }

                if (string.IsNullOrWhiteSpace(options.RootTenantSqlConfiguration.Database))
                {
                    ILogger<SqlConfiguration> logger = sp.GetService<ILogger<SqlConfiguration>>();
                    string message = $"No configuration has been provided for {nameof(options.RootTenantSqlConfiguration.Database)}; local database storage will be used. Please ensure the SQL local database service is configured correctly.";
                    logger?.LogWarning(message);
                }

                rootTenant.SetDefaultSqlConfiguration(options.RootTenantSqlConfiguration);
            }, getOptions);

            return services;
        }
    }
}