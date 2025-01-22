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
using Corvus.Testing.ReqnRoll;

using FluentAssertions;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Reqnroll;

using static Corvus.Tenancy.Specs.Features.Cosmos.CosmosLegacyMigrationStepDefinitions;

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
    private LegacyV2CosmosContainerConfiguration? legacyConfigurationSupplied;

    /// <summary>
    /// V3-style configuration equivalent to <see cref="legacyConfigurationSupplied"/> in the form expected
    /// in tenant properties. Note that this won't necessarily include a container, because
    /// applications might want to store a single per-database config, but use multiple containers
    /// within that.
    /// </summary>
    private CosmosContainerConfiguration? v3ConfigurationSupplied;

    private CosmosContainerConfiguration? v3ConfigurationEquivalentToLegacyWithContainer;

    private CosmosContainerConfiguration? v3ConfigurationEquivalentToV3WithContainer;

    private CosmosContainerConfiguration? configReturnedFromMigration;
    private string? logicalDatabaseNameForLegacy;
    private string? logicalContainerNameForLegacy;
    private string? databaseNameForV3;
    private string? containerNameForV3;
    private string? databaseNameArgument;
    private string? containerNameArgument;

    private Container? cosmosContainer;
    private CosmosContainerConfiguration? configToUseForTest;

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
        ConfigAndArgs,
        DbInConfigContainerInArg,
        DbInConfigContainerInConfigAndArg,
    }

    public enum ExpectedSources
    {
        LogicalNameExact,
        LogicalNameTenanted,
        V2ConfigExact,
        V2ConfigTenanted,
        V3ConfigExact,
    }

    [Given("Cosmos database and container names set in (.*) with tenant-specific names set to '([^']*)'")]
    public void GivenCosmosDatabaseAndContainerNamesSetIn(
        NameLocations nameLocation, bool autoTenantSpecificNamesEnabled)
    {
        if (!autoTenantSpecificNamesEnabled && nameLocation == NameLocations.Args)
        {
            // With the old V2 factory, when database or container names were not in config, they
            // would be derived from the CosmosContainerDefinition, and the
            // TenantCosmosContainerFactory would always convert these to tenanted names. So the
            // only situation in which you could avoid tenant prefix generation was to put the
            // database and container names in configuration, and set DisableTenantIdPrefix to
            // true.
            throw new NotSupportedException("Automatic tenant-specific names can only be disabled when at least one name is stored in configuration");
        }

        ITenant tenant = this.GetTenant();

        this.legacyConfigurationSupplied = this.cosmosBindings.TestLegacyCosmosConfiguration;
        this.legacyConfigurationSupplied!.DisableTenantIdPrefix = !autoTenantSpecificNamesEnabled;
        this.v3ConfigurationSupplied = LegacyCosmosConfigurationConverter.FromV2ToV3(this.legacyConfigurationSupplied);

        // The database and container names as a V2 app would specify them in the
        // CosmosContainerDefinition.
        this.logicalDatabaseNameForLegacy = $"test-corvustenancy-{DateTime.UtcNow:yyyyMMddHHmmss}";
        this.logicalContainerNameForLegacy = $"legacymigration-{Guid.NewGuid()}";

        // We use different names in V3 config so we can tell which config was used
        this.databaseNameForV3 = $"test-corvustenancy-v3-{DateTime.UtcNow:yyyyMMddHHmmss}";
        this.containerNameForV3 = $"legacymigration-v3-{Guid.NewGuid()}";

        // Tenant-specific database and container names in the form that the V2 libraries
        // would generate them either if they are being derived from the logical names
        // because the config doesn't specify them, or they are specified in the config
        // but IsTenend
        string autoTenantedDatabaseName = CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(
            tenant, this.logicalDatabaseNameForLegacy);
        string autoTenantedContainerName = CosmosTenantedContainerNaming.GetTenantSpecificContainerNameFor(
            tenant, this.logicalContainerNameForLegacy);

        (this.databaseNameArgument, this.containerNameArgument) = nameLocation switch
        {
            NameLocations.Args or
            NameLocations.ConfigAndArgs => (this.logicalDatabaseNameForLegacy, this.logicalContainerNameForLegacy),
            NameLocations.DbInConfigContainerInArg or
            NameLocations.DbInConfigContainerInConfigAndArg => (null, this.logicalContainerNameForLegacy),
            _ => (null, null),
        };

        this.legacyConfigurationSupplied.DatabaseName = nameLocation switch
        {
            NameLocations.Args => null,
            _ => this.logicalDatabaseNameForLegacy,
        };
        this.legacyConfigurationSupplied.ContainerName = nameLocation switch
        {
            NameLocations.Config or
            NameLocations.ConfigAndArgs or
            NameLocations.DbInConfigContainerInConfigAndArg => this.logicalContainerNameForLegacy,
            _ => null,
        };

        this.v3ConfigurationEquivalentToLegacyWithContainer = LegacyCosmosConfigurationConverter.FromV2ToV3(this.legacyConfigurationSupplied) with
        {
            Database = autoTenantSpecificNamesEnabled ? autoTenantedDatabaseName : this.logicalDatabaseNameForLegacy,
            Container = autoTenantSpecificNamesEnabled ? autoTenantedContainerName : this.logicalContainerNameForLegacy,
        };

        // When V3 configuration is used, the names are always exact - it's the job of whoever sets
        // up tenant-specific config to choose database and container names.
        this.v3ConfigurationSupplied.Database = nameLocation switch
        {
            NameLocations.Args => null!,
            _ => this.databaseNameForV3,
        };
        this.v3ConfigurationSupplied.Container = nameLocation switch
        {
            NameLocations.Args or
            NameLocations.DbInConfigContainerInArg => null,
            _ => this.containerNameForV3,
        };

        this.v3ConfigurationEquivalentToV3WithContainer = this.v3ConfigurationSupplied.Container is null
            ? this.v3ConfigurationSupplied with { Container = this.containerNameForV3 }
            : this.v3ConfigurationSupplied;
    }

    [Given("the Cosmos database specified in v2 configuration already exists with throughput of (.*)")]
    public async Task GivenTheCosmosDatabaseV2AlreadyExistsWithThroughputOf(int? databaseThroughput)
    {
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.v3ConfigurationEquivalentToLegacyWithContainer!).ConfigureAwait(false);
        await cosmosContainer.Database.Client.CreateDatabaseAsync(
            cosmosContainer.Database.Id,
            databaseThroughput)
            .ConfigureAwait(false);

        this.cosmosBindings.RemoveThisDatabaseOnTestTeardown(cosmosContainer.Database);
    }

    [Given("the Cosmos database specified in v3 configuration already exists with throughput of (.*)")]
    public async Task GivenTheCosmosDatabaseV3AlreadyExistsWithThroughputOf(int? databaseThroughput)
    {
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.v3ConfigurationEquivalentToV3WithContainer!).ConfigureAwait(false);
        await cosmosContainer.Database.Client.CreateDatabaseAsync(
            cosmosContainer.Database.Id,
            databaseThroughput)
            .ConfigureAwait(false);

        this.cosmosBindings.RemoveThisDatabaseOnTestTeardown(cosmosContainer.Database);
    }

    [Given("the Cosmos container specified in v2 configuration already exists with per-database throughput")]
    public async Task GivenTheCosmosContainerV2AlreadyExistsWithPer_DatabaseThroughput()
    {
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.v3ConfigurationEquivalentToLegacyWithContainer!).ConfigureAwait(false);
        await cosmosContainer.Database.CreateContainerAsync(
            cosmosContainer.Id,
            PartitionKeyPath)
            .ConfigureAwait(false);
    }

    [Given("the Cosmos container specified in v3 configuration already exists with per-database throughput")]
    public async Task GivenTheCosmosContainerV3AlreadyExistsWithPer_DatabaseThroughput()
    {
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.v3ConfigurationEquivalentToV3WithContainer!).ConfigureAwait(false);
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
                { configurationKey, this.legacyConfigurationSupplied ?? throw new InvalidOperationException("This test step should not be invoked when this.legacyConfiguration is null") },
            },
            null);
    }

    [Given("the tenant has the property '([^']*)' set to a bogus legacy CosmosConfiguration")]
    public void GivenTheTenantHasThePropertySvSetToABogusLegacyCosmosConfiguration(string configurationKey)
    {
        LegacyV2CosmosContainerConfiguration bogusConfiguration = new()
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
                { configurationKey, this.v3ConfigurationSupplied ?? throw new InvalidOperationException("This test step should not be invoked when this.configuration is null") },
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

        IEnumerable<string>? databaseNames = this.legacyConfigurationSupplied!.DatabaseName is null
            ? new[] { this.databaseNameArgument! }
            : null;
        IEnumerable<(string, string)>? containerNamesAndPartitionKeys = this.legacyConfigurationSupplied.ContainerName is null
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
                Database = this.configReturnedFromMigration.Database ??
                    (this.legacyConfigurationSupplied!.DisableTenantIdPrefix
                        ? this.databaseNameArgument!
                        : CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenant, this.databaseNameArgument!)),
                Container = this.configReturnedFromMigration.Container ??
                (this.legacyConfigurationSupplied!.DisableTenantIdPrefix
                            ? this.containerNameArgument!
                            : CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenant, this.containerNameArgument!)),
            };
            Container cosmosContainer = await this.containerSource.GetStorageContextAsync(configReturnedWithDatabaseAndContainer).ConfigureAwait(false);
            this.cosmosBindings.RemoveThisDatabaseOnTestTeardown(cosmosContainer.Database);
        }
    }

    [Then("under the db with the name from '([^']*)' a new container with the name from '([^']*)' should have been created")]
    [Then("a new Cosmos database and container with names from '([^']*)' and '([^']*)' should have been created")]
    public async Task ThenANewCosmosDatabaseAndContainerWithNamesFromVConfigExactAndShouldHaveBeenCreatedAsync(
        ExpectedSources dbNameFrom, ExpectedSources containerNameFrom)
    {
        ITenant tenant = this.GetTenant();
        string databaseName = this.GetDbNameForSource(dbNameFrom, tenant);
        string containerName = this.GetContainerNameForSource(containerNameFrom, tenant);

        // The easiest way for us to get a client object with which we can inspect the database
        // is to ask Corvus.Tenancy for a Container based on the configuration.
        this.configToUseForTest = this.v3ConfigurationSupplied! with
        {
            Database = databaseName,
            Container = containerName,
        };
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.configToUseForTest).ConfigureAwait(false);
        await cosmosContainer.Database.ReadAsync().ConfigureAwait(false);
        await cosmosContainer.ReadContainerAsync().ConfigureAwait(false);
    }

    [Then("the Cosmos database throughput should match the specified (.*)")]
    public async Task ThenTheCosmosDatabaseThroughputShouldMatchTheSpecifiedThroughput(int? expectedDatabaseThroughput)
    {
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.configToUseForTest!).ConfigureAwait(false);
        int? reportedDatabaseThroughput = await cosmosContainer.Database.ReadThroughputAsync().ConfigureAwait(false);

        Assert.AreEqual(expectedDatabaseThroughput, reportedDatabaseThroughput);
    }

    [Then("the Cosmos container throughput should match the specified (.*)")]
    public async Task ThenTheCosmosContainerThroughputShouldMatchTheSpecifiedThroughput(int? expectedContainerThroughput)
    {
        Container cosmosContainer = await this.containerSource.GetStorageContextAsync(this.configToUseForTest!).ConfigureAwait(false);
        int? reportedContainerThroughput = await cosmosContainer.ReadThroughputAsync().ConfigureAwait(false);

        Assert.AreEqual(expectedContainerThroughput, reportedContainerThroughput);
    }

    [Then("the Cosmos Container object returned should refer to the database with the name from '([^']*)'")]
    public void ThenTheCosmosContainerObjectReturnedShouldReferToTheDatabaseWithTheNameFrom(ExpectedSources dbNameFrom)
    {
        ITenant tenant = this.GetTenant();
        string databaseName = this.GetDbNameForSource(dbNameFrom, tenant);

        Assert.AreEqual(databaseName, this.cosmosContainer!.Database.Id);
    }

    [Then("the Cosmos Container object returned should refer to the container with the name from '([^']*)'")]
    public void ThenTheCosmosContainerObjectReturnedShouldReferToTheContainerWithTheNameFrom(ExpectedSources containerNameFrom)
    {
        ITenant tenant = this.GetTenant();
        string containerName = this.GetContainerNameForSource(containerNameFrom, tenant);

        Assert.AreEqual(containerName, this.cosmosContainer!.Id);
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
        CosmosContainerConfiguration? expectedConfiguration = this.v3ConfigurationSupplied! with
        {
            Database = this.configReturnedFromMigration!.Database,
            Container = this.configReturnedFromMigration.Container,
            AccessKeyInKeyVault = null,
        };
        CosmosContainerConfiguration actualConfigurationExceptKeyVault = this.configReturnedFromMigration with
        {
            AccessKeyInKeyVault = null,
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

    private string GetDbNameForSource(ExpectedSources dbNameFrom, ITenant tenant)
    {
        return dbNameFrom switch
        {
            ExpectedSources.LogicalNameExact => this.databaseNameArgument ?? throw new InvalidOperationException($"Database name source cannot be {ExpectedSources.LogicalNameExact} when the logical name is not being passed as an argument"),
            ExpectedSources.LogicalNameTenanted => CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenant, this.logicalDatabaseNameForLegacy!),
            ExpectedSources.V2ConfigExact => this.legacyConfigurationSupplied?.DatabaseName ?? throw new InvalidOperationException($"Database name source cannot be {ExpectedSources.V2ConfigExact} when V2 config does not specify a database"),
            ExpectedSources.V2ConfigTenanted => CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenant, this.legacyConfigurationSupplied?.DatabaseName ?? throw new InvalidOperationException($"Database name source cannot be {ExpectedSources.V2ConfigExact} when V2 config does not specify a database")),
            ExpectedSources.V3ConfigExact => this.v3ConfigurationSupplied!.Database! ?? throw new InvalidOperationException($"Database name source cannot be {ExpectedSources.V3ConfigExact} when V3 config does not specify a database"),
            _ => throw new InvalidOperationException($"Unexpected database name source: {dbNameFrom}"),
        };
    }

    private string GetContainerNameForSource(ExpectedSources dbNameFrom, ITenant tenant)
    {
        return dbNameFrom switch
        {
            ExpectedSources.LogicalNameExact => this.containerNameArgument ?? throw new InvalidOperationException($"Container name source cannot be {ExpectedSources.LogicalNameExact} when the logical name is not being passed as an argument"),
            ExpectedSources.LogicalNameTenanted => CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenant, this.containerNameArgument ?? throw new InvalidOperationException($"Container name source cannot be {ExpectedSources.LogicalNameTenanted} when the logical name is not being passed as an argument")),
            ExpectedSources.V2ConfigExact => this.legacyConfigurationSupplied?.ContainerName ?? throw new InvalidOperationException($"Container name source cannot be {ExpectedSources.V2ConfigExact} when V2 config does not specify a container"),
            ExpectedSources.V2ConfigTenanted => CosmosTenantedContainerNaming.GetTenantSpecificDatabaseNameFor(tenant, this.legacyConfigurationSupplied?.ContainerName ?? throw new InvalidOperationException($"Database name source cannot be {ExpectedSources.V2ConfigExact} when V2 config does not specify a container")),
            ExpectedSources.V3ConfigExact => this.v3ConfigurationSupplied!.Container ?? throw new InvalidOperationException($"Container name source cannot be {ExpectedSources.V3ConfigExact} when V3 config does not specify a container"),
            _ => throw new InvalidOperationException($"Unexpected database name source: {dbNameFrom}"),
        };
    }
}