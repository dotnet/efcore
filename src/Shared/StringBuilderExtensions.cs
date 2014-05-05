// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Collections.Generic;

namespace System.Text
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendJoin(
            this StringBuilder stringBuilder, IEnumerable<string> values, string separator = ", ")
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
