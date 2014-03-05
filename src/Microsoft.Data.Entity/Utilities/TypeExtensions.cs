// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Microsoft.Data.Entity.Utilities
{
    [DebuggerStepThrough]
    internal static class TypeExtensions
    {
        public static Type ElementType(this Type type)
        {
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType
                && (type.GetGenericTypeDefinition() == typeof(IQueryable<>)
                    || type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                return typeInfo.GenericTypeArguments.Single();
            }

            return type;
        }

        public static bool IsNullableType(this Type type)
        {
            var typeInfo = type.GetTypeInfo();

            return !typeInfo.IsValueType
                   || (typeInfo.IsGenericType
                       && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static PropertyInfo GetAnyProperty(this Type type, string name)
        {
            var props = type.GetRuntimeProperties().Where(p => p.Name == name).ToList();
            if (props.Count() > 1)
            {
                throw new AmbiguousMatchException();
            }

            return props.SingleOrDefault();
        }

        public static ConstructorInfo GetDeclaredConstructor(this Type type, Type[] types)
        {
            types = types ?? new Type[0];

            return type.GetTypeInfo().DeclaredConstructors
                .SingleOrDefault(
                    c => !c.IsStatic
                         && c.GetParameters().Select(p => p.ParameterType).SequenceEqual(types));
        }
    }
}
