// <copyright file="TenancyBlobStorageServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.Tenancy.Internal;

    /// <summary>
    /// Common configuration code for services with stores implemented on top of tenanted
    /// storage.
    /// </summary>
    public static class TenancyBlobStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required by tenancy Azure storage based stores.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">Configuration for the TenantBlobContainerClientFactory.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantBlobContainerClientFactory(
            this IServiceCollection services,
            TenantBlobContainerClientFactoryOptions options)
        {
            return services.AddTenantBlobContainerClientFactory(_ => options);
        }

        /// <summary>
        /// Add components for constructing tenant-specific blob storage containers.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <param name="getOptions">Function to get the configuration options.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddTenantBlobContainerClientFactory(
            this IServiceCollection services,
            Func<IServiceProvider, TenantBlobContainerClientFactoryOptions> getOptions)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (services.Any(s => typeof(ITenantBlobContainerClientFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRequiredTenancyServices();

            services.AddSingleton<ITenantBlobContainerClientFactory>(s =>
            {
                TenantBlobContainerClientFactoryOptions options = getOptions(s);

                return new TenantBlobContainerClientFactory(options);
            });

            return services;
        }
    }
}
