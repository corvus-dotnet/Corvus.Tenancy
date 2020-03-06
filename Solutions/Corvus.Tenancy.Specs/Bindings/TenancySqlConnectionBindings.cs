// <copyright file="TenancySqlConnectionBindings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Specs.Bindings
{
    using System;
    using Corvus.SpecFlow.Extensions;
    using Corvus.Sql.Tenancy;
    using Corvus.Tenancy;
    using Microsoft.Extensions.DependencyInjection;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Specflow bindings to support a tenanted sql connection provider.
    /// </summary>
    [Binding]
    public static class TenancySqlConnectionBindings
    {
        /// <summary>
        /// Set up a tenanted SQL Connection provider for the feature.
        /// </summary>
        /// <param name="featureContext">The feature context.</param>
        /// <remarks>Note that this sets up a resource in Azure and will incur cost. Ensure the corresponding tear down operation is always run, or verify manually after a test run.</remarks>
        [BeforeFeature("@setupTenantedSqlConnection", Order = ContainerBeforeFeatureOrder.ServiceProviderAvailable)]
        public static void SetupSqlConnectionForRootTenant(FeatureContext featureContext)
        {
            IServiceProvider serviceProvider = ContainerBindings.GetServiceProvider(featureContext);
            ITenantProvider tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();

            // You have to get the factory out before you can configure it, or there is no default configuration...
            serviceProvider.GetRequiredService<ITenantSqlConnectionFactory>();
            SqlConfiguration config = tenantProvider.Root.GetDefaultSqlConfiguration() !;

            // Fall back on a local database
            if (string.IsNullOrEmpty(config.ConnectionString) &&
                string.IsNullOrEmpty(config.ConnectionStringSecretName))
            {
                config.IsLocalDatabase = true;
                config.ConnectionString = "Server=(localdb)\\mssqllocaldb;Trusted_Connection=True;MultipleActiveResultSets=true";
                config.DisableTenantIdPrefix = true;
            }

            tenantProvider.Root.SetDefaultSqlConfiguration(config);
        }
    }
}