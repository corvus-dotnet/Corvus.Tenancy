// <copyright file="TableClientSteps.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Features.TableStorage;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Corvus.Json;
using Corvus.Storage.Azure.TableStorage;
using Corvus.Storage.Azure.TableStorage.Tenancy;
using Corvus.Tenancy.Internal;

using global::Azure.Data.Tables;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using TechTalk.SpecFlow;

[Binding]
public sealed class TableClientSteps : IDisposable
{
    private const string TenantStoragePropertyKey = "MyStorageSettings";
    private const string StorageAccountName = "myaccount";

    private readonly List<(TableConfiguration Configuration, TableClientOptions? ConnectionOptions)> contextsRequested = new();
    private readonly List<(TableConfiguration Configuration, TableClientOptions? ConnectionOptions)> contextsReplaced = new();
    private readonly ServiceProvider serviceProvider;
    private readonly RootTenant tenant;
    private readonly ITableSourceFromDynamicConfiguration tableSource;

    private TableConfiguration? configuration;

    public TableClientSteps()
    {
        this.tableSource = new FakeTableSourceFromDynamicConfiguration(
            this.contextsRequested,
            this.contextsReplaced);
        var services = new ServiceCollection();
        services.AddRequiredTenancyServices();
        this.serviceProvider = services.BuildServiceProvider();

        this.tenant = new RootTenant(this.serviceProvider.GetRequiredService<IPropertyBagFactory>());
    }

    public void Dispose()
    {
        this.serviceProvider.Dispose();
    }

    [Given("I have added table storage configuration to a tenant with a table name of '([^']*)'")]
    public void GivenIHaveAddedTableStorageConfigurationToATenantWithATableNameOf(string tableName)
    {
        this.configuration = new TableConfiguration
        {
            AccountName = StorageAccountName,
            TableName = tableName,
        };

        this.tenant.UpdateProperties(values =>
            values.AddTableStorageConfiguration(TenantStoragePropertyKey, this.configuration));
    }

    [Given("I have added table storage configuration to a tenant without a table name")]
    [When("I have added table storage configuration to a tenant without a table name")]
    public void GivenIHaveAddedTableStorageConfigurationToATenantWithoutATableName()
    {
        this.configuration = new TableConfiguration
        {
            AccountName = StorageAccountName,
        };

        this.tenant.UpdateProperties(values =>
            values.AddTableStorageConfiguration(TenantStoragePropertyKey, this.configuration));
    }

    [When("I get a replacement TableClient for the tenant specifying a table name of '([^']*)'")]
    public async Task WhenIGetAReplacementTableClientForTheTenantSpecifyingATableNameOfAsync(string tableName)
    {
        await this.tableSource.GetReplacementForFailedTableClientFromTenantAsync(
            this.tenant,
            TenantStoragePropertyKey,
            tableName)
            .ConfigureAwait(false);
    }

    [When("I get a replacement TableClient for the tenant without specifying a table name")]
    public async Task WhenIGetAReplacementTableClientForTheTenantWithoutSpecifyingATableNameAsync()
    {
        await this.tableSource.GetReplacementForFailedTableClientFromTenantAsync(
            this.tenant,
            TenantStoragePropertyKey)
            .ConfigureAwait(false);
    }

    [Given("I get a TableClient for the tenant without specifying a table name")]
    [When("I get a TableClient for the tenant without specifying a table name")]
    public async Task WhenIGetATableClientForTheTenantWithoutSpecifyingATableNameAsync()
    {
        await this.tableSource.GetTableClientFromTenantAsync(
            this.tenant,
            TenantStoragePropertyKey)
            .ConfigureAwait(false);
    }

    [Given("I get a TableClient for the tenant specifying a table name of '([^']*)'")]
    [When("I get a TableClient for the tenant specifying a table name of '([^']*)'")]
    public async Task WhenIGetATableClientForTheTenantSpecifyingATableNameOfAsync(string tableName)
    {
        await this.tableSource.GetTableClientFromTenantAsync(
            this.tenant,
            TenantStoragePropertyKey,
            tableName)
            .ConfigureAwait(false);
    }

