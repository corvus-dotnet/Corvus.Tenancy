// <copyright file="TenancyTableStorageServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Tenancy.Internal;

    /// <summary>
    /// Standard configuration code for services with stores implemented on top of tenanted
    /// storage.
    /// </summary>
    public static class TenancyTableStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required by tenancy Azure storage based stores.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">Configuration for the TenantCloudTableFactory.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantCloudTableFactory(
            this IServiceCollection services,
            TenantCloudTableFactoryOptions options)
        {
            return services.AddTenantCloudTableFactory(_ => options);
        }

        /// <summary>
        /// Add components for constructing tenant-specific table storage containers.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <param name="getOptions">Function to get the configuration options.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddTenantCloudTableFactory(
            this IServiceCollection services,
            Func<IServiceProvider, TenantCloudTableFactoryOptions> getOptions)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (services.Any(s => typeof(ITenantCloudTableFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRequiredTenancyServices();

            services.AddSingleton<ITenantCloudTableFactory>(s =>
            {
                TenantCloudTableFactoryOptions options = getOptions(s);

                return new TenantCloudTableFactory(options);
            });

            return services;
        }
    }
}
