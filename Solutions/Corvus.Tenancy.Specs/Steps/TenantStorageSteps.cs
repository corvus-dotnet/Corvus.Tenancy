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

        [When(@"I get the tenant id of the tenant called '(.*)' and call it '(.*)'")]
        public void WhenIGetTheTenantIdOfTheTenantCalledAndCallIt(string tenantName, string tenantIdName)
        {
            ITenant tenant = this.scenarioContext.Get<ITenant>(tenantName);
            this.scenarioContext.Set(tenant.Id, tenantIdName);
        }

        [When(@"I get the tenant with the id called '(.*)' and call it '(.*)'")]
        public async Task WhenIGetTheTenantWithTheIdCalled(string tenantIdName, string tenantName)
        {
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            ITenant tenant = await provider.GetTenantAsync(this.scenarioContext.Get<string>(tenantIdName));
            this.scenarioContext.Set(tenant, tenantName);
        }

        [Then(@"the tenant called '(.*)' should have the same ID as the tenant called '(.*)'")]
        public void ThenTheTenantCalledShouldHaveTheSameIDAsTheTenantCalled(string firstName, string secondName)
        {
            ITenant firstTenant = this.scenarioContext.Get<ITenant>(firstName);
            ITenant secondTenant = this.scenarioContext.Get<ITenant>(secondName);
            Assert.AreEqual(firstTenant.Id, secondTenant.Id);
        }

        [Then(@"the tenant called '(.*)' should have the properties")]
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


        [Given(@"I create a child tenant called '(.*)' for the root tenant")]
        public async Task GivenICreateAChildTenantCalledForTheRootTenant(string tenantName)
        {
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            ITenant tenant = await provider.CreateChildTenantAsync(RootTenant.RootTenantId);
            this.scenarioContext.Set(tenant, tenantName);
        }

        [Given(@"I create a child tenant called '(.*)' for the tenant called '(.*)'")]
        public async Task GivenICreateAChildTenantCalledForTheTenantCalled(string childName, string parentName)
        {
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            ITenant parentTenant = this.scenarioContext.Get<ITenant>(parentName);
            ITenant tenant = await provider.CreateChildTenantAsync(parentTenant.Id);
            this.scenarioContext.Set(tenant, childName);
        }


        [When(@"I update the properties of the tenant called '(.*)'")]
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

        [When(@"I get the children of the tenant with the id called '(.*)' and call them '(.*)'")]
        public async Task WhenIGetTheChildrenTenantWithTheIdCalledAndCallThem(string tenantIdName, string childrenName)
        {
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            string tenantId = this.scenarioContext.Get<string>(tenantIdName);
            TenantCollectionResult children = await provider.GetChildrenAsync(tenantId);
            this.scenarioContext.Set(children, childrenName);
        }

        [Then(@"the ids of the children called '(.*)' should match the ids of the tenants called")]
        public void ThenTheIdsOfTheChildrenCalledShouldMatchTheIdsOfTheTenantsCalled(string childrenName, Table table)
        {
            TenantCollectionResult children = this.scenarioContext.Get<TenantCollectionResult>(childrenName);
            Assert.AreEqual(table.Rows.Count, children.Tenants.Count);
            var expected = table.Rows.Select(r => this.scenarioContext.Get<ITenant>(r[0]).Id).ToList();
            CollectionAssert.AreEquivalent(expected, children.Tenants);
        }

        [When(@"I delete the tenant with the id called '(.*)'")]
        public Task WhenIDeleteTheTenantWithTheIdCalled(string tenantIdName)
        {
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            string tenantId = this.scenarioContext.Get<string>(tenantIdName);
            return provider.DeleteTenantAsync(tenantId);
        }

        [When(@"I get a tenant with id '(.*)'")]
        public async Task WhenIGetATenantWithId(string tenantId)
        {
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            try
            {
                await provider.GetTenantAsync(tenantId);
            }
            catch(Exception ex)
            {
                this.scenarioContext.Set(ex);
            }
        }

        [Then(@"it should throw a TenantNotFoundException")]
        public void ThenItShouldThrowATenantNotFoundException()
        {
            Assert.IsInstanceOf<TenantNotFoundException>(this.scenarioContext.Get<Exception>());
        }

    }
}
