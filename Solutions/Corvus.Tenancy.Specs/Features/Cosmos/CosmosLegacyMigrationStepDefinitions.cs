// <copyright file="CosmosLegacyMigrationStepDefinitions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Features.Cosmos;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Corvus.Json;
using Corvus.Storage.Azure.Cosmos;
using Corvus.Storage.Azure.Cosmos.Tenancy;
using Corvus.Tenancy.Specs.Bindings;
using Corvus.Testing.SpecFlow;

using FluentAssertions;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using TechTalk.SpecFlow;

[Binding]
public class CosmosLegacyMigrationStepDefinitions
{
    private const string PartitionKeyPath = "/foo/bar";
    private readonly TenancyCosmosContainerBindings cosmosBindings;
    private readonly IPropertyBagFactory pbf;
    private readonly ICosmosContainerSourceFromDynamicConfiguration containerSource;
    private readonly ICosmosContainerSourceWithTenantLegacyTransition legacyTransitionContainerSource;
    private readonly string tenantId = RootTenant.RootTenantId.CreateChildId(Guid.NewGuid());
    private IPropertyBag tenantProperties;

    /// <summary>
    /// The V2-style configuration in the form expected in tenant properties.
    /// </summary>
    private LegacyV2CosmosContainerConfiguration? legacyConfiguration;

    /// <summary>
    /// V3-style configuration equivalent to <see cref="legacyConfiguration"/> in the form expected
    /// in tenant properties. Note that this won't necessarily include a container, because
    /// applications might want to store a single per-database config, but use multiple containers
    /// within that.
    /// </summary>
    private CosmosContainerConfiguration? v3Configuration;

    /// <summary>
    /// V3-style configuration that always has a container name even if <see cref="v3Configuration"/>
    /// does not. Useful for when we want to obtain a CosmosClient from Corvus.Storage, which will
    /// complain if there's no container. (Nonetheless if's valid for a tenant to contains
    /// config with no container because the app may always plug in a specific one at runtime.)
    /// </summary>
    private CosmosContainerConfiguration? v3ConfigurationWithContainer;
    private CosmosContainerConfiguration? configReturnedFromMigration;
    private string? databaseNameArgument;
    private string? containerNameArgument;

    private Container? cosmosContainer;

    public CosmosLegacyMigrationStepDefinitions(
        ScenarioContext scenarioContext,
        TenancyCosmosContainerBindings cosmosBindings)
    {
        this.cosmosBindings = cosmosBindings;

        IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(scenarioContext);
        this.pbf = serviceProvider.GetRequiredService<IPropertyBagFactory>();
        this.tenantProperties = this.pbf.Create(PropertyBagValues.Empty);

        this.containerSource = serviceProvider.GetRequiredService<ICosmosContainerSourceFromDynamicConfiguration>();
        this.legacyTransitionContainerSource = serviceProvider.GetRequiredService<ICosmosContainerSourceWithTenantLegacyTransition>();
    }

    public enum NameLocations
    {
        Config,
        Args,
        DbInConfigContainerInArg,
    }

