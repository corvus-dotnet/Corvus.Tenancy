// <copyright file="TenancyBlobStorageServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
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
            if (configuration != null)
            {
                services.Configure<RootTenantDefaultStorageConfigurationOptions>(configuration);
            }

            services.AddTenantCloudBlobContainerFactory((sp, rootTenant) =>
            {
                RootTenantDefaultStorageConfigurationOptions options = sp.GetRequiredService<IOptions<RootTenantDefaultStorageConfigurationOptions>>().Value;
                if (string.IsNullOrWhiteSpace(options.StorageAccountName))
                {
                    ILogger<IStorageConfiguration> logger = sp.GetService<ILogger<IStorageConfiguration>>();
                    string message = $"No configuration has been provided for {nameof(options.StorageAccountName)}; development storage will be used. Please ensure the Storage Emulator is running.";
                    logger?.LogWarning(message);
                }

                IStorageConfiguration defaultConfiguration = sp.GetService<IStorageConfiguration>();

                defaultConfiguration.AccountName = options.StorageAccountName;
                defaultConfiguration.BlobStorageConfiguration = new BlobStorageConfiguration
                {
                    Container = string.IsNullOrWhiteSpace(options.BlobStorageContainerName) ? null : options.BlobStorageContainerName,
                };

                defaultConfiguration.SetStorageAccountKeySecretName(options.StorageKeySecretName);

                rootTenant.SetDefaultStorageConfiguration(defaultConfiguration);

                rootTenant.SetKeyVaultName(options.KeyVaultName);
            });

            return services;
        }
    }
}
