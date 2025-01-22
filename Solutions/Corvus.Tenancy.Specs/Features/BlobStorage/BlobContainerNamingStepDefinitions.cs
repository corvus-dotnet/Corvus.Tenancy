// <copyright file="BlobContainerNamingStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Features.BlobStorage
{
    using Corvus.Storage.Azure.BlobStorage.Tenancy;
    using Corvus.Tenancy.Specs.Bindings;

    using Reqnroll;

    [Binding]
    public sealed class BlobContainerNamingStepDefinitions
    {
        private readonly TenantedNameBindings tenantedNameBindings;

        public BlobContainerNamingStepDefinitions(
            TenantedNameBindings tenantedNameBindings)
        {
            this.tenantedNameBindings = tenantedNameBindings;
        }

        [When("I get a blob container name for tenant '([^']*)' with a logical name of '([^']*)' and label the result '([^']*)'")]
        public void WhenIGetABlobContainerForTenantWithALogicalNameOfAndLabelTheResult(
            string tenantLabel, string logicalContainerName, string resultLabel)
        {
            ITenant tenant = this.tenantedNameBindings.Tenants[tenantLabel];
            this.tenantedNameBindings.AddTenantedContainerName(
                resultLabel,
                AzureStorageBlobTenantedContainerNaming.GetHashedTenantedBlobContainerNameFor(tenant, logicalContainerName));
        }

        [When("I get a blob container name for tenantId '([^']*)' with a logical name of '([^']*)' and label the result '([^']*)'")]
        public void WhenIGetABlobContainerNameForTenantIdWithALogicalNameOfAndLabelTheResult(
            string tenantId, string logicalContainerName, string resultLabel)
        {
            this.tenantedNameBindings.AddTenantedContainerName(
                resultLabel,
                AzureStorageBlobTenantedContainerNaming.GetHashedTenantedBlobContainerNameFor(tenantId, logicalContainerName));
        }
    }
}