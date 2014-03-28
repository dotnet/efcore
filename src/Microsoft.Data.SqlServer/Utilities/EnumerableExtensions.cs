// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics;

namespace System.Collections.Generic
{
    [DebuggerStepThrough]
    internal static class EnumerableExtensions
    {
        public static string Join(this IEnumerable<object> source, string separator = ", ")
        {
            return string.Join(separator, source);
        }
    }
}
