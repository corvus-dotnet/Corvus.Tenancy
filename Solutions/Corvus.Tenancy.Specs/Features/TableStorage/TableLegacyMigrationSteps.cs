// <copyright file="TableLegacyMigrationSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Features.TableStorage;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Corvus.Json;
using Corvus.Storage.Azure.TableStorage;
using Corvus.Storage.Azure.TableStorage.Tenancy;
using Corvus.Testing.ReqnRoll;

using global::Azure;
using global::Azure.Core;
using global::Azure.Data.Tables;
using global::Azure.Data.Tables.Models;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Reqnroll;

[Binding]
public sealed class TableLegacyMigrationSteps : IDisposable
{
    private readonly string tenantId = RootTenant.RootTenantId.CreateChildId(Guid.NewGuid());
    private readonly string logicalTableName = RootTenant.RootTenantId.CreateChildId(Guid.NewGuid());
    private readonly MonitoringPolicy tableClientMonitor = new();
    private readonly List<string> tablesCreatedByTest = new();
    private readonly Azure.Storage.Tenancy.TableStorageConfiguration legacyConfigurationInTenant = new();
    private readonly TableConfiguration v3ConfigurationInTenant = new();
    private readonly TestSettings testStorageOptions;
    private readonly string testStorageConnectionString;
    private readonly IServiceProvider serviceProvider;
    private readonly TableClientOptions tableClientOptions;
    private readonly IPropertyBagFactory pbf;

    private IPropertyBag tenantProperties;
    private ITenant? tenant;
    private TableServiceClient? tableServiceClient;
    private TableClient? tableClientFromTestSubject;
    private TableConfiguration? v3ConfigFromMigration;

    public TableLegacyMigrationSteps(
        ScenarioContext scenarioContext)
    {
        this.serviceProvider = ContainerBindings.GetServiceProvider(scenarioContext);
        this.testStorageOptions = this.serviceProvider.GetRequiredService<TestSettings>();
        this.testStorageConnectionString = string.IsNullOrWhiteSpace(this.testStorageOptions.AzureStorageConnectionString)
            ? "UseDevelopmentStorage=true"
            : this.testStorageOptions.AzureStorageConnectionString;

        this.pbf = this.serviceProvider.GetRequiredService<IPropertyBagFactory>();
        this.tenantProperties = this.pbf.Create(PropertyBagValues.Empty);

        this.tableClientOptions = new();
        this.tableClientOptions.AddPolicy(this.tableClientMonitor, HttpPipelinePosition.PerCall);
    }

    // Annoyingly, SpecFlow currently doesn't support IAsyncDisposable
    public void Dispose()
    {
        IEnumerable<string> tablesToDelete = this.tableClientMonitor.TablesCreated
            .Concat(this.tablesCreatedByTest);

        foreach (string tableName in tablesToDelete)
        {
            TableClient? tableClient = this.tableServiceClient!.GetTableClient(tableName);
            tableClient.Delete();
        }
    }

    [Given("a legacy TableConfiguration with an AccountName and a TableName with DisableTenantIdPrefix of (true|false)")]
    public void GivenALegacyTableConfigurationWithAnAccountNameAndATableNameWithDisableTenantIdPrefixOfTrue(
        bool disableTenantIdPrefix)
    {
        // AccountName is interpreted as a connection string when there's no AccountKeySecretName
        this.legacyConfigurationInTenant.AccountName = this.testStorageConnectionString;
        this.legacyConfigurationInTenant.TableName = AzureTableNaming.HashAndEncodeTableName(Guid.NewGuid().ToString());
        this.legacyConfigurationInTenant.DisableTenantIdPrefix = disableTenantIdPrefix;
    }

    [Given("a legacy TableConfiguration with an AccountName")]
    public void GivenALegacyTableConfigurationWithAnAccountName()
    {
        // AccountName is interpreted as a connection string when there's no AccountKeySecretName
        this.legacyConfigurationInTenant.AccountName = this.testStorageConnectionString;
    }

    [Given("a legacy TableConfiguration with a bogus AccountName and a TableName with DisableTenantIdPrefix of (true|false)")]
    public void GivenALegacyTableConfigurationWithABogusAccountNameAndATableNameWithDisableTenantIdPrefixOfTrue(
        bool disableTenantIdPrefix)
    {
        this.legacyConfigurationInTenant.AccountName = "nonsense";
        this.legacyConfigurationInTenant.TableName = AzureTableNaming.HashAndEncodeTableName(Guid.NewGuid().ToString());
        this.legacyConfigurationInTenant.DisableTenantIdPrefix = disableTenantIdPrefix;
    }

