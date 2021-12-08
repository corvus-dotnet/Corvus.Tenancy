// <copyright file="CosmosContainerSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Threading.Tasks;

    using Corvus.Storage.Azure.Cosmos;
    using Corvus.Storage.Azure.Cosmos.Tenancy;
    using Corvus.Tenancy.Specs.Bindings;

    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;

    using NUnit.Framework;

    using TechTalk.SpecFlow;

    [Binding]
    public class CosmosContainerSteps
    {
        private const string ConfigurationKey = "TestCosmos";
        private readonly ScenarioContext scenarioContext;
        private readonly TenancyContainerScenarioBindings tenancyBindings;
        private readonly TenancyCosmosContainerBindings cosmosBindings;
        private readonly string containerName;
        private CosmosContainerConfiguration? cosmosConfiguration;
        private Container? cosmosContainer;

        public CosmosContainerSteps(
            ScenarioContext featureContext,
            TenancyContainerScenarioBindings tenancyBindings,
            TenancyCosmosContainerBindings cosmosBindings)
        {
            this.scenarioContext = featureContext;
            this.tenancyBindings = tenancyBindings;
            this.cosmosBindings = cosmosBindings;

            this.containerName = Guid.NewGuid().ToString();
        }

        [Given("I have added Cosmos configuration to a tenant")]
        public void GivenIHaveAddedCosmosConfigurationToATenant()
        {
            this.cosmosConfiguration = new ();
            this.tenancyBindings.Configuration.Bind("TESTCOSMOSCONFIGURATIONOPTIONS", this.cosmosConfiguration);
            this.cosmosConfiguration.Database = "endjinspecssharedthroughput";
            this.tenancyBindings.RootTenant.UpdateProperties(values =>
                values.AddCosmosConfiguration(ConfigurationKey, this.cosmosConfiguration));
        }

        [Given("I have not added cosmos configuration to a tenant")]
        public void GivenIHaveNotAddedCosmosConfigurationToATenant()
        {
            // Nothing to do here since this is the default - this step exists just so we
            // can state the requirement explicitly in the feature file.
        }

        [Then("I should be able to get the tenanted cosmos container")]
        public async Task ThenIShouldBeAbleToGetTheTenantedCosmosContainer()
        {
            this.cosmosContainer = await this.cosmosBindings.ContainerSource.GetContainerForTenantAsync(
                this.tenancyBindings.RootTenant,
                ConfigurationKey,
                this.containerName).ConfigureAwait(false);

            Assert.IsNotNull(this.cosmosContainer);

            // Since we now don't auto-create containers, we don't need to clean up afterwards.
        }

        [Then("the tenanted cosmos container database should match the configuration")]
        public void ThenTheTenantedCosmosContainerDatabaseShouldMatchTheConfiguration()
        {
            Assert.AreEqual(this.cosmosConfiguration!.Database, this.cosmosContainer?.Database.Id);
        }

        [Then("the tenanted cosmos container name should match the configuration")]
        public void ThenTheTenantedCosmosContainerNameShouldMatchTheConfiguration()
        {
            Assert.AreEqual(this.containerName, this.cosmosContainer?.Id);
        }

        [Then("attempting to get the Cosmos configuration from the tenant throws an ArgumentException")]
        public void ThenAttemptingToGetTheCosmosConfigurationFromTheTenantThrowsAnArgumentException()
        {
            try
            {
                this.tenancyBindings.RootTenant.GetCosmosConfiguration(ConfigurationKey);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}