    [Given("Cosmos database and container names set in (.*) with tenant-specific names set to '([^']*)'")]
    public void GivenComosDatabaseAndContainerNamesSetInConfig(
        NameLocations nameLocation, bool autoTenantSpecificNamesEnabled)
    {
        if (!autoTenantSpecificNamesEnabled && nameLocation != NameLocations.Config)
        {
            // With the old V2 factory, when database or container names were not in config, they
            // would be derived from the CosmosContainerDefinition, and the
            // TenantCosmosContainerFactory would always convert these to tenanted names. So the
            // only situation in which you could avoid tenant prefix generation was to put the
            // database and container names in configuration, and set DisableTenantIdPrefix to
            // true.
            throw new NotSupportedException("Automatic tenant-specific names can only be disabled with NameLocations.Config");
        }

        ITenant tenant = this.GetTenant();

        this.legacyConfiguration = this.cosmosBindings.TestLegacyCosmosConfiguration;
        this.legacyConfiguration!.DisableTenantIdPrefix = !autoTenantSpecificNamesEnabled;
        this.v3Configuration = LegacyCosmosConfigurationConverter.FromV2ToV3(this.legacyConfiguration);

        // The database and container names as a V2 app would specify them in the
        // CosmosContainerDefinition.
        string logicalDatabaseName = $"test-corvustenancy-{DateTime.UtcNow:yyyyMMddHHmmss}";
        string logicalContainerName = $"legacymigration-{Guid.NewGuid()}";

        // Tenant-specific database and container names in the form that the V2 libraries
        // would generate them either if they are being derived from the logical names
        // because the config doesn't specify them, or they are specified in the config
        // but IsTenend
        string autoTenantedDatabaseName = CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenant, logicalDatabaseName);
        string autoTenantedContainerName = CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenant, logicalContainerName);

        // In V2, the configuration might not contain the database and/or container name, and in
        // these cases, they are basd on the "tenanted container definition". In the V2 modes, the
        // app always passes a "container definition", which encapsulates both logical names, and
        // these get turned into tenant-specific names.
        // If the configuration does contain a database name and/or container name, then the name from
        // configuration is used, but may or may not be converted into a tenant-specific name first,
        // depending on the DisableTenantIdPrefix setting.
        switch (nameLocation)
        {
            case NameLocations.Config:
                // In this case, the V2 config does contain the database name. The V3 config
                // always contains the physical database name. If DisableTenantIdPrefix is
                // false (autoTenantSpecificNamesEnabled is true), those names in the V2
                // config become the actual physical names, and so we put the logical names
                // into both V2 and V3 config here. But if the V2 config says to generate
                // tenanted names, we put the logical name in V2 and the generated name in
                // V3.
                this.legacyConfiguration.DatabaseName = logicalDatabaseName;
                this.v3Configuration.Database = autoTenantSpecificNamesEnabled
                    ? autoTenantedDatabaseName : logicalDatabaseName;
                this.legacyConfiguration.ContainerName = logicalContainerName;
                this.v3Configuration.Container = autoTenantSpecificNamesEnabled
                    ? autoTenantedContainerName : logicalContainerName;
                break;

            case NameLocations.Args:
                // In this case, the V2 config contains no names. In the V2 world, the names
                // would always be generated as tenanted versions of the logical names from
                // the definitions. Since the V3 config always represents the actual database
                // name, we just set that.
                this.v3Configuration.Database = autoTenantedDatabaseName;

                this.databaseNameArgument = logicalDatabaseName;
                this.containerNameArgument = logicalContainerName;
                break;

            case NameLocations.DbInConfigContainerInArg:
                // This is a hybrid of the previous two. The database name is in the V2
                // configuration, so the same DisableTenantIdPrefix considerations apply
                // as for the Config case above, but the container name would always have
                // been derived as a tenanted version of the logical name.
                this.legacyConfiguration.DatabaseName = logicalDatabaseName;
                this.v3Configuration.Database = autoTenantSpecificNamesEnabled
                    ? autoTenantedDatabaseName : logicalDatabaseName;

                this.containerNameArgument = logicalContainerName;
                break;
        }

        this.v3ConfigurationWithContainer = this.v3Configuration.Container is null
#pragma warning disable SA1101 // Prefix local calls with this - StyleCop is confused
            ? this.v3Configuration with { Container = autoTenantSpecificNamesEnabled ? autoTenantedContainerName : logicalContainerName }
            : this.v3Configuration;
