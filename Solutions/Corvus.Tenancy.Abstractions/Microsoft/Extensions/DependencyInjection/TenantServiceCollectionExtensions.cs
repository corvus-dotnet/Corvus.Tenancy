// <copyright file="TenantServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using Corvus.Tenancy;

    /// <summary>
    /// Extensions to register the Root tenant with the service collection.
    /// </summary>
    public static class TenantServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the root tenant to the collection.
        /// </summary>
        /// <param name="services">The service collection to which to add the root tenant.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddRootTenant(this IServiceCollection services)
        {
            services.AddSingleton<ITenant>(new Tenant { Id = "Root" });
            return services;
        }
    }
}
