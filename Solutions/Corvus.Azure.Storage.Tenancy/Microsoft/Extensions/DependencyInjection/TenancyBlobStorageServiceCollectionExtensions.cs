// <copyright file="TenancyBlobStorageServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Corvus.Azure.Storage.Tenancy;
    using Microsoft.Extensions.Logging;

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
            return services.AddTenantCloudBlobContainerFactory(_ => options);
        }

        /// <summary>
        /// Adds services required by tenancy Azure storage based stores, and configures the default
        /// tenant's default storage account settings based on configuration settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="getOptions">Function to get the configuration options.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantCloudBlobContainerFactory(
            this IServiceCollection services,
            Func<IServiceProvider, TenantCloudBlobContainerFactoryOptions> getOptions)
        {
            if (services.Any(s => typeof(ITenantCloudBlobContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRootTenant();

            services.AddTenantCloudBlobContainerFactory(
                (sp, rootTenant) =>
            {
                TenantCloudBlobContainerFactoryOptions options = getOptions(sp);

                if (options is null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                ILogger<BlobStorageConfiguration> logger = sp.GetService<ILogger<BlobStorageConfiguration>>();

                if (options.RootTenantBlobStorageConfiguration != null)
                {
                    if (string.IsNullOrWhiteSpace(options.RootTenantBlobStorageConfiguration.AccountName))
                    {
                        string message = $"{nameof(options.RootTenantBlobStorageConfiguration)} has been supplied, but no configuration has been provided for {nameof(options.RootTenantBlobStorageConfiguration.AccountName)}; development storage will be used. Please ensure the Storage Emulator is running.";
                        logger?.LogWarning(message);
                    }
                    else
                    {
                        logger?.LogInformation(
                            "RootTenantBlobStorageConfiguration has beens supplied, with AccountName {accountName] and KeyVaultName {keyVaultName}",
                            options.RootTenantBlobStorageConfiguration.AccountName,
                            options.RootTenantBlobStorageConfiguration.KeyVaultName);
                    }

                    rootTenant.SetDefaultBlobStorageConfiguration(options.RootTenantBlobStorageConfiguration);
                }
                else
                {
                    logger?.LogInformation($"No {nameof(options.RootTenantBlobStorageConfiguration)} has been provided. No default Blob Storage configuration will be added to the Root tenant.");
                }
            }, getOptions);

            return services;
        }
    }
}
