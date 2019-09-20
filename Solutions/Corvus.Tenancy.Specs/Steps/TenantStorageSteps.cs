namespace Corvus.Tenancy.Specs.Steps
{
    using System;
    using System.Threading.Tasks;
    using Corvus.Extensions.Json;
    using Corvus.SpecFlow.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
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


        [Then(@"the tenant called '(.*)' should be the same as the tenant called '(.*)'")]
        public void ThenTheTenantCalledShouldBeTheSameAsTheTenantCalled(string firstName, string secondName)
        {
            ITenant firstTenant = this.scenarioContext.Get<ITenant>(firstName);
            ITenant secondTenant = this.scenarioContext.Get<ITenant>(secondName);
            Assert.AreEqual(firstTenant.Id, secondTenant.Id);
            JsonSerializerSettings serializerSettings = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<IJsonSerializerSettingsProvider>().Instance;
            Assert.AreEqual(JsonConvert.SerializeObject(firstTenant.Properties, serializerSettings), JsonConvert.SerializeObject(secondTenant.Properties, serializerSettings));
        }

        [Given(@"I create a child tenant called '(.*)' for the root tenant")]
        public async Task GivenICreateAChildTenantCalledForTheRootTenant(string tenantName)
        {
            ITenantProvider provider = ContainerBindings.GetServiceProvider(this.featureContext).GetRequiredService<ITenantProvider>();
            ITenant tenant = await provider.CreateChildTenantAsync(RootTenant.RootTenantId);
            this.scenarioContext.Set(tenant, tenantName);
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
    }
}
