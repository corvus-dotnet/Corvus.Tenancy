// <copyright file="SqlConnectionDefinition.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Sql.Tenancy
{
    using System;

    /// <summary>
    /// Specifies the name of a database to be used to create a SQL connection.
    /// </summary>
    /// <remarks>
    /// This is typically used in conjunction with a <see cref="SqlConfiguration"/> which provides a base SQL connection string for a server,
    /// via an <see cref="ITenantSqlConnectionFactory"/>, in order to create a <see cref="System.Data.SqlClient.SqlConnection"/>.
    /// </remarks>
    public class SqlConnectionDefinition
    {
        private string? database;

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
        /// <para>Otherwise, it is used to append the <c>Initial Catalog</c> property of the connection string supplied through the <see cref="SqlConfiguration"/>.</para>
        /// </remarks>
        public string Database
        {
            get => this.database ?? throw new InvalidOperationException(nameof(this.Database) + " has not been set");
            set => this.database = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}