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
        /// <param name="configuration">
        /// Configuration from which to read the settings, or null if the
        /// <see cref="IOptions{RootTenantDefaultGremlinConfigurationOptions}"/> will be
        /// supplied to DI in some other way. This will typically be the root configuration.
        /// </param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantGremlinContainerFactory(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services.Any(s => typeof(ITenantGremlinContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            if (configuration != null)
            {
                services.Configure<GremlinConfiguration>(configuration.GetSection("ROOTTENANTGREMLINCONFIGURATIONOPTIONS"));
            }

            services.AddRootTenant();

            services.AddTenantGremlinContainerFactory((sp, rootTenant) =>
            {
                GremlinConfiguration options = sp.GetRequiredService<IOptions<GremlinConfiguration>>().Value;
                if (string.IsNullOrWhiteSpace(options.HostName))
                {
                    ILogger<GremlinConfiguration> logger = sp.GetService<ILogger<GremlinConfiguration>>();
                    string message = $"No configuration has been provided for {nameof(options.HostName)}; development storage will be used. Please ensure the Storage Emulator is running.";
                    logger?.LogWarning(message);
                }

                rootTenant.SetDefaultGremlinConfiguration(options);
            });

            return services;
        }
    }
}