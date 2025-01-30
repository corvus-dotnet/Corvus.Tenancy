// <copyright file="GremlinContainerSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Threading.Tasks;

    using Corvus.Azure.GremlinExtensions.Tenancy;
    using Corvus.Tenancy.Specs.Bindings;

    using Gremlin.Net.Driver;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using Reqnroll;

    [Binding]
    public class GremlinContainerSteps
    {
        private readonly TenancyContainerScenarioBindings tenancyBindings;
        private readonly TenancyGremlinContainerBindings gremlinBindings;
        private readonly GremlinContainerDefinition containerDefinition;

        public GremlinContainerSteps(
            TenancyContainerScenarioBindings tenancyBindings,
            TenancyGremlinContainerBindings gremlinBindings)
        {
            this.tenancyBindings = tenancyBindings;
            this.gremlinBindings = gremlinBindings;

            string containerBase = Guid.NewGuid().ToString();
            this.containerDefinition = new GremlinContainerDefinition(
                "endjinspecssharedthroughput",
                $"{containerBase}tenancyspecs");
        }

        [Given("I have added Gremlin configuration to a tenant")]
        public void GivenIHaveAddedGremlinConfigurationToATenant()
        {
            var gremlinConfiguration = new GremlinConfiguration();
            TenancyContainerScenarioBindings.Configuration.Bind("TESTGREMLINCONFIGURATIONOPTIONS", gremlinConfiguration);
            gremlinConfiguration.DatabaseName = "endjinspecssharedthroughput";
            gremlinConfiguration.DisableTenantIdPrefix = true;
            this.tenancyBindings.RootTenant.UpdateProperties(values =>
                values.AddGremlinConfiguration(this.containerDefinition, gremlinConfiguration));
        }

        [Given("I have not added Gremlin configuration to a tenant")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Test framework needs this to be non-static")]
        public void GivenIHaveNotAddedGremlinConfigurationToATenant()
        {
            // Nothing do to - just here so we can state the prerequisite explicitly in the step.
        }

        [Then("I should be able to get the tenanted gremlin client")]
        public async Task ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            GremlinClient gremlinClient = await this.gremlinBindings.ContainerFactory.GetClientForTenantAsync(
                this.tenancyBindings.RootTenant,
                this.containerDefinition).ConfigureAwait(false);
            Assert.IsNotNull(gremlinClient);
            this.gremlinBindings.DisposeThisClientOnTestTeardown(gremlinClient);
        }

        [When("I remove the Gremlin configuration from the tenant")]
        public void WhenIRemoveTheGremlinConfigurationFromTheTenant()
        {
            this.tenancyBindings.RootTenant.UpdateProperties(
                propertiesToRemove: this.containerDefinition.RemoveGremlinConfiguration());
        }

        [Then("attempting to get the Gremlin configuration from the tenant should throw an ArgumentException")]
        public void ThenGettingTheGremlinConfigurationOnTheTenantThrowsArgumentException()
        {
            try
            {
                this.tenancyBindings.RootTenant.GetGremlinConfiguration(this.containerDefinition);
            }
            catch (ArgumentException)
            {
                return;
            }

            Assert.Fail("The expected exception was not thrown.");
        }
    }
}