// <copyright file="TenancyBlobStorageServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;

    using Corvus.Azure.Storage.Tenancy;
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
        /// <param name="options">Configuration for the TenantCloudBlobContainerFactory.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantCloudBlobContainerFactory(
            this IServiceCollection services,
            TenantCloudBlobContainerFactoryOptions options)
        {
            return services.AddTenantCloudBlobContainerFactory(_ => options);
        }

        /// <summary>
        /// Add components for constructing tenant-specific blob storage containers.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <param name="getOptions">Function to get the configuration options.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddTenantCloudBlobContainerFactory(
            this IServiceCollection services,
            Func<IServiceProvider, TenantCloudBlobContainerFactoryOptions> getOptions)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(getOptions);

            services.AddRequiredTenancyServices();

            services.AddSingleton<ITenantCloudBlobContainerFactory>(s =>
            {
                TenantCloudBlobContainerFactoryOptions options = getOptions(s);

                return new TenantCloudBlobContainerFactory(options);
            });

            return services;
        }
    }
}
