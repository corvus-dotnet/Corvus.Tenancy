// <copyright file="TenancyCosmosServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Corvus.Azure.Cosmos.Tenancy;
    using Corvus.Azure.Cosmos.Tenancy.Internal;
    using Microsoft.Extensions.Logging;

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
        /// Adds services required by tenancy Cosmos based stores, and configures the default
        /// tenant's default Cosmos account settings based on configuration settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="getOptions">Function to get the configuration options.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantCosmosContainerFactory(
        this IServiceCollection services,
        Func<IServiceProvider, TenantCosmosContainerFactoryOptions> getOptions)
        {
            if (services.Any(s => typeof(ITenantCosmosContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRootTenant();

            services.AddCosmosClientExtensions();

            services.AddTenantCosmosContainerFactory(
                (sp, rootTenant) =>
                {
                    TenantCosmosContainerFactoryOptions options = getOptions(sp);

                    if (options is null)
                    {
                        throw new ArgumentNullException(nameof(options));
                    }

                    if (options.RootTenantCosmosConfiguration is null)
                    {
                        throw new ArgumentNullException(nameof(options.RootTenantCosmosConfiguration));
                    }

                    if (string.IsNullOrWhiteSpace(options.RootTenantCosmosConfiguration.AccountUri))
                    {
                        ILogger<CosmosConfiguration> logger = sp.GetService<ILogger<CosmosConfiguration>>();
                        string message = $"No configuration has been provided for {nameof(options.RootTenantCosmosConfiguration.AccountUri)}; development storage will be used. Please ensure the Storage Emulator is running.";
                        logger?.LogWarning(message);
                    }

                    rootTenant.SetDefaultCosmosConfiguration(options.RootTenantCosmosConfiguration);
                },
                getOptions);

            return services;
        }
    }
}