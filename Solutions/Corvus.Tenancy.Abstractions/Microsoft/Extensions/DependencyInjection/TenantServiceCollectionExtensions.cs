// <copyright file="TenantServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using Corvus.ContentHandling;
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
        /// <remarks>
        /// <para>
        /// This method historically did two things:
        /// </para>
        /// <para>
        /// Firstly, it registered the <see cref="RootTenant"/> class in the DI container as a singleton, allowing you
        /// to retrieve the <see cref="RootTenant"/> via DI. This should not be done; if you need the <see cref="RootTenant"/>,
        /// you should obtain it from the registered <see cref="ITenantProvider"/> by calling <see cref="ITenantProvider.Root"/>.
        /// </para>
        /// <para>
        /// Secondly it registered the <see cref="Tenant"/> type for content type based serialization. This should now be
        /// done by the registration method for the selected <see cref="ITenantProvider"/>.
        /// </para>
        /// <para>
        /// Note that this had the side effect of adding the Corvus.Extensions.Newtonsoft.Json
        /// <see cref="Corvus.Extensions.Json.IJsonSerializerSettingsProvider"/> and the four JsonConverters provided
        /// by that library to the collection (if not already present). <see cref="ITenantProvider"/> registration
        /// will no longer add these JsonConverters to the service collection. If you require them, you should add
        /// them as part of your own container initialisation.
        /// </para>
        /// </remarks>
        [Obsolete("This method is no longer required.")]
        public static IServiceCollection AddRootTenant(this IServiceCollection services)
        {
            if (services.Any(s => typeof(RootTenant).IsAssignableFrom(s.ServiceType)))
            {
                return services;
            }

            services.AddContentSerialization(contentFactory => contentFactory.RegisterContent<Tenant>());

            services.AddSingleton<RootTenant>();
            return services;
        }
    }
}
