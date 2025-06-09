// <copyright file="TableNamingSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Features.TableStorage;

using Corvus.Storage.Azure.TableStorage.Tenancy;
using Corvus.Tenancy.Specs.Bindings;

using Reqnroll;

[Binding]
public sealed class TableNamingSteps
{
    private readonly TenantedNameBindings tenantedNameBindings;

    public TableNamingSteps(
        TenantedNameBindings tenantedNameBindings)
    {
        this.tenantedNameBindings = tenantedNameBindings;
    }

    [When("I get an Azure table name for tenant '([^']*)' with a logical name of '([^']*)' and label the result '([^']*)'")]
    public void WhenIGetAnAzureTableNameForTenantWithALogicalNameOfAndLabelTheResult(
        string tenantLabel, string logicalTableName, string resultLabel)
    {
        ITenant tenant = this.tenantedNameBindings.Tenants[tenantLabel];
        this.tenantedNameBindings.AddTenantedContainerName(
            resultLabel,
            AzureTablesTenantedNaming.GetHashedTenantedTableNameFor(tenant, logicalTableName));
    }

    [When("I get an Azure table name for tenantId '([^']*)' with a logical name of '([^']*)' and label the result '([^']*)'")]
    public void WhenIGetAnAzureTableNameForTenantIdWithALogicalNameOfAndLabelTheResult(
        string tenantId, string logicalTableName, string resultLabel)
    {
        this.tenantedNameBindings.AddTenantedContainerName(
            resultLabel,
            AzureTablesTenantedNaming.GetHashedTenantedTableNameFor(tenantId, logicalTableName));
    }
}