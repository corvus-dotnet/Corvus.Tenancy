namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Sql.Tenancy;
    using Microsoft.Extensions.Configuration;
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
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantSqlConnectionFactory factory = serviceProvider.GetRequiredService<ITenantSqlConnectionFactory>();
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            IConfigurationRoot config = serviceProvider.GetRequiredService<IConfigurationRoot>();

            var sqlConnectionDefinition = new SqlConnectionDefinition($"{databaseName}tenancyspecs");

            var sqlConfiguration = new SqlConfiguration();
            config.Bind("TESTSQLCONFIGURATIONOPTIONS", sqlConfiguration);

            // Fall back on a local database
            if (string.IsNullOrEmpty(sqlConfiguration.ConnectionString) &&
                string.IsNullOrEmpty(sqlConfiguration.ConnectionStringSecretName))
            {
                sqlConfiguration.IsLocalDatabase = true;
                sqlConfiguration.ConnectionString = "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;MultipleActiveResultSets=true";
                sqlConfiguration.DisableTenantIdPrefix = true;
            }

            tenantProvider.Root.UpdateProperties(values => values.AddSqlConfiguration(sqlConnectionDefinition, sqlConfiguration));

            using SqlConnection sqlConnection = await factory.GetSqlConnectionForTenantAsync(
                tenantProvider.Root,
                sqlConnectionDefinition).ConfigureAwait(false);

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
