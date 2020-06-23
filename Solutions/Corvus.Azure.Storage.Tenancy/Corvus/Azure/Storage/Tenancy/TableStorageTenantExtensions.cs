// <copyright file="TableStorageTenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Azure.Storage.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Corvus.Tenancy;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Extension methods for storage features for the tenant.
    /// </summary>
    public static class TableStorageTenantExtensions
    {
        /// <summary>
        /// Determines whether the tenant has a table storage configuration.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="definition">The table storage container definition.</param>
        /// <returns>True if there is an explicit configuration set for this table storage container definition.</returns>
        /// <remarks>This can be more efficient than trying to get the property, as it avoids a deserialization.</remarks>
        public static bool HasStorageTableConfiguration(this ITenant tenant, TableStorageTableDefinition definition)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return ((JObject)tenant.Properties)[GetConfigurationKey(definition)] != null;
        }

        /// <summary>
        /// Get the configuration for the specified table definition for a particular tenant.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="definition">The table storage container definition.</param>
        /// <returns>The configuration for the storage account for this tenant.</returns>
        public static TableStorageConfiguration GetTableStorageConfiguration(this ITenant tenant, TableStorageTableDefinition definition)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (tenant.Properties.TryGet(GetConfigurationKey(definition), out TableStorageConfiguration? configuration))
            {
                return configuration;
            }

            throw new ArgumentException($"No table storage configuration was found for definition with table name '{definition.TableName}'");
        }

        /// <summary>
        /// Creates table configuration properties suitable for passing to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </summary>
        /// <param name="values">Existing configuration values to which to append these.</param>
        /// <param name="definition">The definition of the table for which to set the configuration.</param>
        /// <param name="configuration">The configuration to set.</param>
        /// <returns>
        /// Properties to pass to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </returns>
        public static IEnumerable<KeyValuePair<string, object>> AddTableStorageConfiguration(
            this IEnumerable<KeyValuePair<string, object>> values,
            TableStorageTableDefinition definition,
            TableStorageConfiguration configuration)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return values.Append(new KeyValuePair<string, object>(GetConfigurationKey(definition), configuration));
        }

        /// <summary>
        /// Describes how to clear the table configuration or the specified table from tenant
        /// properties in form suitable for passing as the <c>propertiesToRemove</c> argument to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>.
        /// </summary>
        /// <param name="definition">The definition of the table for which to remove the configuration.</param>
        /// <returns>
        /// A single-entry list of properties that can be passed to
        /// <see cref="ITenantStore.UpdateTenantAsync(string, string?, IEnumerable{KeyValuePair{string, object}}?, IEnumerable{string}?)"/>
        /// to remove the storage configuration.
        /// </returns>
        public static IEnumerable<string> RemoveTableStorageConfiguration(this TableStorageTableDefinition definition)
        {
            if (definition is null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return new string[] { GetConfigurationKey(definition) };
        }

        private static string GetConfigurationKey(TableStorageTableDefinition definition)
        {
            return $"StorageConfiguration__Table__{definition.TableName}";
        }
    }
}