// <copyright file="TenantCollectionResult.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System.Collections.Generic;

    /// <summary>
    /// A result of a request for a collection of tenants.
    /// </summary>
    public class TenantCollectionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantCollectionResult"/> class.
        /// </summary>
        /// <param name="tenantIds">The collection of tenantIds produced by the request.</param>
        /// <param name="continuationToken">The continuation token to pass to retrieve the next batch of tenants.</param>
        public TenantCollectionResult(IEnumerable<string> tenantIds, string continuationToken)
        {
            this.Tenants = new List<string>(tenantIds) ?? throw new System.ArgumentNullException(nameof(tenantIds));
            this.ContinuationToken = continuationToken;
        }

        /// <summary>
        /// Gets the batch of tenants.
        /// </summary>
        public IList<string> Tenants { get; }

        /// <summary>
        /// Gets the continuation token to pass to the API for the next batch of tenants.
        /// </summary>
        public string ContinuationToken { get; }
    }
}
