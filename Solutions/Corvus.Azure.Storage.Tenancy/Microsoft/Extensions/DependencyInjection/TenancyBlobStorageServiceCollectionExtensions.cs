// <copyright file="TenancyBlobStorageServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System.Linq;
    using Corvus.Azure.Storage.Tenancy;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Rest;

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
                services.Configure<RootTenantBlobStorageConfigurationOptions>(configuration);
            }

            services.AddRootTenant();

            services.AddTenantCloudBlobContainerFactory((sp, rootTenant) =>
            {
                RootTenantBlobStorageConfigurationOptions options = sp.GetRequiredService<IOptions<RootTenantBlobStorageConfigurationOptions>>().Value;
                if (string.IsNullOrWhiteSpace(options.AccountName))
                {
                    ILogger<BlobStorageConfiguration> logger = sp.GetService<ILogger<BlobStorageConfiguration>>();

                    string defaultConnectionString = configuration?.GetValue<string>("STORAGEACCOUNTCONNECTIONSTRING");
                    if (!string.IsNullOrWhiteSpace(defaultConnectionString))
                    {
                        options.AccountName = defaultConnectionString;
                        string message = $"No configuration has been provided for {nameof(options.AccountName)}; the connection string in the configuration key STORAGEACCOUNTCONNECTIONSTRING will be used.";
                        logger?.LogWarning(message);
                    }
                    else
                    {
                        string message = $"No configuration has been provided for {nameof(options.AccountName)}; development storage will be used. Please ensure the Storage Emulator is running.";
                        logger?.LogWarning(message);
                    }
                }

                rootTenant.SetDefaultStorageConfiguration(options.CreateBlobStorageConfigurationInstance());
            });

            return services;
        }
    }
}
