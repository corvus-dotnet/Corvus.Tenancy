// <copyright file="LegacyBlobStorageConfiguration.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Storage.Azure.BlobStorage.Tenancy.Internal
{
    using Corvus.Tenancy;

    /// <summary>
    /// Enables this project to read configuration in the legacy v2 <c>BlobStorageConfiguration</c>
    /// format without imposing a dependency on the old v2 components.
    /// </summary>
    internal class LegacyBlobStorageConfiguration
    {
        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        /// <remarks>If the account key secret name is empty, then this should contain a complete connection string.</remarks>
        public string? AccountName { get; set; }

        /// <summary>
        /// Gets or sets the container name. If set, this overrides the name specified when calling
        /// <see cref="BlobStorageTenancyExtensions.GetBlobContainerClientFromTenantWithV2BlobStorageConfigurationAsync(IBlobContainerSourceByConfiguration, ITenant, string)"/>.
        /// </summary>
        public string? Container { get; set; }

        /// <summary>
        /// Gets or sets the access type for the container.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In the v2 API, there were two ways that a container's public access type could be
        /// determined. The configuration object itself might specify it through this property,
        /// but it could also be specified through the <c>BlobStorageContainerDefinition</c>'s
        /// <c>AccessType</c> property. This was part of a concept we ultimately decided was
        /// misguided: the old v2 tenancy libraries could dynamically create new containers for
        /// you the first time you asked for them. One of the problems this caused was that the
        /// definition types (e.g. <c>BlobStorageContainerDefinition</c>) needed to include all of
        /// the information required to be able to create a new container on demand. So with
        /// blob containers, that meant specifying the container's public access type. This was
        /// not a great idea, because it muddied the role of the definition types. These were
        /// primarily logical names, but they also ended up containing the default configuration
        /// settings to use in these auto-container-generation scenarios.
        /// </para>
        /// <para>
        /// With <c>Corvus.Tenancy</c> v3, we have made the decision that the application code must
        /// create all required containers itself when onboarding a new tenant, prior to asking
        /// these libraries for access to those containers. So this is not a setting that should
        /// need to be used when adding new tenants. However, it's possible that an application
        /// will be in a state where tenants were created and configured using v2 libraries, but
        /// for some reason some of the configured containers have not yet been used by that
        /// tenant, and so the on-demand creation of these containers won't have happened yet.
        /// To support migration from v2 to v3, it may be necessary to create tools that read
        /// legacy config and ensure that all relevant containers have been created. (Such a tool
        /// might need to walk the entire tenant tree, and perform some "ensure all configured
        /// containers exist" step for each tenant.)
        /// </para>
        /// </remarks>
        public LegacyBlobContainerPublicAccessType? AccessType { get; set; }

        /// <summary>
        /// Gets or sets the key value name.
        /// </summary>
        public string? KeyVaultName { get; set; }

        /// <summary>
        /// Gets or sets the account key secret mame.
        /// </summary>
        public string? AccountKeySecretName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable the tenant ID prefix.
        /// </summary>
        public bool DisableTenantIdPrefix { get; set; }
    }
}
