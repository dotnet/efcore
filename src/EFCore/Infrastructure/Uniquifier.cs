// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Provides methods for manipulating string identifiers.
    /// </summary>
    public static class Uniquifier
    {
        /// <summary>
        ///     Creates a unique identifier by appending a number to the given string.
        /// </summary>
        /// <typeparam name="T"> The type of the object the identifier maps to. </typeparam>
        /// <param name="currentIdentifier"> The base identifier. </param>
        /// <param name="otherIdentifiers"> A dictionary where the identifier will be used as a key. </param>
        /// <param name="maxLength"> The maximum length of the identifier. </param>
        /// <returns> A unique identifier. </returns>
        public static string Uniquify<T>(
            [NotNull] string currentIdentifier, [NotNull] IReadOnlyDictionary<string, T> otherIdentifiers, int maxLength)
        {
            var finalIdentifier = Truncate(currentIdentifier, maxLength);
            var suffix = 1;
            while (otherIdentifiers.ContainsKey(finalIdentifier))
            {
                finalIdentifier = Truncate(currentIdentifier, maxLength, suffix++);
            }

            return finalIdentifier;
        }

        /// <summary>
        ///     Creates a unique identifier by appending a number to the given string.
        /// </summary>
        /// <typeparam name="TKey"> The type of the key that contains the identifier. </typeparam>
        /// <typeparam name="TValue"> The type of the object the identifier maps to. </typeparam>
        /// <param name="currentIdentifier"> The base identifier. </param>
        /// <param name="otherIdentifiers"> A dictionary where the identifier will be used as part of the key. </param>
        /// <param name="keySelector"> Creates the key object from an identifier. </param>
        /// <param name="maxLength"> The maximum length of the identifier. </param>
        /// <returns> A unique identifier. </returns>
        public static string Uniquify<TKey, TValue>(
            [NotNull] string currentIdentifier,
            [NotNull] IReadOnlyDictionary<TKey, TValue> otherIdentifiers,
            [NotNull] Func<string, TKey> keySelector,
            int maxLength)
        {
            var finalIdentifier = Truncate(currentIdentifier, maxLength);
            var suffix = 1;
            while (otherIdentifiers.ContainsKey(keySelector(finalIdentifier)))
            {
                finalIdentifier = Truncate(currentIdentifier, maxLength, suffix++);
            }

            return finalIdentifier;
        }

        /// <summary>
        ///     Ensures the given identifier is shorter than the given length by removing the extra characters from the end.
        /// </summary>
        /// <param name="identifier"> The identifier to shorten. </param>
        /// <param name="maxLength"> The maximum length of the identifier. </param>
        /// <param name="uniquifier"> An optional number that will be appended to the identifier. </param>
        /// <returns> The shortened identifier. </returns>
        public static string Truncate([NotNull] string identifier, int maxLength, int? uniquifier = null)
        {
            var uniquifierLength = GetLength(uniquifier);
            var maxNameLength = maxLength - uniquifierLength;

            var builder = new StringBuilder();
            if (identifier.Length <= maxNameLength)
            {
                builder.Append(identifier);
            }
            else
            {
                builder.Append(identifier, 0, maxNameLength - 1);
                builder.Append("~");
            }

            if (uniquifier != null)
            {
                builder.Append(uniquifier.Value);
            }

            return builder.ToString();
        }

        private static int GetLength(int? number)
        {
            if (number == null)
            {
                return 0;
            }

            var length = 0;
            do
            {
                number /= 10;
                length++;
            }
            while (number.Value >= 1);

            return length;
        }
    }
}
