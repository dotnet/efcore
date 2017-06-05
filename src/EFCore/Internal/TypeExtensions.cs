// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> _builtInTypeNames = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(ushort), "ushort" }
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsDefaultValue([NotNull] this Type type, [CanBeNull] object value)
            => (value == null) || value.Equals(type.GetDefaultValue());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string ShortDisplayName([NotNull] this Type type)
            => type.DisplayName(fullName: false);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string DisplayName([NotNull] this Type type, bool fullName = true)
        {
            var sb = new StringBuilder();
            ProcessTypeName(type, sb, fullName);
            return sb.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static FieldInfo GetFieldInfo([NotNull] this Type type, [NotNull] string fieldName)
            => type.GetRuntimeFields().FirstOrDefault(f => f.Name == fieldName && !f.IsStatic);

        private static void AppendGenericArguments(Type[] args, int startIndex, int numberOfArgsToAppend, StringBuilder sb, bool fullName)
        {
            var totalArgs = args.Length;
            if (totalArgs >= startIndex + numberOfArgsToAppend)
            {
                sb.Append("<");
                for (var i = startIndex; i < startIndex + numberOfArgsToAppend; i++)
                {
                    ProcessTypeName(args[i], sb, fullName);
                    if (i + 1 < startIndex + numberOfArgsToAppend)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(">");
            }
        }

        private static void ProcessTypeName(Type t, StringBuilder sb, bool fullName)
        {
            if (t.GetTypeInfo().IsGenericType)
            {
                ProcessNestedGenericTypes(t, sb, fullName);
                return;
            }
            if (_builtInTypeNames.ContainsKey(t))
            {
                sb.Append(_builtInTypeNames[t]);
            }
            else
            {
                sb.Append(fullName ? t.FullName : t.Name);
            }
        }

        private static void ProcessNestedGenericTypes(Type t, StringBuilder sb, bool fullName)
        {
            var genericFullName = t.GetGenericTypeDefinition().FullName;
            var genericSimpleName = t.GetGenericTypeDefinition().Name;
            var parts = genericFullName.Split('+');
            var genericArguments = t.GetTypeInfo().GenericTypeArguments;
            var index = 0;
            var totalParts = parts.Length;
            if (totalParts == 1)
            {
                var part = parts[0];
                var num = part.IndexOf('`');
                if (num == -1)
                {
                    return;
                }

                var name = part.Substring(0, num);
                var numberOfGenericTypeArgs = int.Parse(part.Substring(num + 1), CultureInfo.InvariantCulture);
                sb.Append(fullName ? name : genericSimpleName.Substring(0, genericSimpleName.IndexOf('`')));
                AppendGenericArguments(genericArguments, index, numberOfGenericTypeArgs, sb, fullName);
                return;
            }
            for (var i = 0; i < totalParts; i++)
            {
                var part = parts[i];
                var num = part.IndexOf('`');
                if (num != -1)
                {
                    var name = part.Substring(0, num);
                    var numberOfGenericTypeArgs = int.Parse(part.Substring(num + 1), CultureInfo.InvariantCulture);
                    if (fullName || (i == totalParts - 1))
                    {
                        sb.Append(name);
                        AppendGenericArguments(genericArguments, index, numberOfGenericTypeArgs, sb, fullName);
                    }
                    if (fullName && (i != totalParts - 1))
                    {
                        sb.Append("+");
                    }
                    index += numberOfGenericTypeArgs;
                }
                else
                {
                    if (fullName || (i == totalParts - 1))
                    {
                        sb.Append(part);
                    }
                    if (fullName && (i != totalParts - 1))
                    {
                        sb.Append("+");
                    }
                }
            }
        }
    }
}
