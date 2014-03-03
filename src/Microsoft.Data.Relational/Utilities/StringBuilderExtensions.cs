// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.Data.Relational.Utilities
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendJoin(
            [NotNull] this StringBuilder stringBuilder, [NotNull] IEnumerable<string> values, [NotNull] string separator)
        {
            Check.NotNull(stringBuilder, "stringBuilder");
            Check.NotNull(values, "values");
            Check.NotNull(separator, "separator");

            return stringBuilder.AppendJoin(values, (sb, value) => sb.Append(value), separator);
        }

        public static StringBuilder AppendJoin<T>(
            [NotNull] this StringBuilder stringBuilder, [NotNull] IEnumerable<T> values, Action<StringBuilder, T> joinAction,
            [NotNull] string separator)
        {
            Check.NotNull(stringBuilder, "stringBuilder");
            Check.NotNull(values, "values");
            Check.NotNull(joinAction, "joinAction");
            Check.NotNull(separator, "separator");

            var appended = false;

            foreach (var value in values)
            {
                joinAction(stringBuilder, value);
                stringBuilder.Append(separator);
                appended = true;
            }

            if (appended)
            {
                stringBuilder.Length -= separator.Length;
            }

            return stringBuilder;
        }
    }
}
