// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Design.Core.Utilities.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class JsonUtility
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string Serialize([CanBeNull] object obj)
        {
            var sb = new IndentedStringBuilder();
            SerializeObject(obj, sb);
            return sb.ToString();
        }

        private static void SerializeObject(object obj, IndentedStringBuilder sb)
        {
            if (obj == null)
            {
                sb.Append("null");
                return;
            }
            if (obj is string)
            {
                SerializeString(obj as string, sb);
                return;
            }
            if (obj is bool)
            {
                sb.Append(((bool)obj)
                    ? "true"
                    : "false");
                return;
            }
            if (obj is short
                || obj is ushort
                || obj is int
                || obj is uint
                || obj is long
                || obj is ulong
                || obj is decimal
                || obj is float
                || obj is double)
            {
                sb.Append(obj);
                return;
            }
            if (obj is IEnumerable)
            {
                SerializeEnumerable(obj as IEnumerable, sb);
                return;
            }
            if (obj.GetType().GetTypeInfo().IsClass)
            {
                SerializeClass(obj, sb);
                return;
            }

            throw new ArgumentException($"Could not serialize {obj} [{obj.GetType().FullName}]");
        }

        private static void SerializeEnumerable(IEnumerable en, IndentedStringBuilder sb)
        {
            sb.AppendLine("[");
            using (sb.Indent())
            {
                var e = en.GetEnumerator();
                var next = e.MoveNext();
                while (next)
                {
                    SerializeObject(e.Current, sb);
                    if ((next = e.MoveNext()))
                    {
                        sb.AppendLine(",");
                    }
                    else
                    {
                        sb.AppendLine();
                    }
                }
            }
            sb.Append("]");
        }

        private static readonly IDictionary<char, string> _replaces
            = new Dictionary<char, string>
                {
                    {'\n', @"\n"},
                    {'\t', @"\t"},
                    {'\r', @"\r"},
                    {'\f', @"\f"},
                    {'\b', @"\b"},
                    {'"', @"\"""},
                    {'\\', @"\\"}
                };

        private static void SerializeString(string s, IndentedStringBuilder sb)
        {
            sb.Append('"');
            for (var i = 0; i < s.Length; i++)
            {
                if (_replaces.ContainsKey(s[i]))
                {
                    sb.Append(_replaces[s[i]]);
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            sb.Append('"');
        }

        private static void SerializeClass(object obj, IndentedStringBuilder sb)
        {
            sb.AppendLine("{");
            using (sb.Indent())
            {
                var typeInfo = obj.GetType().GetTypeInfo();
                var e = typeInfo.DeclaredProperties.GetEnumerator();
                var next = e.MoveNext();
                while (next)
                {
                    var p = e.Current;
                    SerializeString(p.Name, sb);
                    sb.Append(": ");
                    SerializeObject(p.GetValue(obj), sb);

                    if ((next = e.MoveNext()))
                    {
                        sb.AppendLine(",");
                    }
                    else
                    {
                        sb.AppendLine();
                    }
                }
            }
            sb.Append("}");
        }
    }
}