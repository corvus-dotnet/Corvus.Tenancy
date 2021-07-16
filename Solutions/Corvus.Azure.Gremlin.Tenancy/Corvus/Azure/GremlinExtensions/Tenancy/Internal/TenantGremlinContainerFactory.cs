// <copyright file="TenantGremlinContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy.Internal
{
    using Corvus.Tenancy;

    using Gremlin.Net.Driver;

    /// <summary>
    /// A factory for tenanted <see cref="GremlinClient"/>s.
    /// </summary>
    internal class TenantGremlinContainerFactory :
        TenantedStorageContextFactory<GremlinClient, GremlinConfiguration>,
        ITenantGremlinContainerFactory
    {
        /// <summary>
        /// Creates a <see cref="TenantGremlinContainerFactory"/>.
        /// </summary>
        /// <param name="options">Configuration settings.</param>
        public TenantGremlinContainerFactory(
            TenantGremlinContainerFactoryOptions options)
            : base(new GremlinContainerFactory(options))
        {
        }
    }
}
