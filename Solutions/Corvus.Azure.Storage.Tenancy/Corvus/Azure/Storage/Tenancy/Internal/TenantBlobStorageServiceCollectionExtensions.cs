// <copyright file="TenantBlobStorageServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using System;
    using System.Linq;
    using Corvus.Tenancy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Adds installer methods for tenanted storage-related components.
    /// </summary>
    internal static class TenantBlobStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Add components for constructing tenant-specific blob storage containers.
        /// </summary>
        /// <param name="services">The target service collection.</param>
        /// <param name="configureRootTenant">A function that configures the root tenant.</param>
        /// <returns>The service collection.</returns>
        /// <remarks>
        /// This is typically called by <see cref="TenancyBlobStorageServiceCollectionExtensions.AddTenantCloudBlobContainerFactory(IServiceCollection, IConfiguration)"/>.
        /// </remarks>
        public static IServiceCollection AddTenantCloudBlobContainerFactory(this IServiceCollection services, Action<IServiceProvider, ITenant> configureRootTenant)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureRootTenant is null)
            {
                throw new ArgumentNullException(nameof(configureRootTenant));
            }

            if (services.Any(s => typeof(ITenantCloudBlobContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRootTenant();
            services.AddTransient<BlobStorageConfiguration>();
            services.AddSingleton<ITenantCloudBlobContainerFactory>(s =>
            {
                ITenant tenant = s.GetRequiredService<RootTenant>();
                var result = new TenantCloudBlobContainerFactory(s.GetRequiredService<IConfigurationRoot>());
                configureRootTenant(s, tenant);
                return result;
            });
            return services;
        }
    }
}