// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace System
{
    [DebuggerStepThrough]
    internal static class TypeExtensions
    {
        public static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
        {
            if (!type.GetTypeInfo().IsGenericTypeDefinition)
            {
                var types = GetGenericTypeImplementations(type, interfaceOrBaseType).ToArray();

                return types.Length == 1 ? types[0].GetTypeInfo().GenericTypeArguments.FirstOrDefault() : null;
            }

            return null;
        }

        public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericTypeDefinition)
            {
                return (interfaceOrBaseType.GetTypeInfo().IsInterface ? typeInfo.ImplementedInterfaces : type.GetBaseTypes())
                    .Union(new[] { type })
                    .Where(
                        t => t.GetTypeInfo().IsGenericType
                             && t.GetGenericTypeDefinition() == interfaceOrBaseType);
            }

            return Enumerable.Empty<Type>();
        }

        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            type = type.GetTypeInfo().BaseType;

            while (type != null)
            {
                yield return type;

                type = type.GetTypeInfo().BaseType;
            }
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
