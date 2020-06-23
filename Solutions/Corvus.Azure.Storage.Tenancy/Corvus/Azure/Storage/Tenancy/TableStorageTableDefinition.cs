// <copyright file="TableStorageTableDefinition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    /// <summary>
    /// A definition of a blob storage container.
    /// </summary>
    public class TableStorageTableDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageTableDefinition"/> class.
        /// </summary>
        /// <param name="tableName">The <see cref="TableName"/>.</param>
        public TableStorageTableDefinition(string tableName)
        {
            this.TableName = tableName;
        }

        /// <summary>
        /// Gets or sets the container name.
        /// </summary>
        public string TableName { get; set; }
    }
}