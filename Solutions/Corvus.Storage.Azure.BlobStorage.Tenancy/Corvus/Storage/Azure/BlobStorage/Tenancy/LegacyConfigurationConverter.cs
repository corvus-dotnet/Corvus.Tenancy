// <copyright file="LegacyConfigurationConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage.Tenancy
{
    using System;

    using Corvus.Identity.ClientAuthentication.Azure;

    /// <summary>
    /// Converts legacy v2 configuration into the v3 format.
    /// </summary>
    public static class LegacyConfigurationConverter
    {
        /// <summary>
        /// Converts legacy V2-era configuration settings into the new format introduced in V3,
        /// except for <see cref="BlobContainerConfiguration.Container"/>.
        /// </summary>
        /// <param name="legacyConfiguration">The old settings to convert.</param>
        /// <returns>
        /// The converted settings, except for <see cref="BlobContainerConfiguration.Container"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This always leaves <see cref="BlobContainerConfiguration.Container"/> set to null
        /// because it cannot reliably determine the correct value for it. In cases where the
        /// legacy configuration has <see cref="LegacyV2BlobStorageConfiguration.DisableTenantIdPrefix"/>
        /// set to false, the real container name will be tenant-dependent.
        /// </para>
        /// </remarks>
        public static BlobContainerConfiguration FromV2ToV3(LegacyV2BlobStorageConfiguration legacyConfiguration)
        {
            bool isDeveloperStorage =
                legacyConfiguration.AccountName == "UseDevelopmentStorage=true" ||
                legacyConfiguration.AccountName == null;

            if (isDeveloperStorage)
            {
                return new BlobContainerConfiguration
                {
                    ConnectionStringPlainText = "UseDevelopmentStorage=true",
                };
            }

            bool connectionStringInAccountName = string.IsNullOrWhiteSpace(legacyConfiguration.KeyVaultName);

            return new BlobContainerConfiguration
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
            LegacyV2BlobStorageConfiguration legacyConfiguration)
        {
            return legacyConfiguration.KeyVaultName is null
                ? null
                : new KeyVaultSecretConfiguration
                {
                    VaultName = legacyConfiguration.KeyVaultName,
                    SecretName = legacyConfiguration.AccountKeySecretName
                        ?? throw new InvalidOperationException($"If {nameof(LegacyV2BlobStorageConfiguration.KeyVaultName)} is set in legacy configuration, {nameof(LegacyV2BlobStorageConfiguration.AccountKeySecretName)} must also be set"),
                };
        }
    }
}