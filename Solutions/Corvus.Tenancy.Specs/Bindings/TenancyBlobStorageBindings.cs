// <copyright file="TenancyBlobStorageBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using Corvus.Testing.ReqnRoll;

    using Microsoft.Extensions.DependencyInjection;

    using Reqnroll;

    [Binding]
    public class TenancyBlobStorageBindings
    {
        private readonly ScenarioContext scenarioContext;

        public TenancyBlobStorageBindings(
            ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        /// <summary>
        /// Initializes the container before each scenario runs.
        /// </summary>
        [BeforeScenario("@blobStorageLegacyMigration", Order = ContainerBeforeScenarioOrder.PopulateServiceCollection)]
        public void InitializeContainer()
        {
            ContainerBindings.ConfigureServices(
                   this.scenarioContext,
                   serviceCollection =>
                   {
                       serviceCollection.AddTenantBlobContainerFactory();
                       serviceCollection.AddBlobContainerV2ToV3Transition();
                   });
        }
    }
}