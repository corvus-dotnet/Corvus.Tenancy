// <copyright file="TenantConflictException.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The tenant operation has not been completed due to a conflict.
    /// </summary>
    /// <remarks>
    /// For operations that modify an existing tenant, this exception is thrown when the tenant in
    /// storage has been modified since the client started the operation, as determined by a
    /// non-matching ETag. For operations attempting to create a new tenant, this is thrown when
    /// a tenant with the specified ID already exists.
    /// </remarks>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantConflictException"/> class.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The serlialization context.</param>
        protected TenantConflictException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}