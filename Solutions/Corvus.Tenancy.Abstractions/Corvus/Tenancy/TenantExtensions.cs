// <copyright file="TenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Corvus.Json;

    /// <summary>
    /// Extensions for the <see cref="ITenant"/>.
    /// </summary>
    public static class TenantExtensions
    {
        private static readonly uint[] Lookup32 = CreateLookup32();

        /// <summary>
        /// Gets the id of the parent of a tenant.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <returns>The id of the parent of the specified tenant, or null if this is the <see cref="ITenantProvider.Root"/> tenant.</returns>
        public static string? GetParentId(this ITenant tenant)
        {
            ArgumentNullException.ThrowIfNull(tenant);

            return GetParentId(tenant.Id);
        }

        /// <summary>
        /// Gets the id of the parent of a tenant, throwing an exception if the tenant has no parent (i.e., if
        /// it is the root tenant).
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <returns>The id of the parent of the specified tenant.</returns>
        /// <remarks>
        /// This is for use in scenarios where the caller believes that the tenant ID is not that of the root.
        /// This is common, because the root is often handled as a special case, meaning that by the time we
        /// get to calling this method, we've already ruled out the possibility that there is no parent.
        /// </remarks>
        public static string GetRequiredParentId(this ITenant tenant)
        {
            ArgumentNullException.ThrowIfNull(tenant);

            return GetRequiredParentId(tenant.Id);
        }

        /// <summary>
        /// Creates an ID for a child from the parent Id and a unique ID.
        /// </summary>
        /// <param name="parentId">The full parent ID.</param>
        /// <param name="childTenantGuid">The Guid to use to generate the new child tenant Id.</param>
        /// <returns>The combined ID for the child.</returns>
        public static string CreateChildId(this string parentId, Guid? childTenantGuid)
        {
            childTenantGuid ??= Guid.NewGuid();

            if (parentId == RootTenant.RootTenantId)
            {
                return EncodeGuids(childTenantGuid.Value);
            }

            Guid[] guids = DecodeGuids(parentId);
            var guidsWithChildTenantId = new Guid[guids.Length + 1];
            guids.CopyTo(guidsWithChildTenantId, 0);
            guidsWithChildTenantId[^1] = childTenantGuid.Value;

            return EncodeGuids(guidsWithChildTenantId);
        }

        /// <summary>
        /// Gets the ordered list of tenant paths within an ID.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>An enumerable, starting with the Root tenant ID, of the parents of this tenant.</returns>
        public static IEnumerable<string> GetParentTree(this string id)
        {
            Span<Guid> guids = DecodeGuids(id);
            string[] results = new string[guids.Length];

            for (int i = 0; i < guids.Length; ++i)
            {
                results[i] = EncodeGuids(guids[..(i + 1)]);
            }

            return results;
        }

        /// <summary>
        /// Gets the id of the parent of a tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>The ID of the parent of the specified tenant, or null if this is the <see cref="ITenantProvider.Root"/> tenant ID.</returns>
        public static string? GetParentId(this string tenantId)
        {
            ArgumentNullException.ThrowIfNull(tenantId);

            if (tenantId == RootTenant.RootTenantId)
            {
                return null;
            }

            try
            {
                Span<Guid> guids = DecodeGuids(tenantId);

                if (guids.Length == 1)
                {
                    return RootTenant.RootTenantId;
                }

                return EncodeGuids(guids[0..^1]);
            }
            catch (Exception ex)
            {
                throw new FormatException("Invalid tenantId format.", ex);
            }
        }

        /// <summary>
        /// Gets the id of the parent of a tenant, throwing an exception if the tenant has no parent (i.e., if
        /// it is the root tenant).
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>The ID of the parent of the specified tenant.</returns>
        /// <remarks>
        /// This is for use in scenarios where the caller believes that the tenant ID is not that of the root.
        /// This is common, because the root is often handled as a special case, meaning that by the time we
        /// get to calling this method, we've already ruled out the possibility that there is no parent.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1224:Make method an extension method.", Justification = "Extension methods on basic types such string are evil")]
        public static string GetRequiredParentId(string tenantId) => GetParentId(tenantId) ?? throw new ArgumentException("Must not be root tenant", nameof(tenantId));

        /// <summary>
        /// Encodes a path of guids as a tenant ID.
        /// </summary>
        /// <param name="guid">The guid to encode.</param>
        /// <returns>A string representing the combined and encoded tenant ID.</returns>
        public static string EncodeGuid(this Guid guid)
        {
            return EncodeGuids(guid);
        }

#pragma warning disable RCS1224 // Make method an extension method.
        /// <summary>
        /// Convert a byte array to a hex string.
        /// </summary>
        /// <param name="bytes">The bytes to encode.</param>
        /// <returns>A string composed of pairs of hex digits for the byte array.</returns>
        public static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            uint[] lookup32 = Lookup32;
            char[] result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                uint val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[(2 * i) + 1] = (char)(val >> 16);
            }

            return new string(result);
        }
