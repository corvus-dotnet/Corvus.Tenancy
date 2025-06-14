// <copyright file="CosmosTenantSpecificNamesSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Features.Cosmos;

using Corvus.Storage.Azure.Cosmos.Tenancy;
using Corvus.Tenancy.Specs.Bindings;

using Reqnroll;

[Binding]
public class CosmosTenantSpecificNamesSteps
{
    private readonly TenantedNameBindings tenantedNameBindings;

    public CosmosTenantSpecificNamesSteps(
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

    [When("I get a Cosmos database name for tenantId '([^']*)' with a logical name of '([^']*)' and label the result '([^']*)'")]
    public void WhenIGetACosmosDatabaseNameForTenantIdWithALogicalNameOfAndLabelTheResult(
        string tenantId, string logicalDatabaseName, string resultLabel)
    {
        this.tenantedNameBindings.AddTenantedContainerName(
            resultLabel,
            CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenantId, logicalDatabaseName));
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

    [When("I get a Cosmos container name for tenantId '([^']*)' with a logical name of '([^']*)' and label the result '([^']*)'")]
    public void WhenIGetACosmosContainerNameForTenantIdWithALogicalNameOfAndLabelTheResult(
        string tenantId, string logicalContainerName, string resultLabel)
    {
        this.tenantedNameBindings.AddTenantedContainerName(
            resultLabel,
            CosmosTenantedContainerNaming.GetTenantSpecificContainerNameFor(tenantId, logicalContainerName));
    }
}