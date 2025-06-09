// <copyright file="SqlConnectionSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Features.SqlDatabase;

using System;
using System.Threading.Tasks;

using Corvus.Storage.Sql;
using Corvus.Storage.Sql.Tenancy;
using Corvus.Tenancy.Specs.Bindings;

using Microsoft.Data.SqlClient;

using NUnit.Framework;

using Reqnroll;

[Binding]
public class SqlConnectionSteps
{
    private readonly TenancyContainerScenarioBindings tenancyBindings;
    private readonly TenancySqlBindings sqlBindings;
    private SqlDatabaseConfiguration? configuration;
    private SqlConnection? sqlConnection;

    public SqlConnectionSteps(
        TenancyContainerScenarioBindings tenancyBindings,
        TenancySqlBindings sqlBindings)
    {
        this.tenancyBindings = tenancyBindings;
        this.sqlBindings = sqlBindings;
    }

    [Given("I have added a SqlDatabaseConfiguration with the connection string '([^']*)' to a tenant as '([^']*)'")]
    public void GivenIHaveAddedASqlDatabaseConfigurationWithTheConnectionStringToATenantAs(
        string connectionString, string configurationKey)
    {
        this.configuration = new SqlDatabaseConfiguration
        {
            ConnectionStringPlainText = connectionString,
        };
        this.tenancyBindings.RootTenant.UpdateProperties(values =>
            values.AddSqlDatabaseConfiguration(configurationKey, this.configuration));
    }

    [When("I get the tenanted SqlConnection as '([^']*)'")]
    public async Task WhenIGetTheTenantedSqlConnection(string configurationKey)
    {
        this.sqlConnection = await this.sqlBindings.ConnectionSource.GetSqlConnectionForTenantAsync(
            this.tenancyBindings.RootTenant,
            configurationKey).ConfigureAwait(false);

        Assert.IsNotNull(this.sqlConnection);
    }

    [Then("the connection string should be '([^']*)'")]
    public void ThenTheConnectionStringShouldBe(string connectionString)
    {
        Assert.AreEqual(connectionString, this.sqlConnection!.ConnectionString);
    }

    [When("I remove the Sql configuration '([^']*)' from the tenant")]
    public void WhenIRemoveTheSqlConfigurationFromTheTenant(string configurationKey)
    {
        this.tenancyBindings.RootTenant.UpdateProperties(
            propertiesToRemove: [configurationKey]);
    }

    [Then("attempting to get the Sql configuration '([^']*)' from the tenant throws an InvalidOperationException")]
    public void ThenAttemptingToGetTheSqlConfigurationFromTheTenantThrowsAnInvalidOperationException(string configurationKey)
    {
        try
        {
            this.tenancyBindings.RootTenant.GetSqlDatabaseConfiguration(configurationKey);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        Assert.Fail("The expected exception was not thrown.");
    }
}