    [Given("a legacy TableConfiguration with a bogus AccountName")]
    public void GivenALegacyTableConfigurationWithABogusAccountName()
    {
        this.legacyConfigurationInTenant.AccountName = "nonsense";
    }

    [Given("this test is using an Azure TableClient with a connection string")]
    public void GivenThisTestIsUsingAnAzureTableClientWithAConnectionString()
    {
        this.tableServiceClient = new TableServiceClient(this.testStorageConnectionString);
    }

    [Given("a tenant with the property '([^']*)' set to the legacy TableConfiguration")]
    public void GivenATenantWithThePropertySetToTheLegacyTableConfiguration(string configurationKey)
    {
        this.tenantProperties = this.pbf.CreateModified(
            this.tenantProperties,
            new Dictionary<string, object>
            {
                { configurationKey, this.legacyConfigurationInTenant },
            },
            null);
    }

    [Given("a tenant with the property '([^']*)' set to the v3 TableConfiguration")]
    public void GivenATenantWithThePropertySetToTheVTableConfiguration(string configurationKey)
    {
        this.tenantProperties = this.pbf.CreateModified(
            this.tenantProperties,
            new Dictionary<string, object>
            {
                { configurationKey, this.v3ConfigurationInTenant },
            },
            null);
    }

    [Given("a v3 TableConfiguration with a ConnectionStringPlainText and a TableName")]
    public void GivenAVTableConfigurationWithAConnectionStringPlainTextAndATableName()
    {
        this.v3ConfigurationInTenant.ConnectionStringPlainText = this.testStorageConnectionString;
        this.v3ConfigurationInTenant.TableName = AzureTableNaming.HashAndEncodeTableName(Guid.NewGuid().ToString());
    }

    [Given("a v3 TableConfiguration with a ConnectionStringPlainText")]
    public void GivenAVTableConfigurationWithAConnectionStringPlainText()
    {
        this.v3ConfigurationInTenant.ConnectionStringPlainText = this.testStorageConnectionString;
    }

    [Given("a table with a name derived from a tenanted version of the legacy configuration TableName exists")]
    public async Task GivenATableWithATenant_NameDerivedFromTheConfiguredTableNameExistsTenantedAsync()
    {
        string tableName = this.TenantedContainerNameFromLegacyConfiguration();
        TableClient blobContainerClient = this.tableServiceClient!.GetTableClient(tableName);
        await blobContainerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        this.tablesCreatedByTest.Add(tableName);
    }

    [Given("a table with a non-tenant-specific name derived from the legacy configuration TableName exists")]
    public async Task GivenATableWithATenant_NameDerivedFromTheConfiguredTableNameExistsUntenantedAsync()
    {
        string tableName = this.UntenantedContainerNameFromLegacyConfiguration();
        TableClient blobContainerClient = this.tableServiceClient!.GetTableClient(tableName);
        await blobContainerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        this.tablesCreatedByTest.Add(tableName);
    }

    [Given("a table with a tenanted name derived from the logical table name exists")]
    public async Task GivenATableWithANameDerivedFromTheLogicalTableNameExistsTenantedAsync()
    {
        string hashedTenantedContainerName = this.TenantedContainerNameFromLogicalName();
        TableClient tableClient = this.tableServiceClient!.GetTableClient(hashedTenantedContainerName);
        await tableClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        this.tablesCreatedByTest.Add(hashedTenantedContainerName);
    }

    [Given("a table with the name in the V3 configuration exists")]
    public async Task GivenATableWithTheNameInTheV3ConfigurationExistsAsync()
    {
        TableClient blobContainerClient = this.tableServiceClient!.GetTableClient(this.v3ConfigurationInTenant!.TableName!);
        await blobContainerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        this.tablesCreatedByTest.Add(this.v3ConfigurationInTenant!.TableName!);
    }

    [When(@"ITableSourceWithTenantLegacyTransition\.GetTableClientFromTenantAsync is called with configuration keys of '([^']*)' and '([^']*)'")]
    public async Task WhenITableSourceWithTenantLegacyTransition_GetTableClientFromTenantAsyncIsCalledWithConfigurationKeysOfAndAsync(
        string v2ConfigurationKey, string v3ConfigurationKey)
    {
        ITenant tenant = this.GetTenantCreatingIfNecessary();

        // We also create the container at the last minute to enable other tests steps to
        // update the settings object.
        // TODO: probably not necessary now - don't really need the settings?
        ITableSourceWithTenantLegacyTransition tableSource = this.serviceProvider.GetRequiredService<ITableSourceWithTenantLegacyTransition>();

        this.tableClientFromTestSubject = await tableSource.GetTableClientFromTenantAsync(
            tenant,
            v2ConfigurationKey,
            v3ConfigurationKey,
            this.logicalTableName,
            this.tableClientOptions)
            .ConfigureAwait(false);
    }

