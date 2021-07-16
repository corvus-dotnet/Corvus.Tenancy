// <copyright file="ITenantSqlConnectionFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Sql.Tenancy
{
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    using Corvus.Tenancy;

    /// <summary>
    /// A factory for a <see cref="SqlConnection"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You use this type to get an instance of an <see cref="SqlConnection"/> for a specific
    /// <see cref="ITenant"/>. It typically uses a KeyVault to get the SQL Server connection string for the tenant, and the
    /// configuration comes from the tenant via <see cref="SqlStorageTenantExtensions.AddSqlConfiguration(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{string, object}}, string, SqlConfiguration)"/>.
    /// </para>
    /// <para>
    /// To configure a simple single-tenanted solution, which can ultimately be extended to multitenancy, the easiest route is to configure a configuration-based account key
    /// provider and configuration for your repositories.
    /// </para>
    /// <para>
    /// First, add the Sql connection factory in your container configuration.
    /// </para>
    /// <code>
    /// serviceCollection.AddTenantSqlConnectionFactory(tenantSqlConnectionFactoryOptions);
    /// </code>
    /// <para>
    /// Now, whenever you want to obtain a sql connection for a tenant, you simply call <see cref="GetSqlConnectionForTenantAsync(ITenant, string)"/>, passing
    /// it the tenant and the database connection definition you want to use.
    /// </para>
    /// <para>
    /// <code>
    /// TenantSqlConnectionFactory factory;
    ///
    /// var repository = await factory.GetSqlConnectionForTenantAsync(tenantProvider.Root, new BlobStorageContainerDefinition("somecontainer"));
    /// </code>
    /// </para>
    /// <para>
    /// If you create connections in this way (rather than just newing them up) then your application can easily be multitented
    /// by ensuring that you always pass the Tenant through your stack, and just default to tenantProvider.Root at the top level.
    /// </para>
    /// <para>You are responsible of disposing of the pooled <see cref="SqlConnection"/> in the usual way.</para>
    /// </remarks>
    public interface ITenantSqlConnectionFactory
    {
        /// <summary>
        /// Get a <see cref="SqlConnection"/> for a tenant.
        /// </summary>
        /// <param name="tenant">The tenant for which to retrieve the container.</param>
        /// <param name="storageContextName">The name identifying the context to create.</param>
        /// <returns>The <see cref="SqlConnection"/> instance for the tenant.</returns>
        Task<SqlConnection> GetSqlConnectionForTenantAsync(ITenant tenant, string storageContextName);
    }
}