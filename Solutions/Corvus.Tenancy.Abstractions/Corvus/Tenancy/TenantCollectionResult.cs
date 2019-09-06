// <copyright file="TenantCollectionResult.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// A result of a request for a collection of tenants.
    /// </summary>
    public class TenantCollectionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantCollectionResult"/> class.
        /// </summary>
        /// <param name="tenants">The collection of tenants produced by the request.</param>
        /// <param name="continuationToken">The continuation token to pass to retrieve the next batch of tenants.</param>
        public TenantCollectionResult(ReadOnlyCollection<ITenant> tenants, string continuationToken)
        {
            this.Tenants = tenants ?? throw new System.ArgumentNullException(nameof(tenants));
            this.ContinuationToken = continuationToken;
        }

        /// <summary>
        /// Gets the batch of tenants.
        /// </summary>
        public ReadOnlyCollection<ITenant> Tenants { get; }

        /// <summary>
        /// Gets the continuation token to pass to the API for the next batch of tenants.
        /// </summary>
        public string ContinuationToken { get; }
    }
}
