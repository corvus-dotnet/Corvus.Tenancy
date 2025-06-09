// <copyright file="TenantConflictException.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Exceptions
{
    using System;

    /// <summary>
    /// The tenant has not been modified.
    /// </summary>
    [Serializable]
    public class TenantConflictException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantConflictException"/> class.
        /// </summary>
        public TenantConflictException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantConflictException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public TenantConflictException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantConflictException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public TenantConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}