    [When(@"ITableSourceWithTenantLegacyTransition\.MigrateToV3Async is called with configuration keys of '([^']*)' and '([^']*)'")]
    public async Task WhenITableSourceWithTenantLegacyTransition_MigrateToVAsyncIsCalledWithConfigurationKeysOfAndAsync(
        string v2ConfigurationKey, string v3ConfigurationKey)
    {
        ITenant tenant = this.GetTenantCreatingIfNecessary();

        // We also create the container at the last minute to enable other tests steps to
        // update the settings object.
        // TODO: probably not necessary now - don't really need the settings?
        ITableSourceWithTenantLegacyTransition blobContainerSource = this.serviceProvider.GetRequiredService<ITableSourceWithTenantLegacyTransition>();

        this.v3ConfigFromMigration = await blobContainerSource.MigrateToV3Async(
            tenant,
            v2ConfigurationKey,
            v3ConfigurationKey,
            new[] { this.logicalTableName },
            this.tableClientOptions)
            .ConfigureAwait(false);
    }

    [Then("the TableClient should have access to the table with a tenanted name derived from the legacy configuration TableName")]
    public void ThenTheTableClientShouldHaveAccessToTheTableWithANameDerivedFromTheLegacyConfigurationTableNameTenanted()
    {
        string expectedContainerName = this.TenantedContainerNameFromLegacyConfiguration();
        Assert.AreEqual(expectedContainerName, this.tableClientFromTestSubject!.Name);
    }

    [Then("the TableClient should have access to the table with a non-tenant-specific name derived from the legacy configuration TableName")]
    public void ThenTheTableClientShouldHaveAccessToTheTableWithANameDerivedFromTheLegacyConfigurationTableNameNotTenanted()
    {
        string expectedContainerName = this.UntenantedContainerNameFromLegacyConfiguration();
        Assert.AreEqual(expectedContainerName, this.tableClientFromTestSubject!.Name);
    }

    [Then("the TableClient should have access to the table with a tenanted name derived from the logical table name")]
    public void ThenTheTableClientShouldHaveAccessToTheTableWithANameDerivedFromTheLogicalContainerNameTenanted()
    {
        string expectedContainerName = this.TenantedContainerNameFromLogicalName();
        Assert.AreEqual(expectedContainerName, this.tableClientFromTestSubject!.Name);
    }

    [Then("the TableClient should have access to the table with the name in the V3 configuration")]
    public void ThenTheTableClientShouldHaveAccessToTheTableWithTheNameInTheVConfiguration()
    {
        Assert.AreEqual(this.v3ConfigurationInTenant.TableName, this.tableClientFromTestSubject!.Name);
    }

    [Then(@"ITableSourceWithTenantLegacyTransition\.MigrateToV3Async should have returned a TableConfiguration with these settings")]
    public void ThenITableSourceWithTenantLegacyTransition_MigrateToVAsyncShouldHaveReturnedATableConfigurationWithTheseSettings(
        Table configurationTable)
    {
        TableConfiguration expectedConfiguration = configurationTable.CreateInstance<TableConfiguration>();

        // We recognized some special values in the test for this configuration where we
        // plug in real values at runtime (because the test code can't know what the actual
        // values should be).
        expectedConfiguration.TableName = expectedConfiguration.TableName switch
        {
            "DerivedFromConfiguredTenanted" => this.TenantedContainerNameFromLegacyConfiguration(),
            "DerivedFromConfiguredUntenanted" => this.UntenantedContainerNameFromLegacyConfiguration(),
            "DerivedFromLogicalTenanted" => this.TenantedContainerNameFromLogicalName(),
            _ => expectedConfiguration.TableName,
        };

        if (expectedConfiguration.ConnectionStringPlainText == "testAccountConnectionString")
        {
            expectedConfiguration.ConnectionStringPlainText = this.testStorageConnectionString;
        }

        Assert.AreEqual(this.v3ConfigFromMigration, expectedConfiguration);
    }

