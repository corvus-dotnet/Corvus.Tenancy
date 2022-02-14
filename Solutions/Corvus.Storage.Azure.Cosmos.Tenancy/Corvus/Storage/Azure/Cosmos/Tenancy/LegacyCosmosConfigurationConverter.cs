// <copyright file="LegacyCosmosConfigurationConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.Cosmos.Tenancy;

using System;

using Corvus.Identity.ClientAuthentication.Azure;

/// <summary>
/// Converts legacy v2 Cosmos configuration into the v3 format.
/// </summary>
public static class LegacyCosmosConfigurationConverter
{
    private const string DevelopmentStorageConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    /// <summary>
    /// Converts legacy V2-era configuration settings into the new format introduced in V3.
    /// </summary>
    /// <param name="legacyConfiguration">The old settings to convert.</param>
    /// <returns>The converted settings.</returns>
    public static CosmosContainerConfiguration FromV2ToV3(LegacyV2CosmosContainerConfiguration legacyConfiguration)
    {
        CosmosContainerConfiguration result = new();
        if (string.IsNullOrEmpty(legacyConfiguration.AccountUri) || legacyConfiguration.AccountUri.Equals(DevelopmentStorageConnectionString))
        {
            result.ConnectionStringPlainText = DevelopmentStorageConnectionString;
        }
        else
        {
            if (legacyConfiguration.AccountUri.Contains("AccountKey="))
            {
                // This is a connection string, not an account URI
                result.ConnectionStringPlainText = legacyConfiguration.AccountUri;
            }
            else
            {
                result.AccountUri = legacyConfiguration.AccountUri;

                if (legacyConfiguration.KeyVaultName != null)
                {
                    result.AccessKeyInKeyVault = new KeyVaultSecretConfiguration
                    {
                        VaultName = legacyConfiguration.KeyVaultName,
                        SecretName = legacyConfiguration.AccountKeySecretName
                            ?? throw new InvalidOperationException($"If {nameof(LegacyV2CosmosContainerConfiguration.KeyVaultName)} is set in legacy configuration, {nameof(LegacyV2CosmosContainerConfiguration.AccountKeySecretName)} must also be set"),
                    };
                }
            }
        }

        if (legacyConfiguration.DatabaseName is not null)
        {
            result.Database = legacyConfiguration.DatabaseName;
        }

        if (legacyConfiguration.ContainerName is not null)
        {
            result.Container = legacyConfiguration.ContainerName;
        }

        return result;
    }
}