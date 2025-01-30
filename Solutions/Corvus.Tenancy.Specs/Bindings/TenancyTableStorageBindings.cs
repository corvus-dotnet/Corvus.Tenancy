// <copyright file="TenancyTableStorageBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings;

using Corvus.Testing.ReqnRoll;

using Microsoft.Extensions.DependencyInjection;

using Reqnroll;

[Binding]
public class TenancyTableStorageBindings
{
    private readonly ScenarioContext scenarioContext;

    public TenancyTableStorageBindings(
        ScenarioContext scenarioContext)
    {
        this.scenarioContext = scenarioContext;
    }

    /// <summary>
    /// Initializes the container before each scenario runs.
    /// </summary>
    [BeforeScenario("@tableStorageLegacyMigration", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection)]
    public void InitializeContainer()
    {
        ContainerBindings.ConfigureServices(
            this.scenarioContext,
            serviceCollection =>
            {
                serviceCollection.AddTenantAzureTableFactory();
                serviceCollection.AddAzureTableV2ToV3Transition();
            });
    }
}