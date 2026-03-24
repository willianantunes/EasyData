using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NDjango.Admin
{
    /// <summary>
    /// Encodes and decodes composite primary keys for use in URL segments.
    /// Values are comma-separated and URL-encoded.
    /// </summary>
    public static class CompositeKeyEncoder
    {
        private const char Separator = ',';

        /// <summary>
        /// Encodes a list of key-value pairs into a single comma-separated string.
        /// Each value is URL-encoded to handle special characters.
        /// </summary>
        /// <param name="keyParts">The key parts to encode, each as a name/value pair.</param>
        /// <returns>A comma-separated, URL-encoded string representation of the composite key.</returns>
        public static string Encode(IReadOnlyList<KeyValuePair<string, string>> keyParts)
        {
            if (keyParts == null || keyParts.Count == 0)
                throw new ArgumentException("Key parts cannot be null or empty.", nameof(keyParts));

            return string.Join(Separator, keyParts.Select(kv => WebUtility.UrlEncode(kv.Value)));
        }

        /// <summary>
        /// Decodes a comma-separated encoded string back into a dictionary of PK property name to value.
        /// </summary>
        /// <param name="encoded">The encoded composite key string from the URL.</param>
        /// <param name="pkPropNames">The ordered list of PK property names.</param>
        /// <returns>A dictionary mapping each PK property name to its decoded value.</returns>
        public static Dictionary<string, string> Decode(string encoded, IReadOnlyList<string> pkPropNames)
        {
            if (string.IsNullOrEmpty(encoded))
                throw new ArgumentException("Encoded key cannot be null or empty.", nameof(encoded));
            if (pkPropNames == null || pkPropNames.Count == 0)
                throw new ArgumentException("PK property names cannot be null or empty.", nameof(pkPropNames));

            var parts = encoded.Split(Separator);
            if (parts.Length != pkPropNames.Count)
                throw new ArgumentException(
                    $"Expected {pkPropNames.Count} key parts but got {parts.Length}.", nameof(encoded));

            var result = new Dictionary<string, string>();
            for (var i = 0; i < parts.Length; i++) {
                result[pkPropNames[i]] = WebUtility.UrlDecode(parts[i]);
            }
            return result;
        }
    }
}