    [Then(@"ITableSourceWithTenantLegacyTransition\.MigrateToV3Async should have returned null")]
    public void ThenITableSourceWithTenantLegacyTransition_MigrateToVAsyncShouldHaveReturnedNull()
    {
        Assert.IsNull(this.v3ConfigFromMigration);
    }

    [Then("no new table should have been created")]
    public void ThenNoNewTableShouldHaveBeenCreated()
    {
        Assert.IsEmpty(this.tableClientMonitor.TablesCreated);
    }

    [Then("a new table with a tenanted name derived from the legacy configuration TableName should have been created")]
    public async Task ThenANewTableWithANameDerivedFromTheLegacyConfigurationTableNameShouldHaveBeenCreatedTenantedAsync()
    {
        string expectedContainerName = this.TenantedContainerNameFromLegacyConfiguration();
        await this.CheckTableExists(expectedContainerName).ConfigureAwait(false);
    }

    [Then("a new table with a non-tenant-specific name derived from the legacy configuration TableName should have been created")]
    public async Task ThenANewTableWithANameDerivedFromTheLegacyConfigurationTableNameShouldHaveBeenCreatedUntenantedAsync()
    {
        string expectedContainerName = this.UntenantedContainerNameFromLegacyConfiguration();
        await this.CheckTableExists(expectedContainerName).ConfigureAwait(false);
    }

    [Then("a new table with a tenanted name derived from the logical table name should have been created")]
    public async Task ThenANewTableWithANameDerivedFromTheLogicalTableNameShouldHaveBeenCreatedTenantedAsync()
    {
        await this.CheckTableExists(this.TenantedContainerNameFromLogicalName()).ConfigureAwait(false);
    }

    private ITenant GetTenantCreatingIfNecessary()
    {
        // We create the tenant at the last minute, to ensure that all relevant Given steps
        // have had the opportunity to update the property bag.
        return this.tenant ??= new Tenant(
            this.tenantId,
            "MyTestTenant",
            this.tenantProperties);
    }

    private async Task CheckTableExists(string tableName)
    {
        TableClient blobContainer = this.tableServiceClient!.GetTableClient(tableName);
        AsyncPageable<TableItem> response = this.tableServiceClient.QueryAsync(ti => ti.Name == tableName);
        bool tableExists = await response.AnyAsync().ConfigureAwait(false);
        Assert.IsTrue(tableExists, $"Table {tableName} exists");
    }

    private string TenantedContainerNameFromLogicalName() =>
        AzureTablesTenantedNaming.GetHashedTenantedTableNameFor(this.GetTenantCreatingIfNecessary(), this.logicalTableName);

    private string UntenantedContainerNameFromLegacyConfiguration()
    {
        string baseName = this.legacyConfigurationInTenant!.TableName ?? throw new InvalidOperationException("this.legacyConfigurationInTenant.TableName should not be null at this point in the test");
        return AzureTableNaming.HashAndEncodeTableName(baseName);
    }

    private string TenantedContainerNameFromLegacyConfiguration()
    {
        string baseName = this.legacyConfigurationInTenant!.TableName ?? throw new InvalidOperationException("this.legacyConfigurationInTenant.TableName should not be null at this point in the test");
        string unhashedName = $"{this.tenantId.ToLowerInvariant()}-{baseName}";
        return AzureTableNaming.HashAndEncodeTableName(unhashedName);
    }

    private class MonitoringPolicy : global::Azure.Core.Pipeline.HttpPipelineSynchronousPolicy
    {
        public List<string> ContainerCreateIfExistsCalls { get; } = new List<string>();

        public List<string> TablesCreated { get; } = new List<string>();

        public override void OnReceivedResponse(HttpMessage message)
        {
            if (message.Request.Method.Equals(RequestMethod.Post))
            {
                bool isDevStorage = message.Request.Uri.Host == "127.0.0.1" && message.Request.Uri.Port == 10002;
                string path = isDevStorage
                    ? message.Request.Uri.Path["devstoreaccount1/".Length..]
                    : message.Request.Uri.Path;

                if (path == "Tables")
                {
                    MemoryStream bodyStream = new();
                    message.Request.Content!.WriteTo(bodyStream, CancellationToken.None);
                    bodyStream.Position = 0;
                    var bodyJson = JsonDocument.Parse(bodyStream);
                    string tableName = bodyJson.RootElement.GetProperty("TableName").GetString()!;

                    this.ContainerCreateIfExistsCalls.Add(tableName);
                    if (message.Response.Status == 201)
                    {
                        this.TablesCreated.Add(tableName);
                    }
                }
            }

            base.OnReceivedResponse(message);
        }
    }
}