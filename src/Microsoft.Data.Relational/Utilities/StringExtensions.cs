// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Relational.Utilities
{
    internal static class StringExtensions
    {
        public static bool EqualsOrdinal(this string x, string y)
        {
            return string.Equals(x, y, StringComparison.Ordinal);
        }
    }
}
