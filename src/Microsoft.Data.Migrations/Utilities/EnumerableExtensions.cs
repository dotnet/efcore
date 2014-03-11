// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Data.Migrations.Utilities
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
