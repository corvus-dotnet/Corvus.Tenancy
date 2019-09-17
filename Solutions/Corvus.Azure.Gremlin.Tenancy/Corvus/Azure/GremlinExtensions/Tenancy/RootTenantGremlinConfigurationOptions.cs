// <copyright file="RootTenantGremlinConfigurationOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy
{
    /// <summary>
    /// Defines settings for the default storage account in the root tenant.
    /// </summary>
    public sealed class RootTenantGremlinConfigurationOptions : GremlinConfiguration
    {
        /// <summary>
        /// Creates a new instance of a <see cref="GremlinConfiguration"/> from these options.
        /// </summary>
        /// <returns>The <see cref="GremlinConfiguration"/> derived from these options.</returns>
        public GremlinConfiguration CreateGremlinConfigurationInstance()
        {
            return new GremlinConfiguration()
            {
                AuthKeySecretName = this.AuthKeySecretName,
                ContainerName = string.IsNullOrWhiteSpace(this.ContainerName) ? null : this.ContainerName,
                DatabaseName = string.IsNullOrWhiteSpace(this.DatabaseName) ? null : this.DatabaseName,
                DisableTenantIdPrefix = this.DisableTenantIdPrefix,
                HostName = this.HostName,
                Port = this.Port,
                KeyVaultName = this.KeyVaultName,
            };
        }
    }
}