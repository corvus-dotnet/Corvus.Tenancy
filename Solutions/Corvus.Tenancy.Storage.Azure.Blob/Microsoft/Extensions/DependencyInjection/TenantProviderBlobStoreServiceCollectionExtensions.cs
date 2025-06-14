﻿// <copyright file="TenantProviderBlobStoreServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;

    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Json;
    using Corvus.Json.Serialization;
    using Corvus.Tenancy;
    using Corvus.Tenancy.Internal;

    /// <summary>
    /// Common configuration code for services with stores implemented on top of tenanted
    /// storage.
    /// </summary>
    public static class TenantProviderBlobStoreServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services an Azure Blob storage-based implementation of <see cref="ITenantProvider"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="getRootTenantStorageConfiguration">
        /// A function that returns the <see cref="BlobStorageConfiguration"/> that will be used for the root tenant to
        /// determine where to store its children.
        /// </param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantProviderBlobStore(
            this IServiceCollection services,
            Func<IServiceProvider, BlobStorageConfiguration> getRootTenantStorageConfiguration)
        {
            if (services.Any(s => typeof(ITenantProvider).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRequiredTenancyServices();

            services.AddSingleton(sp =>
            {
                BlobStorageConfiguration rootTenantStorageConfig = getRootTenantStorageConfiguration(sp);

                IPropertyBagFactory propertyBagFactory = sp.GetRequiredService<IPropertyBagFactory>();
                var rootTenant = new RootTenant(propertyBagFactory);

                rootTenant.UpdateProperties(
                    values => values.AddBlobStorageConfiguration(
                        TenantProviderBlobStore.ContainerDefinition, rootTenantStorageConfig));

                ITenantCloudBlobContainerFactory tenantCloudBlobContainerFactory = sp.GetRequiredService<ITenantCloudBlobContainerFactory>();
                IJsonSerializerOptionsProvider serializerSettingsProvider = sp.GetRequiredService<IJsonSerializerOptionsProvider>();

                return new TenantProviderBlobStore(rootTenant, propertyBagFactory, tenantCloudBlobContainerFactory, serializerSettingsProvider);
            });

            services.AddSingleton<ITenantStore>(sp => sp.GetRequiredService<TenantProviderBlobStore>());
            services.AddSingleton<ITenantProvider>(sp => sp.GetRequiredService<TenantProviderBlobStore>());
            return services;
        }
    }
}