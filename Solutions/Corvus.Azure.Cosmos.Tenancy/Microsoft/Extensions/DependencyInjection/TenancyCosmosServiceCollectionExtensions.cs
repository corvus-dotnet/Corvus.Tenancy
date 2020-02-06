// <copyright file="TenancyCosmosServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System.Linq;
    using Corvus.Azure.Cosmos.Tenancy;
    using Corvus.Azure.Cosmos.Tenancy.Internal;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

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
        /// <param name="configuration">
        /// Configuration from which to read the settings, or null if the
        /// <see cref="IOptions{SqlConfiguration}"/> will be
        /// supplied to DI in some other way. This will typically be the root configuration.
        /// </param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantCosmosContainerFactory(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services.Any(s => typeof(ITenantCosmosContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            if (configuration != null)
            {
                services.Configure<CosmosConfiguration>(configuration.GetSection("ROOTTENANTCOSMOSCONFIGURATIONOPTIONS"));
            }

            services.AddRootTenant();

            services.AddCosmosClientExtensions();

            services.AddTenantCosmosContainerFactory((sp, rootTenant) =>
            {
                CosmosConfiguration options = sp.GetRequiredService<IOptions<CosmosConfiguration>>().Value;
                if (string.IsNullOrWhiteSpace(options.AccountUri))
                {
                    ILogger<CosmosConfiguration> logger = sp.GetService<ILogger<CosmosConfiguration>>();
                    string message = $"No configuration has been provided for {nameof(options.AccountUri)}; development storage will be used. Please ensure the Storage Emulator is running.";
                    logger?.LogWarning(message);
                }

                rootTenant.SetDefaultCosmosConfiguration(options);
            });

            return services;
        }
    }
}