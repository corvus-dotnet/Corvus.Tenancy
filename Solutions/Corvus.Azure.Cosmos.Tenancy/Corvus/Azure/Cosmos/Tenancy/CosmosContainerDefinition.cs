// <copyright file="CosmosContainerDefinition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    /// <summary>
    /// A definition of a Cosmos container.
    /// </summary>
    public class CosmosContainerDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosContainerDefinition"/> class.
        /// </summary>
        public CosmosContainerDefinition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosContainerDefinition"/> class.
        /// </summary>
        /// <param name="databaseName">The <see cref="DatabaseName"/>.</param>
        /// <param name="containerName">The <see cref="ContainerName"/>.</param>
        /// <param name="partitionKeyPath">The <see cref="PartitionKeyPath"/>.</param>
        /// <param name="containerThroughput">The <see cref="ContainerThroughput"/>, or null if default container-level throughput is to be used.</param>
        /// <param name="databaseThroughput">The <see cref="DatabaseThroughput"/>, or null if no database-level throughput is required.</param>
        public CosmosContainerDefinition(string databaseName, string containerName, string partitionKeyPath, int? containerThroughput = null, int? databaseThroughput = null)
        {
            this.DatabaseName = databaseName;
            this.ContainerName = containerName;
            this.PartitionKeyPath = partitionKeyPath;
            this.ContainerThroughput = containerThroughput;
            this.DatabaseThroughput = databaseThroughput;
        }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the database throughput, where database-level throughput is required.
        /// </summary>
        public int? DatabaseThroughput { get; set; }

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the container throughput, where container-level throughput is required.
        /// </summary>
        public int? ContainerThroughput { get; set; }

        /// <summary>
        /// Gets or sets the partition key path.
        /// </summary>
        public string PartitionKeyPath { get; set; }
    }
}