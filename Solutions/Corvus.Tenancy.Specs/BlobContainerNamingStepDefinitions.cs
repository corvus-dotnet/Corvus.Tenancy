// <copyright file="BlobContainerNamingStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs
{
    using System;
    using System.Collections.Generic;

    using Corvus.Json;
    using Corvus.Storage.Azure.BlobStorage.Tenancy;
    using Corvus.Tenancy.Internal;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    using TechTalk.SpecFlow;

    [Binding]
    public sealed class BlobContainerNamingStepDefinitions : IDisposable
    {
        private readonly Dictionary<string, ITenant> tenants = new ();
        private readonly Dictionary<string, string> physicalContainerNames = new ();
        private readonly ServiceProvider serviceProvider;
        private readonly IPropertyBagFactory propertyBagFactory;

        public BlobContainerNamingStepDefinitions()
        {
            ServiceCollection services = new ();
            services.AddRequiredTenancyServices();
            this.serviceProvider = services.BuildServiceProvider();

            this.propertyBagFactory = this.serviceProvider.GetRequiredService<IPropertyBagFactory>();
        }

        public void Dispose()
        {
            this.serviceProvider.Dispose();
        }

        [Given("a tenant labelled '([^']*)'")]
        public void GivenATenantLabelled(string tenantLabel)
        {
            this.GivenATenantLabelledWithId(tenantLabel, Guid.NewGuid().ToString("N"));
        }

        [Given("a tenant labelled '([^']*)' with id '([^']*)'")]
        public void GivenATenantLabelledWithId(string tenantLabel, string tenantId)
        {
            this.tenants.Add(
                tenantLabel,
                new Tenant(tenantId, tenantLabel, this.propertyBagFactory.Create(PropertyBagValues.Empty)));
        }

        [When("I get a blob container for tenant '([^']*)' with a logical name of '([^']*)' and label the result '([^']*)'")]
        public void WhenIGetABlobContainerForTenantWithALogicalNameOfAndLabelTheResult(
            string tenantLabel, string logicalContainerName, string resultLabel)
        {
            ITenant tenant = this.tenants[tenantLabel];
            this.physicalContainerNames.Add(
                resultLabel,
                AzureStorageBlobTenantedContainerNaming.GetHashedTenantedBlobContainerNameFor(tenant, logicalContainerName));
        }

        [Then("the returned container names '([^']*)' and '([^']*)' are different")]
        public void ThenTheReturnedContainerNamesAndAreDifferent(string resultLabel1, string resultLabel2)
        {
            string result1 = this.physicalContainerNames[resultLabel1];
            string result2 = this.physicalContainerNames[resultLabel2];

            Assert.AreNotEqual(result1, result2);
        }

        [Then("the returned container names '([^']*)' and '([^']*)' are the same")]
        public void ThenTheReturnedContainerNamesAndAreTheSame(string resultLabel1, string resultLabel2)
        {
            string result1 = this.physicalContainerNames[resultLabel1];
            string result2 = this.physicalContainerNames[resultLabel2];

            Assert.AreEqual(result1, result2);
        }

        [Then("the name returned container name '([^']*)' should be '([^']*)'")]
        public void ThenTheNameReturnedContainerNameShouldBe(string resultLabel, string expectedResult)
        {
            string result = this.physicalContainerNames[resultLabel];
            Assert.AreEqual(expectedResult, result);
        }
    }
}