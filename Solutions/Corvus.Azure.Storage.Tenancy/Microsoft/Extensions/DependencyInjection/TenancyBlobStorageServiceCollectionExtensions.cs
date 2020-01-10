// <copyright file="TenancyBlobStorageServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System.Linq;
    using Corvus.Azure.Storage.Tenancy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Common configuration code for services with stores implemented on top of tenanted
    /// storage.
    /// </summary>
    public static class TenancyBlobStorageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required by tenancy Azure storage based stores, and configures the default
        /// tenant's default storage account settings based on configuration settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">Configuration for the TenantCloudBlobContainerFactory.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantCloudBlobContainerFactory(
            this IServiceCollection services,
            TenantCloudBlobContainerFactoryOptions options)
        {
            if (services.Any(s => typeof(ITenantCloudBlobContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRootTenant();

            services.AddTenantCloudBlobContainerFactory(
                (sp, rootTenant) =>
            {
                if (string.IsNullOrWhiteSpace(options.RootTenantBlobStorageConfiguration?.AccountName))
                {
                    ILogger<BlobStorageConfiguration> logger = sp.GetService<ILogger<BlobStorageConfiguration>>();

                    string message = $"No configuration has been provided for {nameof(options.RootTenantBlobStorageConfiguration.AccountName)}; development storage will be used. Please ensure the Storage Emulator is running.";
                    logger?.LogWarning(message);
                }

                rootTenant.SetDefaultBlobStorageConfiguration(options.RootTenantBlobStorageConfiguration);
            }, options);

            return services;
        }
    }
}
