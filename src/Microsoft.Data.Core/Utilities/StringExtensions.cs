// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Core.Utilities
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    [DebuggerStepThrough]
    internal static class StringExtensions
    {
        public static bool EqualsOrdinal(this string s1, string s2)
        {
            return string.Equals(s1, s2, StringComparison.Ordinal);
        }

        public static bool EqualsIgnoreCase(this string s1, string s2)
        {
            return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
        }

        public static string Format(this string s, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, s, args);
        }
    }
}