    [When("I remove the table storage configuration from the tenant")]
    public void WhenIRemoveTheTableStorageConfigurationFromTheTenant()
    {
        this.tenant.UpdateProperties(
            propertiesToRemove: new[] { TenantStoragePropertyKey });
    }

    [Then("the TableClient source should have been given a configuration identical to the original one")]
    public void ThenTheTableClientSourceShouldHaveBeenGivenAConfigurationIdenticalToTheOriginalOne()
    {
        Assert.AreEqual(this.configuration, this.contextsRequested.Single().Configuration);
    }

    [Then("the TableClient source should have been given a configuration based on the original one but with the table name set to '([^']*)'")]
    public void ThenTheTableClientSourceShouldHaveBeenGivenAConfigurationBasedOnTheOriginalOneButWithTheTableNameSetTo(
        string tableName)
    {
        TableConfiguration? expectedConfiguration = this.configuration! with
        {
            TableName = tableName,
        };

        Assert.AreEqual(expectedConfiguration, this.contextsRequested.Single().Configuration);
    }

    [Then("attempting to get the table storage configuration from the tenant throws an InvalidOperationException")]
    public void ThenAttemptingToGetTheTableStorageConfigurationFromTheTenantThrowsAnInvalidOperationException()
    {
        try
        {
            this.tenant.GetTableStorageConfiguration(TenantStoragePropertyKey);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        Assert.Fail("The expected exception was not thrown.");
    }

    [Then("the TableClient source should have been asked to replace a configuration identical to the original one")]
    public void ThenTheTableClientSourceShouldHaveBeenAskedToReplaceAConfigurationIdenticalToTheOriginalOne()
    {
        Assert.AreEqual(this.configuration, this.contextsReplaced.Single().Configuration);
    }

    [Then("the TableClient source should have been asked to replace a configuration based on the original one but with the table name set to '([^']*)'")]
    public void ThenTheTableClientSourceShouldHaveBeenAskedToReplaceAConfigurationBasedOnTheOriginalOneButWithTheTableNameSetTo(
        string tableName)
    {
        TableConfiguration? expectedConfiguration = this.configuration! with
        {
            TableName = tableName,
        };

        Assert.AreEqual(expectedConfiguration, this.contextsReplaced.Single().Configuration);
    }

    private class FakeTableSourceFromDynamicConfiguration : ITableSourceFromDynamicConfiguration
    {
        private readonly List<(TableConfiguration Configuration, TableClientOptions? ConnectionOptions)> contextsRequested;
        private readonly List<(TableConfiguration Configuration, TableClientOptions? ConnectionOptions)> contextsReplaced;

        public FakeTableSourceFromDynamicConfiguration(List<(TableConfiguration Configuration, TableClientOptions? ConnectionOptions)> contextsRequested, List<(TableConfiguration Configuration, TableClientOptions? ConnectionOptions)> contextsReplaced)
        {
            this.contextsRequested = contextsRequested;
            this.contextsReplaced = contextsReplaced;
        }

        public ValueTask<TableClient> GetReplacementForFailedStorageContextAsync(
            TableConfiguration contextConfiguration,
            TableClientOptions? connectionOptions,
            CancellationToken cancellationToken)
        {
            this.contextsReplaced.Add((contextConfiguration, connectionOptions));
            return new ValueTask<TableClient>(new TableClient(new Uri("https://example.com/test")));
        }

        public ValueTask<TableClient> GetStorageContextAsync(
            TableConfiguration contextConfiguration,
            TableClientOptions? connectionOptions,
            CancellationToken cancellationToken)
        {
            this.contextsRequested.Add((contextConfiguration, connectionOptions));
            return new ValueTask<TableClient>(new TableClient(new Uri("https://example.com/test")));
        }
    }
}