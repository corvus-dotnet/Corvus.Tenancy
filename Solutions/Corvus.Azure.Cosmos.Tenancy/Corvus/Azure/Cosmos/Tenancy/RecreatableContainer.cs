// <copyright file="RecreatableContainer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// Represents a recreatable container.
    /// </summary>
    public struct RecreatableContainer
    {
        private readonly Func<Task<Container>> containerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecreatableContainer"/> struct.
        /// </summary>
        /// <param name="containerFactory">The factory method to create a new instance of the container.</param>
        /// <param name="instance">The initial container instance.</param>
        public RecreatableContainer(Func<Task<Container>> containerFactory, Container instance)
        {
            this.containerFactory = containerFactory;
            this.Instance = instance;
        }

        /// <summary>
        /// Gets the <see cref="Container"/>.
        /// </summary>
        public Container Instance { get; private set; }

        /// <summary>
        /// Recreates the container.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task RecreateContainer()
        {
            this.Instance = await this.containerFactory();
        }
    }
}
