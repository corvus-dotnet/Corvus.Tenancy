﻿// <copyright file="CosmosStorageTenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    using System;
    using Corvus.Tenancy;

    /// <summary>
    /// Extension methods for storage features for the tenant.
    /// </summary>
    public static class CosmosStorageTenantExtensions
    {
        private const string DefaultCosmosConfigurationKey = "DefaultCosmosConfiguration";

        /// <summary>
        /// Get the configuration for the specified Cosmos container definition for a particular tenant.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="definition">The Cosmos storage container definition.</param>
        /// <returns>The configuration for the Cosmos account for this tenant.</returns>
        public static CosmosConfiguration? GetCosmosConfiguration(this ITenant tenant, CosmosContainerDefinition definition)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            // First, try the configuration specific to this instance
            if (tenant.Properties.TryGet(GetConfigurationKey(definition), out CosmosConfiguration configuration))
            {
                return configuration;
            }

            // Otherwise, return the default
            return GetDefaultCosmosConfiguration(tenant);
        }

        /// <summary>
        /// Gets the default Cosmos configuration for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to get the default configuration.</param>
        /// <returns>The Default <see cref="CosmosConfiguration"/> for the tenant.</returns>
        public static CosmosConfiguration? GetDefaultCosmosConfiguration(this ITenant tenant)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (tenant.Properties.TryGet(DefaultCosmosConfigurationKey, out CosmosConfiguration configuration))
            {
                return configuration;
            }

            return null;
        }

        /// <summary>
        /// Sets the Cosmos configuration for the specified container for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to set the default configuration.</param>
        /// <param name="definition">The definition of the Cosmos container for which to set the configuration.</param>
        /// <param name="configuration">The configuration to set.</param>
        public static void SetCosmosConfiguration(this ITenant tenant, CosmosContainerDefinition definition, CosmosConfiguration configuration)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            tenant.Properties.Set(GetConfigurationKey(definition), configuration);
        }

        /// <summary>
        /// Sets the default Cosmos configuration for the tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to set the default configuration.</param>
        /// <param name="defaultConfiguration">The default configuration to set.</param>
        public static void SetDefaultCosmosConfiguration(this ITenant tenant, CosmosConfiguration defaultConfiguration)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (defaultConfiguration is null)
            {
                throw new ArgumentNullException(nameof(defaultConfiguration));
            }

            tenant.Properties.Set(DefaultCosmosConfigurationKey, defaultConfiguration);
        }

        private static string GetConfigurationKey(CosmosContainerDefinition definition)
        {
            return $"StorageConfiguration__{definition.DatabaseName}__{definition.ContainerName}";
        }
    }
}