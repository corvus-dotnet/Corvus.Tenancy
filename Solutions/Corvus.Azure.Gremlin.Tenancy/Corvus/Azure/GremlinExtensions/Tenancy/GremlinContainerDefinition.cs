// <copyright file="GremlinContainerDefinition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.GremlinExtensions.Tenancy
{
    /// <summary>
    /// A definition of a Gremlin container.
    /// </summary>
    public class GremlinContainerDefinition
    {
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
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        public string ContainerName { get; set; }
    }
}