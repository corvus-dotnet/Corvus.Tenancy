﻿// <copyright file="LegacyCloudTableSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Threading.Tasks;

    using Corvus.Azure.Storage.Tenancy;
    using Corvus.Azure.Storage.Tenancy.Internal;
    using Corvus.Tenancy.Specs.Bindings;

    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Configuration;

    using NUnit.Framework;

    using Reqnroll;

    [Binding]
    public class LegacyCloudTableSteps
    {
        private readonly TenancyContainerScenarioBindings tenancyBindings;
        private readonly TenancyCloudTableBindings tableBindings;
        private CloudTable? cloudTable;
        private TableStorageTableDefinition? tableStorageTableDefinition;

        public LegacyCloudTableSteps(TenancyContainerScenarioBindings tenancyBindings, TenancyCloudTableBindings tableBindings)
        {
            this.tenancyBindings = tenancyBindings;
            this.tableBindings = tableBindings;
        }

        private CloudTable CloudTable => this.cloudTable ?? throw new InvalidOperationException("Cloud table not created");

        private TableStorageTableDefinition TableStorageTableDefinition => this.tableStorageTableDefinition ?? throw new InvalidOperationException("Definition not created yet");

        [Given("I have added table storage configuration to the current tenant")]
        public void GivenIHaveAddedTableStorageConfigurationToTheCurrentTenant(Table table)
        {
            string containerBase = Guid.NewGuid().ToString();

            this.tableStorageTableDefinition = new TableStorageTableDefinition($"{containerBase}tenancyspecs");

            var tableStorageConfiguration = new TableStorageConfiguration();
            TenancyContainerScenarioBindings.Configuration.Bind("TESTTABLESTORAGECONFIGURATIONOPTIONS", tableStorageConfiguration);

            string overriddenTableName = table.Rows[0]["TableName"];
            if (!string.IsNullOrEmpty(overriddenTableName))
            {
                tableStorageConfiguration.TableName = overriddenTableName;
            }

            tableStorageConfiguration.DisableTenantIdPrefix = bool.Parse(table.Rows[0]["DisableTenantIdPrefix"]);

            this.tenancyBindings.RootTenant.UpdateProperties(values =>
                values.AddTableStorageConfiguration(this.TableStorageTableDefinition, tableStorageConfiguration));
        }

        [Then("I should be able to get the tenanted cloud table")]
        public async Task ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            this.cloudTable = await this.tableBindings.ConnectionFactory.GetTableForTenantAsync(
                this.tenancyBindings.RootTenant,
                this.TableStorageTableDefinition).ConfigureAwait(false);

            Assert.IsNotNull(this.cloudTable);

            this.tableBindings.RemoveThisTableOnTestTeardown(this.CloudTable);
        }

        [Then("the tenanted cloud table should be named using a hash of the tenant Id and the name specified in the table definition")]
        public void ThenTheTenantedCloudTableShouldBeNamedUsingAHashOfTheTenantIdAndTheNameSpecifiedInTheTableDefinition()
        {
            string expectedNamePlain = string.Concat(RootTenant.RootTenantId, "-", this.TableStorageTableDefinition.TableName);
            string expectedName = AzureStorageNameHelper.HashAndEncodeTableName(expectedNamePlain);

            Assert.AreEqual(expectedName, this.CloudTable.Name);
        }

        [Then("the tenanted cloud table should be named using a hash of the tenant Id and the name specified in the table configuration")]
        public void ThenTheTenantedCloudTableShouldBeNamedUsingAHashOfTheTenantIdAndTheNameSpecifiedInTheTableConfiguration()
        {
            TableStorageConfiguration tableStorageConfiguration = this.tenancyBindings.RootTenant.GetTableStorageConfiguration(this.TableStorageTableDefinition);

            string expectedNamePlain = string.Concat(RootTenant.RootTenantId, "-", tableStorageConfiguration.TableName);
            string expectedName = AzureStorageNameHelper.HashAndEncodeTableName(expectedNamePlain);

            Assert.AreEqual(expectedName, this.CloudTable.Name);
        }

        [Then("the tenanted cloud table should be named using a hash of the name specified in the blob configuration")]
        public void ThenTheTenantedCloudTableShouldBeNamedUsingAHashOfTheNameSpecifiedInTheBlobConfiguration()
        {
            TableStorageConfiguration tableStorageConfiguration = this.tenancyBindings.RootTenant.GetTableStorageConfiguration(this.TableStorageTableDefinition);

            string expectedNamePlain = tableStorageConfiguration.TableName!;
            string expectedName = AzureStorageNameHelper.HashAndEncodeTableName(expectedNamePlain);

            Assert.AreEqual(expectedName, this.CloudTable.Name);
        }

        [When("I remove the legacy table storage configuration from the tenant")]
        public void WhenIRemoveTheTableStorageConfigurationFromTheTenant()
        {
            this.tenancyBindings.RootTenant.UpdateProperties(propertiesToRemove: this.TableStorageTableDefinition.RemoveTableStorageConfiguration());
        }

        [Then("attempting to get the table storage configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheTableStorageConfigurationOnTheTenantThrowsArgumentException()
        {
            try
            {
                this.tenancyBindings.RootTenant.GetTableStorageConfiguration(this.TableStorageTableDefinition);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}