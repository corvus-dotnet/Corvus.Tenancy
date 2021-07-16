// <copyright file="TenantCosmosContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Corvus.Extensions.Cosmos;
    using Corvus.Tenancy;

    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// A factory for tenanted <see cref="Container"/>s.
    /// </summary>
    internal class TenantCosmosContainerFactory :
        TenantedStorageContextFactory<Container, CosmosConfiguration>,
        ITenantCosmosContainerFactory
    {
        /// <summary>
        /// Creates a <see cref="TenantCosmosContainerFactory"/>.
        /// </summary>
        /// <param name="cosmosClientBuilderFactory">Client builder factory.</param>
        /// <param name="options">Configuration settings.</param>
        public TenantCosmosContainerFactory(
            ICosmosClientBuilderFactory cosmosClientBuilderFactory,
            TenantCosmosContainerFactoryOptions? options = null)
            : base(new CosmosContainerFactory(cosmosClientBuilderFactory, options))
        {
        }
    }
}