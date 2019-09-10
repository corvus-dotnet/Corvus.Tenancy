// <copyright file="RootTenantDefaultGremlinConfigurationOptions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy
{
    /// <summary>
    /// Defines settings for the default storage account in the root tenant.
    /// </summary>
    public sealed class RootTenantDefaultGremlinConfigurationOptions
    {
        /// <summary>
        /// Gets or sets the storage account name to use.
        /// </summary>
        public string GremlinHostName { get; set; }

        /// <summary>
        /// Gets or sets the port to use.
        /// </summary>
        public int GremlinPort { get; set; }

        /// <summary>
        /// Gets or sets the name of the key vault in which the account secret is stored.
        /// </summary>
        public string KeyVaultName { get; set; }

        /// <summary>
        /// Gets or sets the name of secret in the key vault in which the account secret is stored.
        /// </summary>
        public string GremlinAuthKeySecretName { get; set; }

        /// <summary>
        /// Gets or sets the Gremlin database name to use. Set this to force a particular
        /// container to be used regardless of what a <see cref="GremlinContainerDefinition"/> might
        /// specify.
        /// </summary>
        public string GremlinDatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the Gremlin container name to use. Set this to force a particular
        /// container to be used regardless of what a <see cref="GremlinContainerDefinition"/> might
        /// specify.
        /// </summary>
        public string GremlinContainerName { get; set; }
    }
}