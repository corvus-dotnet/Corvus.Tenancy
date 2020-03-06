namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Tenancy.Exceptions;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class TenantStorageSteps
    {
        private readonly FeatureContext featureContext;
        private readonly ScenarioContext scenarioContext;

        public TenantStorageSteps(FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            this.featureContext = featureContext;
            this.scenarioContext = scenarioContext;
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
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            ITenant? tenant = await provider.GetTenantAsync(this.scenarioContext.Get<string>(tenantIdName)).ConfigureAwait(false);
            this.scenarioContext.Set(tenant, tenantName);
        }

        [Then("the tenant called \"(.*)\" should have the same ID as the tenant called \"(.*)\"")]
        public void ThenTheTenantCalledShouldHaveTheSameIDAsTheTenantCalled(string firstName, string secondName)
        {
            ITenant firstTenant = this.scenarioContext.Get<ITenant>(firstName);
            ITenant secondTenant = this.scenarioContext.Get<ITenant>(secondName);
            Assert.AreEqual(firstTenant.Id, secondTenant.Id);
        }

        [Then("the tenant called \"(.*)\" should have the properties")]
        public void ThenTheTenantCalledShouldHaveTheProperties(string tenantName, Table table)
        {
            ITenant tenant = this.scenarioContext.Get<ITenant>(tenantName);

            foreach (TableRow row in table.Rows)
            {
                row.TryGetValue("Key", out string key);
                row.TryGetValue("Value", out string value);
                row.TryGetValue("Type", out string type);
                switch (type)
                {
                    case "string":
                        {
                            Assert.IsTrue(tenant.Properties.TryGet<string>(key, out string actual));
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
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            ITenant tenant = await provider.CreateChildTenantAsync(RootTenant.RootTenantId).ConfigureAwait(false);
            this.scenarioContext.Set(tenant, tenantName);
        }

        [Given("I create a child tenant called \"(.*)\" for the tenant called \"(.*)\"")]
        public async Task GivenICreateAChildTenantCalledForTheTenantCalled(string childName, string parentName)
        {
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            ITenant parentTenant = this.scenarioContext.Get<ITenant>(parentName);
            ITenant tenant = await provider.CreateChildTenantAsync(parentTenant.Id).ConfigureAwait(false);
            this.scenarioContext.Set(tenant, childName);
        }

        [When("I update the properties of the tenant called \"(.*)\"")]
        public void WhenIUpdateThePropertiesOfTheTenantCalled(string tenantName, Table table)
        {
            ITenant tenant = this.scenarioContext.Get<ITenant>(tenantName);
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();

            foreach (TableRow row in table.Rows)
            {
                row.TryGetValue("Key", out string key);
                row.TryGetValue("Value", out string value);
                row.TryGetValue("Type", out string type);
                switch (type)
                {
                    case "string":
                        {
                            tenant.Properties.Set(key, value);
                            break;
                        }

                    case "integer":
                        {
                            tenant.Properties.Set(key, int.Parse(value));
                            break;
                        }

                    case "datetimeoffset":
                        {
                            tenant.Properties.Set(key, DateTimeOffset.Parse(value));
                            break;
                        }

                    default:
                        throw new InvalidOperationException($"Unknown data type '{type}'");
                }
            }

            provider.UpdateTenantAsync(tenant);
        }

        [When("I get the children of the tenant with the id called \"(.*)\" with maxItems (.*) and call them \"(.*)\"")]
        public async Task WhenIGetTheChildrenOfTheTenantWithTheIdCalledWithMaxItemsAndCallThem(string tenantIdName, int maxItems, string childrenName)
        {
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            string tenantId = this.scenarioContext.Get<string>(tenantIdName);
            TenantCollectionResult children = await provider.GetChildrenAsync(tenantId, maxItems).ConfigureAwait(false);
            this.scenarioContext.Set(children, childrenName);
        }

        [When("I get the children of the tenant with the id called \"(.*)\" with maxItems (.*) and continuation token \"(.*)\" and call them \"(.*)\"")]
        public async Task WhenIGetTheChildrenOfTheTenantWithTheIdCalledWithMaxItemsAndCallThem(string tenantIdName, int maxItems, string continuationTokenSource, string childrenName)
        {
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            string tenantId = this.scenarioContext.Get<string>(tenantIdName);
            TenantCollectionResult previousChildren = this.scenarioContext.Get<TenantCollectionResult>(continuationTokenSource);
            TenantCollectionResult children = await provider.GetChildrenAsync(tenantId, maxItems, previousChildren.ContinuationToken).ConfigureAwait(false);
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
        public Task WhenIDeleteTheTenantWithTheIdCalled(string tenantIdName)
        {
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            string tenantId = this.scenarioContext.Get<string>(tenantIdName);
            return provider.DeleteTenantAsync(tenantId);
        }

        [When("I get a tenant with id \"(.*)\"")]
        public async Task WhenIGetATenantWithId(string tenantId)
        {
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            try
            {
                await provider.GetTenantAsync(tenantId).ConfigureAwait(false);
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
                ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
                ITenant? tenant = await provider.GetTenantAsync(this.scenarioContext.Get<string>(tenantIdName), this.scenarioContext.Get<string>(tenantETagName)).ConfigureAwait(false);
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
    }
}
