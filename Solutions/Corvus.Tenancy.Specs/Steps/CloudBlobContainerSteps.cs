namespace Corvus.Tenancy.Specs.Steps
{
    using Corvus.Tenancy.Specs.Bindings;
    using Microsoft.Azure.Storage.Blob;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    [Binding]
    public class CloudBlobContainerSteps
    {
        private readonly FeatureContext featureContext;

        public CloudBlobContainerSteps(FeatureContext featureContext)
        {
            this.featureContext = featureContext;
        }

        [Then(@"I should be able to get the tenanted cloud blob container")]
        public void ThenIShouldBeAbleToGetTheTenantedContainer()
        {
            CloudBlobContainer cloudBlobContainer = this.featureContext.Get<CloudBlobContainer>(TenancyCloudBlobContainerBindings.TenancySpecsContainer);
            Assert.IsNotNull(cloudBlobContainer);
        }
    }
}
