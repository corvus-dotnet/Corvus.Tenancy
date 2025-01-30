// <copyright file="TenantStorageSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.Tenancy.Exceptions;
    using Corvus.Testing.ReqnRoll;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using Reqnroll;

    [Binding]
    public class TenantStorageSteps
    {
        private readonly ScenarioContext scenarioContext;
        private readonly ITenantStore store;

        public TenantStorageSteps(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
            this.store = ContainerBindings.GetServiceProvider(featureContext).GetRequiredService<ITenantStore>();
        }

        [Given("I get the tenant id of the tenant called \"(.*)\" and call it \"(.*)\"")]
        [When("I get the tenant id of the tenant called \"(.*)\" and call it \"(.*)\"")]
        public void WhenIGetTheTenantIdOfTheTenantCalledAndCallIt(string tenantName, string tenantIdName)
        {
            ITenant tenant = this.scenarioContext.Get<ITenant>(tenantName);
            this.scenarioContext.Set(tenant.Id, tenantIdName);
        }

        [Given("I get the tenant with the id called \"(.*)\" and call it \"(.*)\"")]
        [When("I get the tenant with the id called \"(.*)\" and call it \"(.*)\"")]
        public async Task WhenIGetTheTenantWithTheIdCalled(string tenantIdName, string tenantName)
        {
            ITenant? tenant = await this.store.GetTenantAsync(this.scenarioContext.Get<string>(tenantIdName)).ConfigureAwait(false);
            this.scenarioContext.Set(tenant, tenantName);
        }

        [Then("the tenant called \"(.*)\" should have the same ID as the tenant called \"(.*)\"")]
        public void ThenTheTenantCalledShouldHaveTheSameIDAsTheTenantCalled(string firstName, string secondName)
        {
            ITenant firstTenant = this.scenarioContext.Get<ITenant>(firstName);
            ITenant secondTenant = this.scenarioContext.Get<ITenant>(secondName);
            Assert.AreEqual(firstTenant.Id, secondTenant.Id);
        }

        [Then("The tenant called \"(.*)\" has tenant Id \"(.*)\"")]
        public void ThenTheTenantCalledHasTenantId(string tenantName, string expectedTenantId)
        {
            ITenant tenant = this.scenarioContext.Get<ITenant>(tenantName);
            Assert.AreEqual(expectedTenantId, tenant.Id);
        }

        [Then("the tenant called \"(.*)\" should have the properties")]
        public void ThenTheTenantCalledShouldHaveTheProperties(string tenantName, Table table)
        {
            ITenant tenant = this.scenarioContext.Get<ITenant>(tenantName);

            foreach (DataTableRow row in table.Rows)
            {
                row.TryGetValue("Key", out string key);
                row.TryGetValue("Value", out string value);
                row.TryGetValue("Type", out string type);
                switch (type)
                {
                    case "string":
                        {
                            Assert.IsTrue(tenant.Properties.TryGet<string>(key, out string? actual));
                            Assert.AreEqual(value, actual);
                            break;
                        }

                    case "integer":
                        {
                            Assert.IsTrue(tenant.Properties.TryGet<int>(key, out int actual));
                            Assert.AreEqual(int.Parse(value), actual);
                            break;
                        }

                    case "datetimeoffset":
                        {
                            Assert.IsTrue(tenant.Properties.TryGet<DateTimeOffset>(key, out DateTimeOffset actual));
                            Assert.AreEqual(DateTimeOffset.Parse(value), actual);
                            break;
                        }

                    default:
                        throw new InvalidOperationException($"Unknown data type '{type}'");
                }
            }
        }

        [Given("I create a child tenant called \"(.*)\" for the root tenant")]
        public async Task GivenICreateAChildTenantCalledForTheRootTenant(string tenantName)
        {
            ITenant tenant = await this.store.CreateChildTenantAsync(RootTenant.RootTenantId, tenantName).ConfigureAwait(false);
            this.scenarioContext.Set(tenant, tenantName);
        }

        [Given("I create a well known child tenant called \"(.*)\" with a Guid of \"(.*)\" for the root tenant")]
        public async Task GivenICreateAWellKnownChildTenantCalledWithAGuidOfForTheRootTenant(
            string tenantName,
            Guid wellKnownChildTenantGuid)
        {
            ITenant tenant = await this.store.CreateWellKnownChildTenantAsync(
                RootTenant.RootTenantId,
                wellKnownChildTenantGuid,
                tenantName).ConfigureAwait(false);
            this.scenarioContext.Set(tenant, tenantName);
        }

        [Given("I create a child tenant called \"(.*)\" for the tenant called \"(.*)\"")]
        public async Task GivenICreateAChildTenantCalledForTheTenantCalled(string childName, string parentName)
        {
            ITenant parentTenant = this.scenarioContext.Get<ITenant>(parentName);
            ITenant tenant = await this.store.CreateChildTenantAsync(parentTenant.Id, childName).ConfigureAwait(false);
            this.scenarioContext.Set(tenant, childName);
        }

        [Given("I create a well known child tenant called \"(.*)\" with a Guid of \"(.*)\" for tenant called \"(.*)\"")]
        public async Task GivenICreateAWellKnownChildTenantCalledWithAGuidOfForTenantCalled(
            string childName,
            Guid wellKnownChildTenantGuid,
            string parentName)
        {
            ITenant parentTenant = this.scenarioContext.Get<ITenant>(parentName);

            try
            {
                ITenant tenant = await this.store.CreateWellKnownChildTenantAsync(
                    parentTenant.Id,
                    wellKnownChildTenantGuid,
                    childName).ConfigureAwait(false);
                this.scenarioContext.Set(tenant, childName);
            }
            catch (Exception ex)
            {
                this.scenarioContext.Set(ex);
            }
        }

        [Given(@"I update the properties of the tenant called ""([^""]*)""")]
        [When(@"I update the properties of the tenant called ""([^""]*)""")]
        public async Task GivenIUpdateThePropertiesOfTheTenantCalled(string tenantName, Table table)
        {
            await this.WhenIUpdateThePropertiesOfTheTenantCalled(tenantName, null, table).ConfigureAwait(false);
        }

        [Given("I update the properties of the tenant called \"(.*)\" and call the returned tenant \"(.*)\"")]
        [When("I update the properties of the tenant called \"(.*)\" and call the returned tenant \"(.*)\"")]
        public async Task WhenIUpdateThePropertiesOfTheTenantCalled(
            string tenantName,
            string? returnedTenantName,
            Table table)
        {
            ITenant tenant = this.scenarioContext.Get<ITenant>(tenantName);

            var properties = new Dictionary<string, object>();
            foreach (DataTableRow row in table.Rows)
            {
                row.TryGetValue("Key", out string key);
                row.TryGetValue("Value", out string value);
                row.TryGetValue("Type", out string type);
                switch (type)
                {
                    case "string":
                        {
                            properties.Add(key, value);
                            break;
                        }

                    case "integer":
                        {
                            properties.Add(key, int.Parse(value));
                            break;
                        }

                    case "datetimeoffset":
                        {
                            properties.Add(key, DateTimeOffset.Parse(value));
                            break;
                        }

                    default:
                        throw new InvalidOperationException($"Unknown data type '{type}'");
                }
            }

            ITenant updatedTenant = await this.store.UpdateTenantAsync(
                tenant.Id,
                propertiesToSetOrAdd: properties).ConfigureAwait(false);

            if (returnedTenantName is not null)
            {
                this.scenarioContext.Set(updatedTenant, returnedTenantName);
            }
        }

        [When(@"I remove the ""(.*)"" property of the tenant called ""(.*)"" and call the returned tenant ""(.*)""")]
        public async Task WhenIRemoveThePropertyOfTheTenantCalledAsync(
            string propertyName,
            string tenantName,
            string returnedTenantName)
        {
            ITenant tenant = this.scenarioContext.Get<ITenant>(tenantName);
            ITenant updatedTenant = await this.store.UpdateTenantAsync(
                tenant.Id,
                propertiesToRemove: new[] { propertyName })
                .ConfigureAwait(false);
            this.scenarioContext.Set(updatedTenant, returnedTenantName);
        }

        [When(@"I update the properties of the tenant called ""(.*)"" and remove the ""(.*)"" property and call the returned tenant ""(.*)""")]
        public async Task WhenIUpdateThePropertiesOfTheTenantCalledAndRemoveThePropertyAndCallTheReturnedTenant(
            string tenantName,
            string propertyToRemove,
            string returnedTenantName,
            Table table)
        {
            await this.WhenIUpdateThePropertiesOfTheTenantCalled(tenantName, null, table).ConfigureAwait(false);
            await this.WhenIRemoveThePropertyOfTheTenantCalledAsync(propertyToRemove, tenantName, returnedTenantName).ConfigureAwait(false);
        }

        [When(@"I change the name of the tenant called ""(.*)"" to ""(.*)"" and call the returned tenant ""(.*)""")]
        public async Task WhenIChangeTheNameOfTheTenantCalledToAndCallTheReturnedTenantAsync(
            string tenantName,
            string newName,
            string returnedTenantName)
        {
            ITenant tenant = this.scenarioContext.Get<ITenant>(tenantName);
            ITenant updatedTenant = await this.store.UpdateTenantAsync(
                tenant.Id,
                name: newName).ConfigureAwait(false);

            this.scenarioContext.Set(updatedTenant, returnedTenantName);
        }

        [When("I get the children of the tenant with the id called \"(.*)\" with maxItems (.*) and call them \"(.*)\"")]
        public async Task WhenIGetTheChildrenOfTheTenantWithTheIdCalledWithMaxItemsAndCallThem(string tenantIdName, int maxItems, string childrenName)
        {
            string tenantId = this.scenarioContext.Get<string>(tenantIdName);
            TenantCollectionResult children = await this.store.GetChildrenAsync(tenantId, maxItems).ConfigureAwait(false);
            this.scenarioContext.Set(children, childrenName);
        }

        [When("I get the children of the tenant with the id called \"(.*)\" with maxItems (.*) and continuation token \"(.*)\" and call them \"(.*)\"")]
        public async Task WhenIGetTheChildrenOfTheTenantWithTheIdCalledWithMaxItemsAndCallThem(string tenantIdName, int maxItems, string continuationTokenSource, string childrenName)
        {
            string tenantId = this.scenarioContext.Get<string>(tenantIdName);
            TenantCollectionResult previousChildren = this.scenarioContext.Get<TenantCollectionResult>(continuationTokenSource);
            TenantCollectionResult children = await this.store.GetChildrenAsync(tenantId, maxItems, previousChildren.ContinuationToken).ConfigureAwait(false);
            this.scenarioContext.Set(children, childrenName);
        }

        [Then("the ids of the children called \"(.*)\" should match the ids of the tenants called")]
        public void ThenTheIdsOfTheChildrenCalledShouldMatchTheIdsOfTheTenantsCalled(string childrenName, Table table)
        {
            TenantCollectionResult children = this.scenarioContext.Get<TenantCollectionResult>(childrenName);
            Assert.AreEqual(table.Rows.Count, children.Tenants.Count);
            var expected = table.Rows.Select(r => this.scenarioContext.Get<ITenant>(r[0]).Id).ToList();
            CollectionAssert.AreEquivalent(expected, children.Tenants);
        }

        [Then("there should be (.*) tenants in \"(.*)\"")]
        public void ThenThereShouldBeTenantsIn(int count, string childrenName)
        {
            TenantCollectionResult children = this.scenarioContext.Get<TenantCollectionResult>(childrenName);
            Assert.AreEqual(count, children.Tenants.Count);
        }

        [Then(@"the ids of the children called ""(.*)"" and ""(.*)"" should each match (.*) of the ids of the tenants called")]
        public void ThenTheIdsOfTheChildrenCalledAndShouldEachMatchOfTheIdsOfTheTenantsCalled(string childrenName1, string childrenName2, int count, Table table)
        {
            TenantCollectionResult children1 = this.scenarioContext.Get<TenantCollectionResult>(childrenName1);
            TenantCollectionResult children2 = this.scenarioContext.Get<TenantCollectionResult>(childrenName2);
            Assert.AreEqual(count, children1.Tenants.Count);
            Assert.AreEqual(count, children2.Tenants.Count);
            var expected = table.Rows.Select(r => this.scenarioContext.Get<ITenant>(r[0]).Id).ToList();
            CollectionAssert.AreEquivalent(expected, children1.Tenants.Union(children2.Tenants));
        }

        [When("I delete the tenant with the id called \"(.*)\"")]
        public async Task WhenIDeleteTheTenantWithTheIdCalled(string tenantIdName)
        {
            string tenantId = this.scenarioContext.Get<string>(tenantIdName);
            await this.store.DeleteTenantAsync(tenantId).ConfigureAwait(false);
        }

        [When("I get a tenant with id \"(.*)\"")]
        public async Task WhenIGetATenantWithId(string tenantId)
        {
            try
            {
                await this.store.GetTenantAsync(tenantId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.scenarioContext.Set(ex);
            }
        }

        [Then("it should throw a TenantNotFoundException")]
        public void ThenItShouldThrowATenantNotFoundException()
        {
            Assert.IsInstanceOf<TenantNotFoundException>(this.scenarioContext.Get<Exception>());
        }

        [Given(@"I get the ETag of the tenant called ""(.*)"" and call it ""(.*)""")]
        public void GivenIGetTheETagOfTheTenantCalledAndCallIt(string tenantName, string eTagName)
        {
            ITenant tenant = this.scenarioContext.Get<ITenant>(tenantName);
            this.scenarioContext.Set(tenant.ETag, eTagName);
        }

        [When(@"I get the tenant with the id called ""(.*)"" and the ETag called ""(.*)""")]
        public async Task WhenIGetTheTenantWithTheIdCalledAndTheETagCalled(string tenantIdName, string tenantETagName)
        {
            try
            {
                ITenant? tenant = await this.store.GetTenantAsync(this.scenarioContext.Get<string>(tenantIdName), this.scenarioContext.Get<string>(tenantETagName)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.scenarioContext.Set(ex);
            }
        }

        [Then("it should throw a TenantNotModifiedException")]
        public void ThenItShouldThrowATenantNotModifiedException()
        {
            Assert.IsInstanceOf<TenantNotModifiedException>(this.scenarioContext.Get<Exception>());
        }

        [Then("an \"(.*)\" is thrown")]
        public void ThenAnIsThrown(string expectedExceptionType)
        {
            this.scenarioContext.TryGetValue(out Exception ex);
            Assert.IsNotNull(ex, $"Expected an exception of type '{expectedExceptionType}' but no exception was thrown");
            Assert.AreEqual(expectedExceptionType, ex.GetType().Name);
        }

        [Then("no exception is thrown")]
        public void ThenNoExceptionIsThrown()
        {
            Assert.IsNull(this.scenarioContext.TryGetValue(out Exception x) ? x : null);
        }
    }
}