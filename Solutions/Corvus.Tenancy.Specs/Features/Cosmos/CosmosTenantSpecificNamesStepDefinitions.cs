// <copyright file="CosmosTenantSpecificNamesStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Features.Cosmos;

using Corvus.Storage.Azure.Cosmos.Tenancy;
using Corvus.Tenancy.Specs.Bindings;

using TechTalk.SpecFlow;

[Binding]
public class CosmosTenantSpecificNamesStepDefinitions
{
    private readonly TenantedNameBindings tenantedNameBindings;

    public CosmosTenantSpecificNamesStepDefinitions(
        TenantedNameBindings tenantedNameBindings)
    {
        this.tenantedNameBindings = tenantedNameBindings;
    }

    [When("I get a Cosmos database name for tenant '([^']*)' with a logical name of '([^']*)' and label the result '([^']*)'")]
    public void WhenIGetACosmosDatabaseNameForTenantWithALogicalNameOfAndLabelTheResult(
        string tenantLabel, string logicalDatabaseName, string resultLabel)
    {
        ITenant tenant = this.tenantedNameBindings.Tenants[tenantLabel];
        this.tenantedNameBindings.AddTenantedContainerName(
            resultLabel,
            CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenant, logicalDatabaseName));
    }

    [When("I get a Cosmos container name for tenant '([^']*)' with a logical name of '([^']*)' and label the result '([^']*)'")]
    public void WhenIGetACosmosContainerNameForTenantWithALogicalNameOfAndLabelTheResult(
        string tenantLabel, string logicalContainerName, string resultLabel)
    {
        ITenant tenant = this.tenantedNameBindings.Tenants[tenantLabel];
        this.tenantedNameBindings.AddTenantedContainerName(
            resultLabel,
            CosmosTenantedContainerNaming.GetTenantSpecificContainerNameFor(tenant, logicalContainerName));
    }
}