#pragma warning restore SA1101
    }

    [Given("the Cosmos database already exists with throughput of (.*)")]
    public async Task GivenTheCosmosDatabaseAlreadyExistsWithThroughputOf(int? databaseThroughput)
    {
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.v3ConfigurationWithContainer!).ConfigureAwait(false);
        await cosmosContainer.Database.Client.CreateDatabaseAsync(
            cosmosContainer.Database.Id,
            databaseThroughput)
            .ConfigureAwait(false);

        this.cosmosBindings.RemoveThisDatabaseOnTestTeardown(cosmosContainer.Database);
    }

    [Given("the Cosmos container already exists with per-database throughput")]
    public async Task GivenTheCosmosContainerAlreadyExistsWithPer_DatabaseThroughput()
    {
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.v3ConfigurationWithContainer!).ConfigureAwait(false);
        await cosmosContainer.Database.CreateContainerAsync(
            cosmosContainer.Id,
            PartitionKeyPath)
            .ConfigureAwait(false);
    }

    [Given("the tenant has the property '([^']*)' set to the legacy CosmosConfiguration")]
    public void GivenTheTenantHasThePropertySetToTheLegacyCosmosConfiguration(string configurationKey)
    {
        this.tenantProperties = this.pbf.CreateModified(
            this.tenantProperties,
            new Dictionary<string, object>
            {
                { configurationKey, this.legacyConfiguration ?? throw new InvalidOperationException("This test step should not be invoked when this.legacyConfiguration is null") },
            },
            null);
    }

    [Given("the tenant has the property '([^']*)' set to a bogus legacy CosmosConfiguration")]
    public void GivenTheTenantHasThePropertySvSetToABogusLegacyCosmosConfiguration(string configurationKey)
    {
        LegacyV2CosmosContainerConfiguration bogusConfiguration = new ()
        {
            AccountUri = "Well this is all wrong",
            KeyVaultName = "It's a good thing this should not be used during this test",
        };

        this.tenantProperties = this.pbf.CreateModified(
            this.tenantProperties,
            new Dictionary<string, object>
            {
                { configurationKey, bogusConfiguration },
            },
            null);
    }

    [Given("the tenant has the property '([^']*)' set to the CosmosContainerConfiguration")]
    public void GivenTheTenantHasThePropertySvSetToTheCosmosContainerConfiguration(string configurationKey)
    {
        this.tenantProperties = this.pbf.CreateModified(
            this.tenantProperties,
            new Dictionary<string, object>
            {
                { configurationKey, this.v3Configuration ?? throw new InvalidOperationException("This test step should not be invoked when this.configuration is null") },
            },
            null);
    }

    [When(@"ICosmosContainerSourceWithTenantLegacyTransition\.GetContainerForTenantAsync is called with configuration keys of '([^']*)' and '([^']*)' with db throughput of (.*) and container throughput of (.*)")]
    public async Task WhenICosmosContainerSourceWithTenantLegacyTransition_GetContainerForTenantAsyncIsCalledWithConfigurationKeysOfAndWithDbThroughputOfAndContainerThroughputOf(
        string v2ConfigurationKey,
        string v3ConfigurationKey,
        int? databaseThroughput,
        int? containerThroughput)
    {
        ITenant tenant = this.GetTenant();

        this.cosmosContainer = await this.legacyTransitionContainerSource.GetContainerForTenantAsync(
            tenant,
            v2ConfigurationKey,
            v3ConfigurationKey,
            this.databaseNameArgument,
            this.containerNameArgument,
            PartitionKeyPath,
            databaseThroughput,
            containerThroughput)
            .ConfigureAwait(false);
        Assert.IsNotNull(this.cosmosContainer);

        this.cosmosBindings.RemoveThisDatabaseOnTestTeardown(this.cosmosContainer.Database);
    }

    [When(@"ICosmosContainerSourceWithTenantLegacyTransition\.MigrateToV3Async is called with configuration keys of '([^']*)' and '([^']*)' with db throughput of (.*) and container throughput of (.*)")]
    public async Task WhenICosmosContainerSourceWithTenantLegacyTransition_MigrateToVAsyncIsCalledWithConfigurationKeysOfAndWithDbThroughputOfAndContainerThroughputOf(
        string v2ConfigurationKey,
        string v3ConfigurationKey,
        int? databaseThroughput,
        int? containerThroughput)
    {
        ITenant tenant = this.GetTenant();

        IEnumerable<string>? databaseNames = this.legacyConfiguration!.DatabaseName is null
            ? new[] { this.databaseNameArgument! }
            : null;
        IEnumerable<(string, string)>? containerNamesAndPartitionKeys = this.legacyConfiguration.ContainerName is null
            ? new[] { (this.containerNameArgument!, PartitionKeyPath) }
            : null;
        this.configReturnedFromMigration = await this.legacyTransitionContainerSource.MigrateToV3Async(
            tenant,
            v2ConfigurationKey,
            v3ConfigurationKey,
            databaseNames,
            containerNamesAndPartitionKeys,
            containerNamesAndPartitionKeys is null ? PartitionKeyPath : null,
            databaseThroughput,
            containerThroughput)
            .ConfigureAwait(false);

        // When only V3 config exists, this returns null because no migration was required
        if (this.configReturnedFromMigration is not null)
        {
            // In tests where the database and container name are passed in arguments, the returned
            // configuration won't include them (because this migration method takes lists of
            // databases and containers in that scenario, meaning there are potentially many
            // different valid configurations.
            CosmosContainerConfiguration configReturnedWithDatabaseAndContainer = this.configReturnedFromMigration with
            {
#pragma warning disable SA1101 // Prefix local calls with this - StyleCop is confused
                Database = this.configReturnedFromMigration.Database ??
                    (this.legacyConfiguration!.DisableTenantIdPrefix
                        ? this.databaseNameArgument!
                        : CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenant, this.databaseNameArgument!)),
                Container = this.configReturnedFromMigration.Container ??
#pragma warning restore SA1101
                (this.legacyConfiguration!.DisableTenantIdPrefix
                            ? this.containerNameArgument!
                            : CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenant, this.containerNameArgument!))
            };
            Container cosmosContainer = await this.containerSource.GetStorageContextAsync(configReturnedWithDatabaseAndContainer).ConfigureAwait(false);
            this.cosmosBindings.RemoveThisDatabaseOnTestTeardown(cosmosContainer.Database);
        }
    }

    [Then("a new Cosmos database with the specified name should have been created")]
    public async Task ThenANewCosmosDatabaseWithTheSpecifiedNameShouldHaveBeenCreated()
    {
        // The easiest way for us to get a client object with which we can inspect the database
        // is to ask Corvus.Tenancy for a Container based on the configuration.
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.v3ConfigurationWithContainer!).ConfigureAwait(false);
        await cosmosContainer.Database.ReadAsync().ConfigureAwait(false);
    }

    [Then("the Cosmos database throughput should match the specified (.*)")]
    public async Task ThenTheCosmosDatabaseThroughputShouldMatchTheSpecifiedThroughput(int? expectedDatabaseThroughput)
    {
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.v3ConfigurationWithContainer!).ConfigureAwait(false);
        int? reportedDatabaseThroughput = await cosmosContainer.Database.ReadThroughputAsync().ConfigureAwait(false);

        Assert.AreEqual(expectedDatabaseThroughput, reportedDatabaseThroughput);
    }

    [Then("a new Cosmos container with the specified name should have been created")]
    public async Task ThenANewCosmosContainerTheSpecifiedNameShouldHaveBeenCreated()
    {
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.v3ConfigurationWithContainer!).ConfigureAwait(false);
        await cosmosContainer.ReadContainerAsync().ConfigureAwait(false);
    }

    [Then("the Cosmos container throughput should match the specified (.*)")]
    public async Task ThenTheCosmosContainerThroughputShouldMatchTheSpecifiedThroughput(int? expectedContainerThroughput)
    {
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.v3ConfigurationWithContainer!).ConfigureAwait(false);
        int? reportedContainerThroughput = await cosmosContainer.ReadThroughputAsync().ConfigureAwait(false);

        Assert.AreEqual(expectedContainerThroughput, reportedContainerThroughput);
    }

    [Then("the Cosmos Container object returned should refer to the database")]
    public void ThenTheCosmosContainerObjectReturnedShouldReferToTheDatabase()
    {
        Assert.AreEqual(this.v3Configuration!.Database, this.cosmosContainer!.Database.Id);
    }

    [Then("the Cosmos Container object returned should refer to the container")]
    public void ThenTheCosmosContainerObjectReturnedShouldReferToTheContainer()
    {
        Assert.AreEqual(this.v3ConfigurationWithContainer!.Container, this.cosmosContainer!.Id);
    }

    [Then("MigrateToV3Async should have returned a CosmosContainerConfiguration with settings matching the legacy CosmosConfiguration")]
    public void ThenMigrateToVAsyncShouldHaveReturnedACosmosContainerConfigurationWithSettingsMatchingTheLegacyCosmosConfiguration()
    {
        // The v3 configuration we built up earlier in the test already looks mostly like we expect, but
        // there are a few differences in expectations around Database and Container name, depending on
        // which of the various modes this test is running in.
        // Also, the build-in value comparison we get thanks to CosmosContainerConfiguration being
        // a record doesn't work with key vault settings, because those aren't records so it ends
        // up doing identity comparison. When tests run in the CI build, we're generally using
        // key vault, so we need to handle that part specially.
        CosmosContainerConfiguration? expectedConfiguration = this.v3Configuration! with
        {
#pragma warning disable SA1101 // Prefix local calls with this - StyleCop is confused
            Database = this.configReturnedFromMigration!.Database,
            Container = this.configReturnedFromMigration.Container,
            AccessKeyInKeyVault = null,
#pragma warning restore SA1101
        };
        CosmosContainerConfiguration actualConfigurationExceptKeyVault = this.configReturnedFromMigration with
        {
#pragma warning disable SA1101 // Prefix local calls with this - StyleCop is confused
            AccessKeyInKeyVault = null,
#pragma warning restore SA1101
        };

        Assert.AreEqual(expectedConfiguration, actualConfigurationExceptKeyVault);

        if (expectedConfiguration.AccessKeyInKeyVault is not null)
        {
            Assert.AreEqual(expectedConfiguration.AccessKeyInKeyVault.VaultName, actualConfigurationExceptKeyVault.AccessKeyInKeyVault?.VaultName);
            Assert.AreEqual(expectedConfiguration.AccessKeyInKeyVault.SecretName, actualConfigurationExceptKeyVault.AccessKeyInKeyVault?.SecretName);
        }
    }

    [Then(@"ICosmosContainerSourceWithTenantLegacyTransition\.MigrateToV3Async should have returned null")]
    public void ThenICosmosContainerSourceWithTenantLegacyTransition_MigrateToVAsyncShouldHaveReturnedNull()
    {
        this.configReturnedFromMigration.Should().BeNull();
    }

    private ITenant GetTenant()
    {
        // We create the tenant afresh each time, to ensure that all relevant Given steps
        // have had the opportunity to update the property bag.
        return new Tenant(
            this.tenantId,
            "MyTestTenant",
            this.tenantProperties);
    }
}