#pragma warning restore RCS1224 // Make method an extension method.

        /// <summary>
        /// Updates the root tenant's properties, using a callback that produces a collection of
        /// key pair values where the values are all typed as non-null objects to describe the
        /// properties to add or change.
        /// </summary>
        /// <param name="rootTenant">The root tenant to configure.</param>
        /// <param name="builder">A function that builds the collection describing the properties to add.</param>
        /// <param name="propertiesToRemove">Optional list of properties to remove.</param>
        public static void UpdateProperties(
            this RootTenant rootTenant,
            Func<IEnumerable<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>> builder,
            IEnumerable<string>? propertiesToRemove = null)
        {
            rootTenant.UpdateProperties(
                builder(PropertyBagValues.Empty),
                propertiesToRemove);
        }

        private static uint[] CreateLookup32()
        {
            uint[] result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("x2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }

            return result;
        }

        private static byte[] HexadecimalStringToByteArray(string input)
        {
            int outputLength = input.Length / 2;
            byte[] output = new byte[outputLength];
            using (var sr = new StringReader(input))
            {
                for (int i = 0; i < outputLength; i++)
                {
                    output[i] = Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
                }
            }

            return output;
        }

        /// <summary>
        /// Encodes a path of guids as a tenant ID.
        /// </summary>
        /// <param name="guids">The guids to encode.</param>
        /// <returns>A string representing the combined and encoded tenant ID.</returns>
        private static string EncodeGuids(params Guid[] guids)
        {
            int length = guids.Length;
            byte[] guidBytes = new byte[length * 16];
            int index = 0;

            foreach (Guid guid in guids)
            {
                guid.ToByteArray().CopyTo(guidBytes, index);
                index += 16;
            }

            return EncodeGuidBytes(guidBytes);
        }

        /// <summary>
        /// Encodes a path of guids as a tenant ID.
        /// </summary>
        /// <param name="guids">The guids to encode.</param>
        /// <returns>A string representing the combined and encoded tenant ID.</returns>
        private static string EncodeGuids(Span<Guid> guids)
        {
            byte[] guidBytes = new byte[guids.Length * 16];
            int index = 0;

            foreach (Guid guid in guids)
            {
                guid.ToByteArray().CopyTo(guidBytes, index);
                index += 16;
            }

            return EncodeGuidBytes(guidBytes);
        }

        private static string EncodeGuidBytes(byte[] guidBytes)
        {
            return ByteArrayToHexViaLookup32(guidBytes);
        }

        private static Guid[] DecodeGuids(string encodedGuids)
        {
            ArgumentNullException.ThrowIfNull(encodedGuids);

            byte[] guidBytes = HexadecimalStringToByteArray(encodedGuids);

            if (guidBytes.Length % 16 != 0)
            {
                throw new FormatException("The byte length should be a multple of 16.");
            }

            var result = new Guid[guidBytes.Length / 16];

            // In the next version of netstandard, we will be able to create a guid from a Span<bytes> and clear up some more of this
            // allocation
            for (int i = 0; i < result.Length; ++i)
            {
                var guid = new Guid(guidBytes.AsSpan(i * 16, 16).ToArray());
                result[i] = guid;
            }

            return result;
        }
    }
}