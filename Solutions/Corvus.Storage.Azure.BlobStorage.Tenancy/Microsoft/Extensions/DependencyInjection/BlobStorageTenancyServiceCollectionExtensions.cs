// <copyright file="BlobStorageTenancyServiceCollectionExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using Corvus.Storage.Azure.BlobStorage.Tenancy;
    using Corvus.Storage.Azure.BlobStorage.Tenancy.Internal;
    using Corvus.Tenancy.Internal;

    /// <summary>
    /// DI service configuration applications with stores implemented on top of tenanted blob
    /// storage.
    /// </summary>
    public static class BlobStorageTenancyServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services that enable applications that have used <c>Corvus.Tenancy</c> v2 to
        /// migrate to v3.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddTenantBlobContainerFactory(
            this IServiceCollection services)
        {
            return services.AddRequiredTenancyServices();
        }

        /// <summary>
        /// Adds services that enable applications that have used <c>Corvus.Tenancy</c> v2 to
        /// migrate to v3.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddBlobContainerV2ToV3Transition(
            this IServiceCollection services)
        {
            return services.AddSingleton<IBlobContainerSourceWithTenantLegacyTransition, BlobContainerSourceWithTenantLegacyTransition>();
        }
    }
}