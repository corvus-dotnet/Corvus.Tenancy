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

        public SqlConnectionSteps(FeatureContext featureContext)
        {
            this.featureContext = featureContext;
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
    }
}
