// <copyright file="TenantCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy.Internal
{
    using Corvus.Tenancy;

    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// A factory for tenanted <see cref="CloudTableClient"/>s.
    /// </summary>
    internal class TenantCloudTableFactory :
        TenantedStorageContextFactory<CloudTable, TableStorageConfiguration>,
        ITenantCloudTableFactory
    {
        /// <summary>
        /// Creates a <see cref="TenantCloudTableFactory"/>.
        /// </summary>
        /// <param name="options">Configuration settings.</param>
        public TenantCloudTableFactory(TenantCloudTableFactoryOptions? options = null)
            : base(new CloudTableFactory(options))
        {
        }
    }
}