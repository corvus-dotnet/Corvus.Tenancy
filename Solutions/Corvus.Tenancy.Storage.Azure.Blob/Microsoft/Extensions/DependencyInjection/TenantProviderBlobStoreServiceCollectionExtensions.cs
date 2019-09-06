﻿// <copyright file="TenantProviderBlobStoreServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System.Linq;
    using Corvus.Tenancy;

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
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantProviderBlobStore(
            this IServiceCollection services)
        {
            if (services.Any(s => typeof(ITenantProvider).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddRootTenant();
            services.AddSingleton<ITenantProvider, TenantProviderBlobStore>();
            return services;
        }
    }
}
