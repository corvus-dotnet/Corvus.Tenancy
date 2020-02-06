// <copyright file="TenancySqlServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System.Linq;
    using Corvus.Sql.Tenancy;
    using Corvus.Sql.Tenancy.Internal;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Common configuration code for services with stores implemented on top of tenanted
    /// storage.
    /// </summary>
    public static class TenancySqlServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required by tenanted SQL Server based stores, and configures the default
        /// tenant's default SQL Server connection settings based on settings stored in configuration.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">
        /// Configuration from which to read the settings, or null if the
        /// <see cref="IOptions{SqlConfiguration}"/> will be
        /// supplied to DI in some other way. This will typically be the root configuration.
        /// </param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantSqlConnectionFactory(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services.Any(s => typeof(ITenantSqlConnectionFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            if (configuration != null)
            {
                services.Configure<SqlConfiguration>(configuration.GetSection("ROOTTENANTSQLCONFIGURATIONOPTIONS"));
            }

            services.AddRootTenant();

            services.AddTenantSqlConnectionFactory((sp, rootTenant) =>
            {
                SqlConfiguration options = sp.GetRequiredService<IOptions<SqlConfiguration>>().Value;
                if (string.IsNullOrWhiteSpace(options.Database))
                {
                    ILogger<SqlConfiguration> logger = sp.GetService<ILogger<SqlConfiguration>>();
                    string message = $"No configuration has been provided for {nameof(options.Database)}; local database storage will be used. Please ensure the SQL local database service is configured correctly.";
                    logger?.LogWarning(message);
                }

                rootTenant.SetDefaultSqlConfiguration(options);
            });

            return services;
        }
    }
}