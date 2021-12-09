// <copyright file="SqlConnectionSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    using Corvus.Sql.Tenancy;
    using Corvus.Tenancy.Specs.Bindings;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using TechTalk.SpecFlow;

    [Binding]
    public class SqlConnectionSteps
    {
        private readonly TenancyContainerScenarioBindings tenancyBindings;
        private readonly TenancySqlBindings sqlBindings;
        private readonly SqlConnectionDefinition sqlConnectionDefinition;
        private SqlConfiguration? configuration;
        private bool validationResult;

        public SqlConnectionSteps(
            TenancyContainerScenarioBindings tenancyBindings,
            TenancySqlBindings sqlBindings)
        {
            this.tenancyBindings = tenancyBindings;
            this.sqlBindings = sqlBindings;
            string databaseName = Guid.NewGuid().ToString();
            this.sqlConnectionDefinition = new SqlConnectionDefinition($"{databaseName}tenancyspecs");
        }

        [Then("I should be able to get the tenanted SqlConnection")]
        public async Task ThenIShouldBeAbleToGetTheTenantedSqlConnection()
        {
            var sqlConfiguration = new SqlConfiguration();
            TenancyContainerScenarioBindings.Configuration.Bind("TESTSQLCONFIGURATIONOPTIONS", sqlConfiguration);

            // Fall back on a local database
            if (string.IsNullOrEmpty(sqlConfiguration.ConnectionString) &&
                string.IsNullOrEmpty(sqlConfiguration.ConnectionStringSecretName))
            {
                sqlConfiguration.IsLocalDatabase = true;
                sqlConfiguration.ConnectionString = "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;MultipleActiveResultSets=true";
                sqlConfiguration.DisableTenantIdPrefix = true;
            }

            this.tenancyBindings.RootTenant.UpdateProperties(values =>
                values.AddSqlConfiguration(this.sqlConnectionDefinition, sqlConfiguration));

            using SqlConnection sqlConnection = await this.sqlBindings.ConnectionFactory.GetSqlConnectionForTenantAsync(
                this.tenancyBindings.RootTenant,
                this.sqlConnectionDefinition).ConfigureAwait(false);

            Assert.IsNotNull(sqlConnection);
        }

        [Given("a SqlConfiguration")]
        public void GivenASqlConfiguration(Table table)
        {
            Assert.AreEqual(1, table.Rows.Count);
            this.configuration = new SqlConfiguration
            {
                ConnectionString = table.Rows[0]["ConnectionString"],
                ConnectionStringSecretName = table.Rows[0]["ConnectionStringSecretName"],
                KeyVaultName = table.Rows[0]["KeyVaultName"],
            };
        }

        [When("I validate the configuration")]
        public void WhenIValidateTheConfiguration()
        {
            this.validationResult = this.configuration!.Validate();
        }

        [Then("the result should be valid")]
        public void ThenTheResultShouldBeValid()
        {
            Assert.IsTrue(this.validationResult);
        }

        [Then("the result should be invalid")]
        public void ThenTheResultShouldBeInvalid()
        {
            Assert.IsFalse(this.validationResult);
        }

        [When("I remove the Sql configuration from the tenant")]
        public void WhenIRemoveTheSqlConfigurationFromTheTenant()
        {
            this.tenancyBindings.RootTenant.UpdateProperties(
                propertiesToRemove: this.sqlConnectionDefinition.RemoveSqlConfiguration());
        }

        [Then("attempting to get the Sql configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheSqlConfigurationOnTheTenantThrowsArgumentException()
        {
            try
            {
                this.tenancyBindings.RootTenant.GetSqlConfiguration(this.sqlConnectionDefinition);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}