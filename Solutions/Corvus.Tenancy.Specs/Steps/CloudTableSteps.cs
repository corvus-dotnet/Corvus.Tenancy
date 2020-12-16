namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Threading.Tasks;
    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Tenancy.Specs.Bindings;
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

        public CloudTableSteps(FeatureContext featureContext)
        {
            this.featureContext = featureContext;
            this.serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
        }

        [Given("I have added table storage configuration to the current tenant")]
        public void GivenIHaveAddedTableStorageConfigurationToTheCurrentTenant(Table table)
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            IConfigurationRoot config = this.serviceProvider.GetRequiredService<IConfigurationRoot>();

            string containerBase = Guid.NewGuid().ToString();

            var tableStorageTableDefinition = new TableStorageTableDefinition($"{containerBase}tenancyspecs");
            this.featureContext.Set(tableStorageTableDefinition);

            var tableStorageConfiguration = new TableStorageConfiguration();
            config.Bind("TESTTABLESTORAGECONFIGURATIONOPTIONS", tableStorageConfiguration);

            string overriddenTableName = table.Rows[0]["TableName"];
            if (!string.IsNullOrEmpty(overriddenTableName))
            {
                tableStorageConfiguration.TableName = overriddenTableName;
            }

            tableStorageConfiguration.DisableTenantIdPrefix = bool.Parse(table.Rows[0]["DisableTenantIdPrefix"]);

            tenantProvider.Root.UpdateProperties(values => values.AddTableStorageConfiguration(tableStorageTableDefinition, tableStorageConfiguration));
        }

        [Then("I should be able to get the tenanted cloud table")]
        public async Task ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            ITenantCloudTableFactory factory = this.serviceProvider.GetRequiredService<ITenantCloudTableFactory>();

            TableStorageTableDefinition tableStorageTableDefinition = this.featureContext.Get<TableStorageTableDefinition>();

            CloudTable cloudTable = await factory.GetTableForTenantAsync(
                tenantProvider.Root,
                tableStorageTableDefinition).ConfigureAwait(false);

            Assert.IsNotNull(cloudTable);

            // Add to feature context so it will be torn down after the test.
            this.featureContext.Set(cloudTable, TenancyCloudTableBindings.TenancySpecsContainer);
        }

        [Then("the tenanted cloud table should be named using a hash of the tenant Id and the name specified in the table definition")]
        public void ThenTheTenantedCloudTableShouldBeNamedUsingAHashOfTheTenantIdAndTheNameSpecifiedInTheTableDefinition()
        {
            TableStorageTableDefinition tableDefinition = this.featureContext.Get<TableStorageTableDefinition>();
            string expectedNamePlain = string.Concat(RootTenant.RootTenantId, "-", tableDefinition.TableName);
            string expectedName = AzureStorageNameHelper.HashAndEncodeTableName(expectedNamePlain);

            CloudTable table = this.featureContext.Get<CloudTable>(TenancyCloudTableBindings.TenancySpecsContainer);

            Assert.AreEqual(expectedName, table.Name);
        }

        [Then(@"the tenanted cloud table should be named using a hash of the tenant Id and the name specified in the table configuration")]
        public void ThenTheTenantedCloudTableShouldBeNamedUsingAHashOfTheTenantIdAndTheNameSpecifiedInTheTableConfiguration()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            TableStorageTableDefinition tableDefinition = this.featureContext.Get<TableStorageTableDefinition>();
            TableStorageConfiguration tableStorageConfiguration = tenantProvider.Root.GetTableStorageConfiguration(tableDefinition);

            string expectedNamePlain = string.Concat(RootTenant.RootTenantId, "-", tableStorageConfiguration.TableName);
            string expectedName = AzureStorageNameHelper.HashAndEncodeTableName(expectedNamePlain);

            CloudTable table = this.featureContext.Get<CloudTable>(TenancyCloudTableBindings.TenancySpecsContainer);

            Assert.AreEqual(expectedName, table.Name);
        }

        [Then("the tenanted cloud table should be named using a hash of the name specified in the blob configuration")]
        public void ThenTheTenantedCloudTableShouldBeNamedUsingAHashOfTheNameSpecifiedInTheBlobConfiguration()
        {
            ITenantProvider tenantProvider = this.serviceProvider.GetRequiredService<ITenantProvider>();
            TableStorageTableDefinition tableDefinition = this.featureContext.Get<TableStorageTableDefinition>();
            TableStorageConfiguration tableStorageConfiguration = tenantProvider.Root.GetTableStorageConfiguration(tableDefinition);

            string expectedNamePlain = tableStorageConfiguration.TableName!;
            string expectedName = AzureStorageNameHelper.HashAndEncodeTableName(expectedNamePlain);

            CloudTable table = this.featureContext.Get<CloudTable>(TenancyCloudTableBindings.TenancySpecsContainer);

            Assert.AreEqual(expectedName, table.Name);
        }

        [When("I remove the table storage configuration from the tenant")]
        public void WhenIRemoveTheTableStorageConfigurationFromTheTenant()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            TableStorageTableDefinition definition = this.featureContext.Get<TableStorageTableDefinition>();
            tenantProvider.Root.UpdateProperties(
                propertiesToRemove: definition.RemoveTableStorageConfiguration());
        }

        [Then("attempting to get the table storage configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheTableStorageConfigurationOnTheTenantThrowsArgumentException()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            TableStorageTableDefinition definition = this.featureContext.Get<TableStorageTableDefinition>();

            try
            {
                tenantProvider.Root.GetTableStorageConfiguration(definition);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}
