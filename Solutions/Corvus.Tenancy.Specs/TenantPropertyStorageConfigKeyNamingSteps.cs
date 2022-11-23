// <copyright file="TenantPropertyStorageConfigKeyNamingSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs;

using Corvus.Storage.Azure.BlobStorage.Tenancy;
using Corvus.Storage.Azure.Cosmos.Tenancy;
using Corvus.Storage.Azure.TableStorage.Tenancy;
using Corvus.Storage.Sql.Tenancy;

using NUnit.Framework;

using TechTalk.SpecFlow;

[Binding]
public class TenantPropertyStorageConfigKeyNamingSteps
{
    private string? propertyKeyName;

    [When("I get the tenant property key for a logical blob container name of '([^']*)'")]
    public void WhenIGetTheTenantPropertyKeyForALogicalBlobContainerNameOf(string logicalContainerName)
    {
        this.propertyKeyName = LegacyV2BlobConfigurationKeyNaming.TenantPropertyKeyForLogicalContainer(logicalContainerName);
    }

    [When("I get the tenant property key for a logical table storage container name of '([^']*)'")]
    public void WhenIGetTheTenantPropertyKeyForALogicalTableStorageContainerNameOf(string logicalContainerName)
    {
        this.propertyKeyName = LegacyV2TableConfigurationKeyNaming.TenantPropertyKeyForLogicalContainer(logicalContainerName);
    }

    [When("I get the tenant property key for a logical Cosmos database name of '([^']*)' and logical container name of '([^']*)'")]
    public void WhenIGetTheTenantPropertyKeyForALogicalCosmosDatabaseNameOfAndLogicalContainerNameOf(
        string logicalDatabaseName, string logicalContainerName)
    {
        this.propertyKeyName = LegacyV2CosmosConfigurationKeyNaming.TenantPropertyKeyForLogicalDatabaseAndContainer(
            logicalDatabaseName, logicalContainerName);
    }

    [When("I get the tenant property key for a logical SQL database name of '([^']*)'")]
    public void WhenIGetTheTenantPropertyKeyForALogicalSQLDatabaseNameOf(string logicalDatabaseName)
    {
        this.propertyKeyName = LegacyV2SqlConfigurationKeyNaming.TenantPropertyKeyForLogicalDatabase(logicalDatabaseName);
    }

    [Then("the property key name is '([^']*)'")]
    public void ThenThePropertyKeyNameIs(string expectedPropertyKeyName)
    {
        Assert.AreEqual(expectedPropertyKeyName, this.propertyKeyName);
    }
}