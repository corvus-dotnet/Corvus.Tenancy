// <copyright file="CosmosLegacyConfigurationTransformationSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Features.Cosmos;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Corvus.Storage.Azure.Cosmos;
using Corvus.Storage.Azure.Cosmos.Tenancy;
using NUnit.Framework;
using Reqnroll;

[Binding]
public class CosmosLegacyConfigurationTransformationSteps
{
    private LegacyV2CosmosContainerConfiguration? legacyConfiguration;
    private CosmosContainerConfiguration? resultingConfiguration;

    [Given("legacy v2 cosmos storage configuration with these properties")]
    public void GivenLegacyVCosmosStorageConfigurationWithTheseProperties(Table table)
    {
        this.legacyConfiguration = table.CreateInstance<LegacyV2CosmosContainerConfiguration>();
    }

    [When("the legacy v2 cosmos storage configuration is converted to the new format")]
    public void WhenTheLegacyVCosmosStorageConfigurationIsConvertedToTheNewFormat()
    {
        this.resultingConfiguration = LegacyCosmosConfigurationConverter.FromV2ToV3(this.legacyConfiguration!);
    }

    [Then("the resulting CosmosContainerConfiguration has these properties")]
    public void ThenTheResultingCosmosContainerConfigurationHasTheseProperties(Table table)
    {
        AssertProperties(this.resultingConfiguration, table);
    }

    [Then(@"the resulting CosmosContainerConfiguration\.AccessKeyInKeyVault has these properties")]
    public void ThenTheResultingCosmosContainerConfiguration_AccessKeyInKeyVaultHasTheseProperties(Table table)
    {
        AssertProperties(this.resultingConfiguration!.AccessKeyInKeyVault, table);
    }

    private static void AssertProperties<T>(T value, Table table)
    {
        IEnumerable<(string, string)> expectedProperties = table.CreateSet(
            row => (row["PropertyName"], row["Value"]));

        HashSet<string> propertiesNotExpectedToBeNull = new();
        foreach ((string name, string expectedValue) in expectedProperties)
        {
            propertiesNotExpectedToBeNull.Add(name);

            string effectiveExpectedValue = expectedValue == "$WellKnownCosmosDevelopmentStorageUri"
                ? "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
                : expectedValue;

            PropertyInfo pi = typeof(T).GetProperty(name)!;
            object? actualValue = pi.GetValue(value);
            if (effectiveExpectedValue == "<notnull>")
            {
                // The test expects this to be set, but it's a nested value that's going to
                // be checked in detail elsewhere.
                Assert.IsNotNull(actualValue);
            }
            else
            {
                Assert.AreEqual(actualValue, effectiveExpectedValue);
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