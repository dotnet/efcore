// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.Data.Migrations.Utilities
{
    [DebuggerStepThrough]
    internal static class EnumerableExtensions
    {
        public static string Join([NotNull] this IEnumerable<object> source, string separator = ", ")
        {
            Check.NotNull(source, "source");

            return string.Join(separator, source);
        }
    }
}
