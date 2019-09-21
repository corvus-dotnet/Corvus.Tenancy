﻿// <copyright file="TenantNotModifiedException.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The tenant has not been modified.
    /// </summary>
    [Serializable]
    public class TenantNotModifiedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantNotModifiedException"/> class.
        /// </summary>
        public TenantNotModifiedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantNotModifiedException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public TenantNotModifiedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantNotModifiedException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public TenantNotModifiedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantNotModifiedException"/> class.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The serlialization context.</param>
        protected TenantNotModifiedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}