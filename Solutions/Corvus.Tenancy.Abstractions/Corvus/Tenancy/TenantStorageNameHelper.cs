// <copyright file="TenantStorageNameHelper.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    /// <summary>
    /// Methods for building strings used in tenanted storage.
    /// </summary>
    public static class TenantStorageNameHelper
    {
        /// <summary>
        /// Gets the name of the property used to store configuration for a named storage
        /// context of a particular type in tenant properties.
        /// </summary>
        /// <typeparam name="TConfiguration">
        /// The configuration type to store.
        /// </typeparam>
        /// <param name="contextName">
        /// The name of the storage context for which to store configuration.
        /// </param>
        /// <returns>
        /// The name of the tenant property in which these storage settings should be stored.
        /// </returns>
        public static string GetStorageContextConfigurationPropertyName<TConfiguration>(string contextName)
            => typeof(TConfiguration).Name + "__" + contextName;
    }
}
