// <copyright file="SqlConnectionDefinition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Sql.Tenancy
{
    /// <summary>
    /// A definition of a SQL Connection.
    /// </summary>
    public class SqlConnectionDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlConnectionDefinition"/> class.
        /// </summary>
        public SqlConnectionDefinition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlConnectionDefinition"/> class.
        /// </summary>
        /// <param name="database">The <see cref="Database"/> to which to connect.</param>
        /// <remarks>Note that this would typically be the database name. In test scenarios, you may choose to provide an entire connection string for the database in this property.</remarks>
        public SqlConnectionDefinition(string database)
        {
            this.Database = database;
        }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        /// <remarks>
        /// <para>Note that this would typically be the database name. In test scenarios, you may choose to provide an entire connection string for the database in this property.</para>
        /// <para>Otherwise, it is used to append the <c>InitialCatalog</c> property of the connection string supplied through the <see cref="SqlConfiguration"/>.</para>
        /// </remarks>
        public string Database { get; set; }
    }
}