// <copyright file="GremlinContainerDefinition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy
{
    using System;

    /// <summary>
    /// A definition of a Gremlin container.
    /// </summary>
    public class GremlinContainerDefinition
    {
        private string? databaseName;
        private string? containerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="GremlinContainerDefinition"/> class.
        /// </summary>
        public GremlinContainerDefinition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GremlinContainerDefinition"/> class.
        /// </summary>
        /// <param name="databaseName">The <see cref="DatabaseName"/>.</param>
        /// <param name="containerName">The <see cref="ContainerName"/>.</param>
        public GremlinContainerDefinition(string databaseName, string containerName)
        {
            this.DatabaseName = databaseName;
            this.ContainerName = containerName;
        }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string DatabaseName
        {
            get => this.databaseName ?? throw new InvalidOperationException(nameof(this.DatabaseName) + " has not been set");
            set => this.databaseName = value ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        public string ContainerName
        {
            get => this.containerName ?? throw new InvalidOperationException(nameof(this.ContainerName) + " has not been set");
            set => this.containerName = value ?? throw new ArgumentNullException();
        }
    }
}