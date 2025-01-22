// <copyright file="TenantedNameBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings;

using System;
using System.Collections.Generic;

using Corvus.Json;
using Corvus.Tenancy.Internal;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Reqnroll;

[Binding]
public sealed class TenantedNameBindings : IDisposable
{
    private readonly Dictionary<string, ITenant> tenants = new();
    private readonly Dictionary<string, string> physicalContainerNames = new();
    private readonly ServiceProvider serviceProvider;
    private readonly IPropertyBagFactory propertyBagFactory;

    public TenantedNameBindings()
    {
        ServiceCollection services = new();
        services.AddRequiredTenancyServices();
        this.serviceProvider = services.BuildServiceProvider();

        this.propertyBagFactory = this.serviceProvider.GetRequiredService<IPropertyBagFactory>();
    }

    public IReadOnlyDictionary<string, ITenant> Tenants => this.tenants;

    public void Dispose()
    {
        this.serviceProvider.Dispose();
    }

    public void AddTenantedContainerName(string resultLabel, string tenantedContainerName)
    {
        this.physicalContainerNames.Add(resultLabel, tenantedContainerName);
    }

    [Given("a tenant labelled '([^']*)' with id '([^']*)'")]
    public void GivenATenantLabelledWithId(string tenantLabel, string tenantId)
    {
        this.tenants.Add(
            tenantLabel,
            new Tenant(tenantId, tenantLabel, this.propertyBagFactory.Create(PropertyBagValues.Empty)));
    }

    [Given("a tenant labelled '([^']*)'")]
    public void GivenATenantLabelled(string tenantLabel)
    {
        this.GivenATenantLabelledWithId(tenantLabel, Guid.NewGuid().ToString("N"));
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