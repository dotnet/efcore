// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Data.Relational.Utilities
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendJoin(
            this StringBuilder stringBuilder, IEnumerable<string> values, string separator)
        {
            return stringBuilder.AppendJoin(values, (sb, value) => sb.Append(value), separator);
        }

        public static StringBuilder AppendJoin<T>(
            this StringBuilder stringBuilder, IEnumerable<T> values, Action<StringBuilder, T> joinAction,
            string separator)
        {
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
