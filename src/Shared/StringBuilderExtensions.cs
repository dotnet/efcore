// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Text
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendJoin(
            this StringBuilder stringBuilder, IEnumerable<string> values, string separator = ", ")
            => stringBuilder.AppendJoin(values, (sb, value) => sb.Append(value), separator);

        public static StringBuilder AppendJoin(
            this StringBuilder stringBuilder, string separator, params string[] values)
            => stringBuilder.AppendJoin(values, (sb, value) => sb.Append(value), separator);

        public static StringBuilder AppendJoin<T>(
            this StringBuilder stringBuilder,
            IEnumerable<T> values,
            Action<StringBuilder, T> joinAction,
            string separator = ", ")
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

        public static StringBuilder AppendJoin<T, TParam>(
            this StringBuilder stringBuilder,
            IEnumerable<T> values,
            TParam param,
            Action<StringBuilder, T, TParam> joinAction,
            string separator = ", ")
        {
            var appended = false;

            foreach (var value in values)
            {
                joinAction(stringBuilder, value, param);
                stringBuilder.Append(separator);
                appended = true;
            }

            if (appended)
            {
                stringBuilder.Length -= separator.Length;
            }

            return stringBuilder;
        }

        public static StringBuilder AppendJoin<T, TParam1, TParam2>(
            this StringBuilder stringBuilder,
            IEnumerable<T> values,
            TParam1 param1,
            TParam2 param2,
            Action<StringBuilder, T, TParam1, TParam2> joinAction,
            string separator = ", ")
        {
            var appended = false;

            foreach (var value in values)
            {
                joinAction(stringBuilder, value, param1, param2);
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
