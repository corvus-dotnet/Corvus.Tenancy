// <copyright file="BlobStorageTenantLegacyTransitionSettings.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage.Tenancy
{
    /// <summary>
    /// Settings for applications in the process of transitioning from v2 to v3.
    /// </summary>
    public class BlobStorageTenantLegacyTransitionSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether new v3 configurations should be created
        /// automatically when using the v2 to v3 transition features.
        /// </summary>
        public bool ShouldCreateV3Configurations { get; set; }
    }
}