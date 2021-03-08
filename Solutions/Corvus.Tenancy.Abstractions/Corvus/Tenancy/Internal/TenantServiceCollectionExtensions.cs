// <copyright file="TenantServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Internal
{
    using System;
    using Corvus.ContentHandling;
    using Corvus.Tenancy;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extensions to add common serialization services required for tenancy to operate correctly.
    /// </summary>
    public static class TenantServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the required serialization services to the container.
        /// </summary>
        /// <param name="services">The service collection to which to add the services.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddRequiredTenancyServices(this IServiceCollection services)
        {
            services.AddJsonNetPropertyBag();
            services.AddContentTypeBasedSerializationSupport();

            services.AddContent(
                contentFactory =>
                {
                    if (!contentFactory.TryGetContentType(Tenant.RegisteredContentType, out Type _))
                    {
                        contentFactory.RegisterContent<Tenant>();
                    }
                });

            return services;
        }
    }
}
