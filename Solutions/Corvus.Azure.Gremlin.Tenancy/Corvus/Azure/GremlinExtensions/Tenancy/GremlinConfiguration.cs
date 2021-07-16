﻿// <copyright file="GremlinConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy
{
    /// <summary>
    /// Encapsulates configuration for a container in a specific Gremlin account.
    /// </summary>
    public class GremlinConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GremlinConfiguration"/> class.
        /// </summary>
        public GremlinConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        public string? HostName { get; set; }

        /// <summary>
        /// Gets or sets the port number.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the name of the key vault in which the account secret is stored.
        /// </summary>
        public string? KeyVaultName { get; set; }

        /// <summary>
        /// Gets or sets the account key secret mame.
        /// </summary>
        public string? AuthKeySecretName { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        public string? ContainerName { get; set; }
    }
}