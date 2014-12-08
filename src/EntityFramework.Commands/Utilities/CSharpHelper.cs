// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Data.Entity.Commands.Utilities
{
    internal static class CSharpHelper
    {
        private static readonly Dictionary<Type, string> _typeNames = CreateTypeNames();

        private static Dictionary<Type, string> CreateTypeNames()
        {
            return
                new Dictionary<Type, string>
                    {
                        { typeof(bool), "bool" },
                        { typeof(byte), "byte" },
                        { typeof(sbyte), "sbyte" },
                        { typeof(char), "char" },
                        { typeof(short), "short" },
                        { typeof(int), "int" },
                        { typeof(long), "long" },
                        { typeof(ushort), "ushort" },
                        { typeof(uint), "uint" },
                        { typeof(ulong), "ulong" },
                        { typeof(decimal), "decimal" },
                        { typeof(float), "float" },
                        { typeof(double), "double" },
                        { typeof(string), "string" }
                    };
        }

        public static string GetTypeName(this Type type)
        {
            string name;
            var underlyingType = type.UnwrapNullableType().UnwrapEnumType();

            return (_typeNames.TryGetValue(underlyingType, out name) ? name : underlyingType.Name) + (Nullable.GetUnderlyingType(type) != null ? "?" : "");
        }
    }
}
