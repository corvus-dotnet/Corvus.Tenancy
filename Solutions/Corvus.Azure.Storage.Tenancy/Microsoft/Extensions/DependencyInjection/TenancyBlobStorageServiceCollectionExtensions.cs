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
        /// <param name="configuration">
        /// Configuration from which to read the settings, or null if the
        /// <see cref="IOptions{RootTenantDefaultStorageConfigurationOptions}"/> will be
        /// supplied to DI in some other way. This will typically be the root configuration.
        /// </param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantCloudBlobContainerFactory(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services.Any(s => typeof(ITenantCloudBlobContainerFactory).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            if (configuration != null)
            {
                services.Configure<RootTenantDefaultBlobStorageConfigurationOptions>(configuration);
            }

            services.AddContentFactory(factory => { });
            services.AddContentHandlingJsonConverters();

            services.AddTenantCloudBlobContainerFactory((sp, rootTenant) =>
            {
                RootTenantDefaultBlobStorageConfigurationOptions options = sp.GetRequiredService<IOptions<RootTenantDefaultBlobStorageConfigurationOptions>>().Value;
                if (string.IsNullOrWhiteSpace(options.AccountName))
                {
                    ILogger<BlobStorageConfiguration> logger = sp.GetService<ILogger<BlobStorageConfiguration>>();
                    string message = $"No configuration has been provided for {nameof(options.AccountName)}; development storage will be used. Please ensure the Storage Emulator is running.";
                    logger?.LogWarning(message);
                }

                BlobStorageConfiguration defaultConfiguration = sp.GetRequiredService<BlobStorageConfiguration>();

                defaultConfiguration.AccountName = options.AccountName;
                defaultConfiguration.Container = string.IsNullOrWhiteSpace(options.BlobStorageContainerName) ? null : options.BlobStorageContainerName;
                defaultConfiguration.AccountKeySecretName = options.AccountKeySecretName;

                rootTenant.SetDefaultStorageConfiguration(defaultConfiguration);

                rootTenant.SetKeyVaultName(options.KeyVaultName);
            });

            return services;
        }
    }
}
