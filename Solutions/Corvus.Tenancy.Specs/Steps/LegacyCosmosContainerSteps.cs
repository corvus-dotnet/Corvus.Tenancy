// <copyright file="LegacyCosmosContainerSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Threading.Tasks;

    using Corvus.Azure.Cosmos.Tenancy;
    using Corvus.Tenancy.Specs.Bindings;
    using Corvus.Testing.SpecFlow;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class LegacyCosmosContainerSteps
    {
        private readonly ScenarioContext scenarioContext;
        private readonly TenancyContainerScenarioBindings tenancyBindings;
        private readonly LegacyTenancyCosmosContainerBindings cosmosBindings;
        private readonly CosmosContainerDefinition containerDefinition;

        public LegacyCosmosContainerSteps(
            ScenarioContext featureContext,
            TenancyContainerScenarioBindings tenancyBindings,
            LegacyTenancyCosmosContainerBindings cosmosBindings)
        {
            this.scenarioContext = featureContext;
            this.tenancyBindings = tenancyBindings;
            this.cosmosBindings = cosmosBindings;

            string containerBase = Guid.NewGuid().ToString();

            this.containerDefinition = new CosmosContainerDefinition(
                "endjinspecssharedthroughput",
                $"{containerBase}tenancyspecs",
                "/partitionKey",
                databaseThroughput: 400);
        }

        [Given("I have added legacy Cosmos configuration to a tenant")]
        public void GivenIHaveAddedLegacyCosmosConfigurationToATenant()
        {
            CosmosConfiguration cosmosConfiguration = new ();
            TenancyContainerScenarioBindings.Configuration.Bind("TESTLEGACYCOSMOSCONFIGURATIONOPTIONS", cosmosConfiguration);
            cosmosConfiguration.DatabaseName = "endjinspecssharedthroughput";
            cosmosConfiguration.DisableTenantIdPrefix = true;
            this.tenancyBindings.RootTenant.UpdateProperties(values => values.AddCosmosConfiguration(this.containerDefinition, cosmosConfiguration));
        }

        [Then("I should be able to get the tenanted cosmos container from the legacy API")]
        public async Task ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            // Note that this sets up a resource in Azure and will incur cost. It's important we ensure the corresponding
            // tear down operation is always run, or verify manually after a test run.
            Container cosmosContainer = await this.cosmosBindings.TenantCosmosContainerFactory.GetContainerForTenantAsync(
                this.tenancyBindings.RootTenant,
                this.containerDefinition).ConfigureAwait(false);

            Assert.IsNotNull(cosmosContainer);
            this.cosmosBindings.RemoveThisContainerOnTestTeardown(cosmosContainer);
        }

        [When("I remove the legacy Cosmos configuration from the tenant")]
        public void WhenIRemoveTheCosmosConfigurationFromTheTenant()
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(this.scenarioContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            CosmosContainerDefinition definition = this.containerDefinition;
            tenantProvider.Root.UpdateProperties(
                propertiesToRemove: definition.RemoveCosmosConfiguration());
        }

        [Then("attempting to get the legacy Cosmos configuration from the tenant throws an ArgumentException")]
        public void ThenGettingTheCosmosConfigurationOnTheTenantThrowsArgumentException()
        {
            try
            {
                this.tenancyBindings.RootTenant.GetCosmosConfiguration(this.containerDefinition);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}