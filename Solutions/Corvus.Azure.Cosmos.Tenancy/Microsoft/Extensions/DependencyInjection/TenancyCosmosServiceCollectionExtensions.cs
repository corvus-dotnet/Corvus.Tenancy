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

                    ILogger<CosmosConfiguration> logger = sp.GetService<ILogger<CosmosConfiguration>>();

                    if (options.RootTenantCosmosConfiguration != null)
                    {
                        if (string.IsNullOrWhiteSpace(options.RootTenantCosmosConfiguration.AccountUri))
                        {
                            string message = $"{nameof(options.RootTenantCosmosConfiguration)} has been supplied, but no configuration has been provided for {nameof(options.RootTenantCosmosConfiguration.AccountUri)}; development storage will be used. Please ensure the Storage Emulator is running.";
                            logger?.LogWarning(message);
                        }
                        else
                        {
                            logger?.LogInformation(
                                "RootTenantCosmosConfiguration has beens supplied, with AccountUri {accountUri] and KeyVaultName {keyVaultName}",
                                options.RootTenantCosmosConfiguration.AccountUri,
                                options.RootTenantCosmosConfiguration.KeyVaultName);
                        }

                        rootTenant.SetDefaultCosmosConfiguration(options.RootTenantCosmosConfiguration);
                    }
                    else
                    {
                        logger?.LogInformation($"No {nameof(options.RootTenantCosmosConfiguration)} has been provided. No default Cosmos configuration will be added to the Root tenant.");
                    }
                },
                getOptions);

            return services;
        }
    }
}