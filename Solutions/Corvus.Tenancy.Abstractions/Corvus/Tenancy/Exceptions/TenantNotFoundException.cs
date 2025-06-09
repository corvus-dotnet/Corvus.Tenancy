// <copyright file="TenantNotFoundException.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Exceptions
{
    using System;

    /// <summary>
    /// The tenant has not been modified.
    /// </summary>
    [Serializable]
    public class TenantNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantNotFoundException"/> class.
        /// </summary>
        public TenantNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public TenantNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public TenantNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}