// <copyright file="ICosmosConfigurationExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Cosmos.Tenancy
{
    /// <summary>
    /// Extension methods for storage configuration features for the tenant.
    /// </summary>
    public static class ICosmosConfigurationExtensions
    {
        private const string AccountKeySecretNameKey = "StorageAccountKeySecretName";
        private const string AccountKeyConfigurationKey = "StorageAccountKeyConfigurationKey";

        /// <summary>
        /// Get the account key secret name for the specified storage configuration.
        /// </summary>
        /// <param name="configuration">The storage configuration.</param>
        /// <returns>The account key secret name.</returns>
        public static string GetStorageAccountKeySecretName(this ICosmosConfiguration configuration)
        {
            // First, try the configuration specific to this instance
            if (configuration.Properties.TryGet(AccountKeySecretNameKey, out string accountKeySecretName))
            {
                return accountKeySecretName;
            }

            return null;
        }

        /// <summary>
        /// Sets the account key secret name for the specified storage configuraiton.
        /// </summary>
        /// <param name="configuration">The storage configuration.</param>
        /// <param name="accountKeySecretName">The account key secret name.</param>
        public static void SetStorageAccountKeySecretName(this ICosmosConfiguration configuration, string accountKeySecretName)
        {
            configuration.Properties.Set(AccountKeySecretNameKey, accountKeySecretName);
        }

        /// <summary>
        /// Get the account key configuration key for the specified storage configuration.
        /// </summary>
        /// <param name="configuration">The storage configuration.</param>
        /// <returns>The account key configuration key.</returns>
        public static string GetStorageAccountKeyConfigurationKey(this ICosmosConfiguration configuration)
        {
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
        public static void SetAccountKeyConfigurationKey(this ICosmosConfiguration configuration, string accountKeyConfigurationKey)
        {
            configuration.Properties.Set(AccountKeyConfigurationKey, accountKeyConfigurationKey);
        }
    }
}