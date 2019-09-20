// <copyright file="TenantExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Tenancy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

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
        public static string GetParentId(this ITenant tenant)
        {
            if (tenant is null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            return GetParentId(tenant.Id);
        }

        /// <summary>
        /// Creates an ID for a child from the parent Id and a unique ID.
        /// </summary>
        /// <param name="parentId">The full parent ID.</param>
        /// <returns>The combined ID for the child.</returns>
        public static string CreateChildId(this string parentId)
        {
            if (parentId == RootTenant.RootTenantId)
            {
                return EncodeGuids(true, new Guid[0]);
            }

            Guid[] guids = DecodeGuids(parentId);
            return EncodeGuids(true, guids);
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
                results[i] = EncodeGuids(guids.Slice(0, i + 1));
            }

            return results;
        }

        /// <summary>
        /// Gets the id of the parent of a tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>The ID of the parent of the specified tenant, or null if this is the <see cref="ITenantProvider.Root"/> tenant ID.</returns>
        public static string GetParentId(this string tenantId)
        {
            if (tenantId is null)
            {
                throw new ArgumentNullException(nameof(tenantId));
            }

            if (tenantId == RootTenant.RootTenantId)
            {
                return null;
            }

            Span<Guid> guids = DecodeGuids(tenantId);

            if (guids.Length == 1)
            {
                return RootTenant.RootTenantId;
            }

            return EncodeGuids(guids.Slice(0, guids.Length - 1));
        }

        /// <summary>
        /// Encodes a path of guids as a tenant ID.
        /// </summary>
        /// <param name="guids">The guids to encode.</param>
        /// <returns>A string representing the combined and encoded tenant ID.</returns>
        public static string EncodeGuids(params Guid[] guids)
        {
            return EncodeGuids(false, guids);
        }

        /// <summary>
        /// Encodes a path of guids as a tenant ID.
        /// </summary>
        /// <param name="guid">The guid to encode.</param>
        /// <returns>A string representing the combined and encoded tenant ID.</returns>
        public static string EncodeGuid(Guid guid)
        {
            return EncodeGuids(false, guid);
        }

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
        /// <param name="appendAdditionalGuid">Whether to append an additional guid.</param>
        /// <param name="guids">The guids to encode.</param>
        /// <returns>A string representing the combined and encoded tenant ID.</returns>
        private static string EncodeGuids(bool appendAdditionalGuid, params Guid[] guids)
        {
            int length = guids.Length + (appendAdditionalGuid ? 1 : 0);
            byte[] guidBytes = new byte[length * 16];
            int index = 0;

            foreach (Guid guid in guids)
            {
                guid.ToByteArray().CopyTo(guidBytes, index);
                index += 16;
            }

            if (appendAdditionalGuid)
            {
                Guid.NewGuid().ToByteArray().CopyTo(guidBytes, index);
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
            if (encodedGuids == null)
            {
                throw new ArgumentNullException(nameof(encodedGuids));
            }

            byte[] guidBytes = HexadecimalStringToByteArray(encodedGuids);

            if (guidBytes.Length % 16 != 0)
            {
                throw new InvalidOperationException("The byte length should be a multple of 16.");
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
