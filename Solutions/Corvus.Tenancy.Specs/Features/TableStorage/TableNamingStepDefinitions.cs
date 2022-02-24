// <copyright file="TableNamingStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Features.TableStorage;

using Corvus.Storage.Azure.TableStorage.Tenancy;
using Corvus.Tenancy.Specs.Bindings;

using TechTalk.SpecFlow;

[Binding]
public sealed class TableNamingStepDefinitions
{
    private readonly TenantedNameBindings tenantedNameBindings;

    public TableNamingStepDefinitions(
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
}