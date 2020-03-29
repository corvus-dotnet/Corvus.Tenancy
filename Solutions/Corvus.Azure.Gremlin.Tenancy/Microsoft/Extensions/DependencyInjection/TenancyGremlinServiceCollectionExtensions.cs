// <copyright file="TenancyGremlinServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Corvus.Azure.GremlinExtensions.Tenancy;
    using Corvus.Azure.GremlinExtensions.Tenancy.Internal;
    using Microsoft.Extensions.Logging;

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
            return services.AddTenantGremlinContainerFactory(_ => options);
        }

        /// <summary>
        /// Adds services required by tenancy Gremlin based stores, and configures the default
        /// tenant's default Gremlin account settings based on configuration settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="getOptions">Function to get the configuration options.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantGremlinContainerFactory(
        this IServiceCollection services,
        Func<IServiceProvider, TenantGremlinContainerFactoryOptions> getOptions)
        {
            if (services.Any(s => typeof(ITenantGremlinContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRootTenant();

            services.AddTenantGremlinContainerFactory(
                (sp, rootTenant) =>
            {
                TenantGremlinContainerFactoryOptions options = getOptions(sp);

                if (options is null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                ILogger<GremlinConfiguration> logger = sp.GetService<ILogger<GremlinConfiguration>>();

                if (options.RootTenantGremlinConfiguration != null)
                {
                    if (string.IsNullOrWhiteSpace(options.RootTenantGremlinConfiguration.HostName))
                    {
                        string message = $"{nameof(options.RootTenantGremlinConfiguration)} has been supplied, but no configuration has been provided for {nameof(options.RootTenantGremlinConfiguration.HostName)}; development storage will be used. Please ensure the Storage Emulator is running.";
                        logger?.LogWarning(message);
                    }
                    else
                    {
                        logger?.LogInformation(
                            "RootTenantGremlinConfiguration has beens supplied, with HostName {hostName] and KeyVaultName {keyVaultName}",
                            options.RootTenantGremlinConfiguration.HostName,
                            options.RootTenantGremlinConfiguration.KeyVaultName);
                    }

                    rootTenant.SetDefaultGremlinConfiguration(options.RootTenantGremlinConfiguration);
                }
                else
                {
                    logger?.LogInformation($"No {nameof(options.RootTenantGremlinConfiguration)} has been provided. No default Gremlin configuration will be added to the Root tenant.");
                }
            }, getOptions);

            return services;
        }
    }
}