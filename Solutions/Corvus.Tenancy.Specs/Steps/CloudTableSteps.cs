namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Threading.Tasks;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.Tenancy.Specs.Bindings;
    using Corvus.Testing.SpecFlow;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class CloudTableSteps
    {
        private readonly FeatureContext featureContext;
        private readonly IServiceProvider serviceProvider;

        private string? tableStorageContextName;

        public CloudTableSteps(FeatureContext featureContext)
        {
            this.featureContext = featureContext;
            this.serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
        }

        [Given("I have added table storage configuration to the current tenant with a table name of '(.*)'")]
        public void GivenIHaveAddedTableStorageConfigurationToTheCurrentTenantWithATableNameOf(string tableName)
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            IConfigurationRoot config = this.serviceProvider.GetRequiredService<IConfigurationRoot>();

            this.tableStorageContextName = $"tenancyspecs{Guid.NewGuid()}";

            var tableStorageConfiguration = new TableStorageConfiguration
            {
                TableName = tableName,
            };
            config.Bind("TESTTABLESTORAGECONFIGURATIONOPTIONS", tableStorageConfiguration);

            tenantProvider.Root.UpdateProperties(values => values.AddTableStorageConfiguration(this.tableStorageContextName, tableStorageConfiguration));
        }

        [Then("I should be able to get the tenanted cloud table")]
        public async Task ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            ITenantCloudTableFactory factory = this.serviceProvider.GetRequiredService<ITenantCloudTableFactory>();

            CloudTable cloudTable = await factory.GetContextForTenantAsync(
                tenantProvider.Root,
                this.tableStorageContextName!).ConfigureAwait(false);

            Assert.IsNotNull(cloudTable);

            // Add to feature context so it will be torn down after the test.
            this.featureContext.Set(cloudTable, TenancyCloudTableBindings.TenancySpecsContainer);
        }

        [When("I remove the table storage configuration from the tenant")]
        public void WhenIRemoveTheTableStorageConfigurationFromTheTenant()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            tenantProvider.Root.UpdateProperties(
                propertiesToRemove: TableStorageTenantExtensions.RemoveTableStorageConfiguration(this.tableStorageContextName!));
        }

        [Then("attempting to get the table storage configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheTableStorageConfigurationOnTheTenantThrowsArgumentException()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();

            try
            {
                tenantProvider.Root.GetTableStorageConfiguration(this.tableStorageContextName!);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}