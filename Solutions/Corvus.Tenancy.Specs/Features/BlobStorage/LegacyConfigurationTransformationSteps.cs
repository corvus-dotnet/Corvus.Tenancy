// <copyright file="LegacyConfigurationTransformationSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Features.BlobStorage
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Corvus.Storage.Azure.BlobStorage;
    using Corvus.Storage.Azure.BlobStorage.Tenancy;
    using NUnit.Framework;
    using Reqnroll;

    [Binding]
    public class LegacyConfigurationTransformationSteps
    {
        private LegacyV2BlobStorageConfiguration legacyConfiguration = new();
        private BlobContainerConfiguration? resultingConfiguration;

        [Given("legacy v2 blob storage configuration with these properties")]
        public void GivenLegacyVConfigurationWithTheseProperties(Table table)
        {
            this.legacyConfiguration = table.CreateInstance<LegacyV2BlobStorageConfiguration>();
        }

        [When("the legacy v2 blob storage configuration is converted to the new format")]
        public void WhenTheLegacyVConfigurationIsConvertedToTheNewFormat()
        {
            this.resultingConfiguration = LegacyConfigurationConverter.FromV2ToV3(this.legacyConfiguration);
        }

        [Then("the resulting BlobContainerConfiguration has these properties")]
        public void ThenTheResultingBlobContainerConfigurationHasTheseProperties(Table table)
        {
            AssertProperties(this.resultingConfiguration, table);
        }

        [Then(@"the resulting BlobContainerConfiguration\.AccessKeyInKeyVault has these properties")]
        public void ThenTheResultingBlobContainerConfiguration_AccessKeyInKeyVaultHasTheseProperties(Table table)
        {
            AssertProperties(this.resultingConfiguration!.AccessKeyInKeyVault, table);
        }

        private static void AssertProperties<T>(T value, Table table)
        {
            IEnumerable<(string, string)> expectedProperties = table.CreateSet(
                row => (row["PropertyName"], row["Value"]));

            HashSet<string> propertiesNotExpectedToBeNull = [];
            foreach ((string name, string expectedValue) in expectedProperties)
            {
                propertiesNotExpectedToBeNull.Add(name);

                PropertyInfo pi = typeof(T).GetProperty(name)!;
                object? actualValue = pi.GetValue(value);
                if (expectedValue == "<notnull>")
                {
                    // The test expects this to be set, but it's a nested value that's going to
                    // be checked in detail elsewhere.
                    Assert.IsNotNull(actualValue);
                }
                else if (expectedValue == "<null>")
                {
                    Assert.IsNull(actualValue);
                }
                else
                {
                    Assert.AreEqual(actualValue, expectedValue);
                }
            }

            IEnumerable<PropertyInfo> nullProperties = typeof(T).GetProperties()
                .Where(pi => !propertiesNotExpectedToBeNull.Contains(pi.Name));
            foreach (PropertyInfo? pi in nullProperties)
            {
                object? actualValue = pi.GetValue(value);
                Assert.IsNull(actualValue);
            }
        }
    }
}