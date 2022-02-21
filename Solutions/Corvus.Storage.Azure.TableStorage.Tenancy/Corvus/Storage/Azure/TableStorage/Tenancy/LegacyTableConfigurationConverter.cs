// <copyright file="LegacyTableConfigurationConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.TableStorage.Tenancy;

using System;

using Corvus.Identity.ClientAuthentication.Azure;

/// <summary>
/// Converts legacy v2 configuration into the v3 format.
/// </summary>
public static class LegacyTableConfigurationConverter
{
    /// <summary>
    /// Converts legacy V2-era configuration settings into the new format introduced in V3,
    /// except for <see cref="TableConfiguration.TableName"/>.
    /// </summary>
    /// <param name="legacyConfiguration">The old settings to convert.</param>
    /// <returns>
    /// The converted settings, except for <see cref="TableConfiguration.TableName"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This always leaves <see cref="TableConfiguration.TableName"/> set to null
    /// because it cannot reliably determine the correct value for it. In cases where the
    /// legacy configuration has <see cref="LegacyV2TableConfiguration.DisableTenantIdPrefix"/>
    /// set to false, the real container name will be tenant-dependent.
    /// </para>
    /// </remarks>
    public static TableConfiguration FromV2ToV3(LegacyV2TableConfiguration legacyConfiguration)
    {
        bool isDeveloperStorage =
            legacyConfiguration.AccountName == "UseDevelopmentStorage=true" ||
            legacyConfiguration.AccountName == null;

        if (isDeveloperStorage)
        {
            return new TableConfiguration
            {
                ConnectionStringPlainText = "UseDevelopmentStorage=true",
            };
        }

        bool connectionStringInAccountName = string.IsNullOrWhiteSpace(legacyConfiguration.KeyVaultName);

        return new TableConfiguration
        {
            AccountName = connectionStringInAccountName
                ? null
                : legacyConfiguration.AccountName,
            ConnectionStringPlainText = connectionStringInAccountName
                ? legacyConfiguration.AccountName
                : null,

            AccessKeyInKeyVault = GetAccessKeyInKeyVaultSecretConfigurationIfApplicable(legacyConfiguration),
        };
    }

    private static KeyVaultSecretConfiguration? GetAccessKeyInKeyVaultSecretConfigurationIfApplicable(
        LegacyV2TableConfiguration legacyConfiguration)
    {
        return legacyConfiguration.KeyVaultName is null
            ? null
            : new KeyVaultSecretConfiguration
            {
                VaultName = legacyConfiguration.KeyVaultName,
                SecretName = legacyConfiguration.AccountKeySecretName
                    ?? throw new InvalidOperationException($"If {nameof(LegacyV2TableConfiguration.KeyVaultName)} is set in legacy configuration, {nameof(LegacyV2TableConfiguration.AccountKeySecretName)} must also be set"),
            };
    }
}