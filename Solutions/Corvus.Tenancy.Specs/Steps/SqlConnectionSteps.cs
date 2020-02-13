namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Sql.Tenancy;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class SqlConnectionSteps
    {
        private readonly FeatureContext featureContext;
        private readonly ScenarioContext scenarioContext;

        public SqlConnectionSteps(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            this.featureContext = featureContext;
            this.scenarioContext = scenarioContext;
        }

        [Then("I should be able to get the tenanted SqlConnection")]
        public async Task ThenIShouldBeAbleToGetTheTenantedSqlConnection()
        {
            string databaseName = Guid.NewGuid().ToString();
            ITenantSqlConnectionFactory factory = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantSqlConnectionFactory>();
            ITenantProvider tenantProvider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();

            using SqlConnection sqlConnection = await factory.GetSqlConnectionForTenantAsync(
                tenantProvider.Root,
                new SqlConnectionDefinition($"{databaseName}tenancyspecs")).ConfigureAwait(false);

            Assert.IsNotNull(sqlConnection);
        }

        [Given("a SqlConfiguration")]
        public void GivenASqlConfiguration(Table table)
        {
            Assert.AreEqual(1, table.Rows.Count);
            this.scenarioContext.Set(new SqlConfiguration { ConnectionString = table.Rows[0]["ConnectionString"], ConnectionStringSecretName = table.Rows[0]["ConnectionStringSecretName"], KeyVaultName = table.Rows[0]["KeyVaultName"] });
        }

        [When("I validate the configuration")]
        public void WhenIValidateTheConfiguration()
        {
            this.scenarioContext.Set(this.scenarioContext.Get<SqlConfiguration>().Validate(), "Result");
        }

        [Then("the result should be valid")]
        public void ThenTheResultShouldBeValid()
        {
            Assert.IsTrue(this.scenarioContext.Get<bool>("Result"));
        }

        [Then("the result should be invalid")]
        public void ThenTheResultShouldBeInvalid()
        {
            Assert.IsFalse(this.scenarioContext.Get<bool>("Result"));
        }
    }
}
