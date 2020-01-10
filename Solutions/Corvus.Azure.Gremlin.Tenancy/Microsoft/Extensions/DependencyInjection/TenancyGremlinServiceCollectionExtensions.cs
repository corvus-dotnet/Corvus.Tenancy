// <copyright file="TenancyGremlinServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System.Linq;
    using Corvus.Azure.GremlinExtensions.Tenancy;
    using Corvus.Azure.GremlinExtensions.Tenancy.Internal;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Common configuration code for services with stores implemented on top of tenanted
    /// storage.
    /// </summary>
    public static class TenancyGremlinServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required by tenancy Gremlin based stores, and configures the default
        /// tenant's default Gremlin account settings based on configuration settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">Configuration for the TenantGremlinContainerFactory.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantGremlinContainerFactory(
            this IServiceCollection services,
            TenantGremlinContainerFactoryOptions options)
        {
            if (services.Any(s => typeof(ITenantGremlinContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRootTenant();

            services.AddTenantGremlinContainerFactory(
                (sp, rootTenant) =>
            {
                if (string.IsNullOrWhiteSpace(options.RootTenantGremlinConfiguration?.HostName))
                {
                    ILogger<GremlinConfiguration> logger = sp.GetService<ILogger<GremlinConfiguration>>();
                    string message = $"No configuration has been provided for {nameof(options.RootTenantGremlinConfiguration.HostName)}; development storage will be used. Please ensure the Storage Emulator is running.";
                    logger?.LogWarning(message);
                }

                rootTenant.SetDefaultGremlinConfiguration(options.RootTenantGremlinConfiguration);
            }, options);

            return services;
        }
    }
}