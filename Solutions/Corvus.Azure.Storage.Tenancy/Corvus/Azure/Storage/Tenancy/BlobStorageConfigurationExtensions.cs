// <copyright file="BlobStorageConfigurationExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    /// <summary>
    /// Extension methods for storage configuration features for the tenant.
    /// </summary>
    public static class BlobStorageConfigurationExtensions
    {
        private const string AccountKeySecretNameKey = "StorageAccountKeySecretName";
        private const string AccountKeyConfigurationKey = "StorageAccountKeyConfigurationKey";
        private const string DisableTenantIdPrefixKey = "StorageAccountDisableTenantIdPrefixKey";

        /// <summary>
        /// Get the account key secret name for the specified storage account configuration.
        /// </summary>
        /// <param name="configuration">The storage configuration.</param>
        /// <returns>The account key secret name.</returns>
        public static string GetAccountKeySecretName(this IStorageConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }
            // First, try the configuration specific to this instance
            if (configuration.Properties.TryGet(AccountKeySecretNameKey, out string accountKeySecretName))
            {
                return accountKeySecretName;
            }

            return null;
        }

        /// <summary>
        /// Sets the account key secret name for the specified storage configuration.
        /// </summary>
        /// <param name="configuration">The storage configuration.</param>
        /// <param name="accountKeySecretName">The account key secret name.</param>
        public static void SetAccountKeySecretName(this IStorageConfiguration configuration, string accountKeySecretName)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            configuration.Properties.Set(AccountKeySecretNameKey, accountKeySecretName);
        }

        /// <summary>
        /// Get whether to disable the tenant ID prefix for this account.
        /// </summary>
        /// <param name="configuration">The storage configuration.</param>
        /// <returns>True if the tenant ID prefix should be disabled.</returns>
        public static bool GetDisableTenantIdPrefix(this IStorageConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }
            // First, try the configuration specific to this instance
            if (configuration.Properties.TryGet(DisableTenantIdPrefixKey, out bool disableTenantIdPrefix))
            {
                return disableTenantIdPrefix;
            }

            // Default to false
            return false;
        }

        /// <summary>
        /// Sets whether to disable the tenant ID prefix for this account.
        /// </summary>
        /// <param name="configuration">The storage configuration.</param>
        /// <param name="disableTenantIdPrefix">The account key secret name.</param>
        public static void SetDisableTenantIdPrefix(this IStorageConfiguration configuration, bool disableTenantIdPrefix)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            configuration.Properties.Set(DisableTenantIdPrefixKey, disableTenantIdPrefix);
        }

        /// <summary>
        /// Get the account key configuration key for the specified storage configuration.
        /// </summary>
        /// <param name="configuration">The storage configuration.</param>
        /// <returns>The account key configuration key.</returns>
        public static string GetAccountKeyConfigurationKey(this IStorageConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }
            // First, try the configuration specific to this instance
            if (configuration.Properties.TryGet(AccountKeyConfigurationKey, out string accountKeyConfigurationKey))
            {
                return accountKeyConfigurationKey;
            }

            return null;
        }

        /// <summary>
        /// Sets the account key configuration key for the specified storage configuraiton.
        /// </summary>
        /// <param name="configuration">The storage configuration.</param>
        /// <param name="accountKeyConfigurationKey">The account key  configuration key.</param>
        public static void SetAccountKeyConfigurationKey(this IStorageConfiguration configuration, string accountKeyConfigurationKey)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            configuration.Properties.Set(AccountKeyConfigurationKey, accountKeyConfigurationKey);
        }
    }
}
