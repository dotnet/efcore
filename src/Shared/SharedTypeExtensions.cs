// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System
{
    [DebuggerStepThrough]
    internal static class SharedTypeExtensions
    {
        public static Type UnwrapNullableType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;

        public static bool IsNullableType(this Type type)
        {
            var typeInfo = type.GetTypeInfo();

            return !typeInfo.IsValueType
                   || typeInfo.IsGenericType
                   && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsValidEntityType(this Type type)
            => type.GetTypeInfo().IsClass;

        public static Type MakeNullable(this Type type, bool nullable = true)
            => type.IsNullableType() == nullable
                ? type
                : nullable
                    ? typeof(Nullable<>).MakeGenericType(type)
                    : type.UnwrapNullableType();

        public static bool IsNumeric(this Type type)
        {
            type = type.UnwrapNullableType();

            return type.IsInteger()
                || type == typeof(decimal)
                || type == typeof(float)
                || type == typeof(double);
        }

        public static bool IsInteger(this Type type)
        {
            type = type.UnwrapNullableType();

            return type == typeof(int)
                   || type == typeof(long)
                   || type == typeof(short)
                   || type == typeof(byte)
                   || type == typeof(uint)
                   || type == typeof(ulong)
                   || type == typeof(ushort)
                   || type == typeof(sbyte)
                   || type == typeof(char);
        }

        public static bool IsSignedInteger(this Type type)
            => type == typeof(int)
                   || type == typeof(long)
                   || type == typeof(short)
                   || type == typeof(sbyte);

        public static bool IsAnonymousType(this Type type)
            => type.Name.StartsWith("<>")
               && type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: false).Length > 0
               && type.Name.Contains("AnonymousType");

        public static bool IsTupleType(this Type type)
        {
            if (type == typeof(Tuple))
            {
                return true;
            }

            if (type.IsGenericType)
            {
                var genericDefinition = type.GetGenericTypeDefinition();
                if (genericDefinition == typeof(Tuple<>)
                    || genericDefinition == typeof(Tuple<,>)
                    || genericDefinition == typeof(Tuple<,,>)
                    || genericDefinition == typeof(Tuple<,,,>)
                    || genericDefinition == typeof(Tuple<,,,,>)
                    || genericDefinition == typeof(Tuple<,,,,,>)
                    || genericDefinition == typeof(Tuple<,,,,,,>)
                    || genericDefinition == typeof(Tuple<,,,,,,,>)
                    || genericDefinition == typeof(Tuple<,,,,,,,>))
                {
                    return true;
                }
            }

            return false;
        }

        public static PropertyInfo GetAnyProperty(this Type type, string name)
        {
            var props = type.GetRuntimeProperties().Where(p => p.Name == name).ToList();
            if (props.Count > 1)
            {
                throw new AmbiguousMatchException();
            }

            return props.SingleOrDefault();
        }

        public static bool IsInstantiable(this Type type) => IsInstantiable(type.GetTypeInfo());

        private static bool IsInstantiable(TypeInfo type)
            => !type.IsAbstract
               && !type.IsInterface
               && (!type.IsGenericType || !type.IsGenericTypeDefinition);

        public static bool IsGrouping(this Type type) => IsGrouping(type.GetTypeInfo());

        private static bool IsGrouping(TypeInfo type)
            => type.IsGenericType
               && (type.GetGenericTypeDefinition() == typeof(IGrouping<,>)
                   || type.GetGenericTypeDefinition() == typeof(IAsyncGrouping<,>));

        public static Type UnwrapEnumType(this Type type)
        {
            var isNullable = type.IsNullableType();
            var underlyingNonNullableType = isNullable ? type.UnwrapNullableType() : type;
            if (!underlyingNonNullableType.GetTypeInfo().IsEnum)
            {
                return type;
            }

            var underlyingEnumType = Enum.GetUnderlyingType(underlyingNonNullableType);
            return isNullable ? MakeNullable(underlyingEnumType) : underlyingEnumType;
        }

        public static Type GetSequenceType(this Type type)
        {
            var sequenceType = TryGetSequenceType(type);
            if (sequenceType == null)
            {
                // TODO: Add exception message
                throw new ArgumentException();
            }

            return sequenceType;
        }

        public static Type TryGetSequenceType(this Type type)
            => type.TryGetElementType(typeof(IEnumerable<>))
               ?? type.TryGetElementType(typeof(IAsyncEnumerable<>));

        public static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
        {
            if (type.GetTypeInfo().IsGenericTypeDefinition)
            {
                return null;
            }

            var types = GetGenericTypeImplementations(type, interfaceOrBaseType);

            Type singleImplementation = null;
            foreach (var impelementation in types)
            {
                if (singleImplementation == null)
                {
                    singleImplementation = impelementation;
                }
                else
                {
                    singleImplementation = null;
                    break;
                }
            }

            return singleImplementation?.GetTypeInfo().GenericTypeArguments.FirstOrDefault();
        }

        public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericTypeDefinition)
            {
                var baseTypes = interfaceOrBaseType.GetTypeInfo().IsInterface
                    ? typeInfo.ImplementedInterfaces
                    : type.GetBaseTypes();
                foreach (var baseType in baseTypes)
                {
                    if (baseType.GetTypeInfo().IsGenericType
                        && baseType.GetGenericTypeDefinition() == interfaceOrBaseType)
                    {
                        yield return baseType;
                    }
                }

                if (type.GetTypeInfo().IsGenericType
                    && type.GetGenericTypeDefinition() == interfaceOrBaseType)
                {
                    yield return type;
                }
            }
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

        public static IEnumerable<Type> GetTypesInHierarchy(this Type type)
        {
            while (type != null)
            {
                yield return type;

                type = type.GetTypeInfo().BaseType;
            }
        }

        public static ConstructorInfo GetDeclaredConstructor(this Type type, Type[] types)
        {
            types = types ?? Array.Empty<Type>();

            return type.GetTypeInfo().DeclaredConstructors
                .SingleOrDefault(
                    c => !c.IsStatic
                         && c.GetParameters().Select(p => p.ParameterType).SequenceEqual(types));
        }

        public static IEnumerable<PropertyInfo> GetPropertiesInHierarchy(this Type type, string name)
        {
            do
            {
                var typeInfo = type.GetTypeInfo();
                foreach (var propertyInfo in typeInfo.DeclaredProperties)
                {
                    if (propertyInfo.Name.Equals(name, StringComparison.Ordinal)
                        && !(propertyInfo.GetMethod ?? propertyInfo.SetMethod).IsStatic)
                    {
                        yield return propertyInfo;
                    }
                }

                type = typeInfo.BaseType;
            }
            while (type != null);
        }

        public static IEnumerable<MemberInfo> GetMembersInHierarchy(this Type type)
        {
            do
            {
                // Do the whole hierarchy for properties first since looking for fields is slower.
                foreach (var propertyInfo in type.GetRuntimeProperties().Where(pi => !(pi.GetMethod ?? pi.SetMethod).IsStatic))
                {
                    yield return propertyInfo;
                }

                foreach (var fieldInfo in type.GetRuntimeFields().Where(f => !f.IsStatic))
                {
                    yield return fieldInfo;
                }

                type = type.BaseType;
            }
            while (type != null);
        }

        public static IEnumerable<MemberInfo> GetMembersInHierarchy(this Type type, string name)
            => type.GetMembersInHierarchy().Where(m => m.Name == name);

        private static readonly Dictionary<Type, object> _commonTypeDictionary = new Dictionary<Type, object>
        {
#pragma warning disable IDE0034 // Simplify 'default' expression - default causes default(object)
            { typeof(int), default(int) },
            { typeof(Guid), default(Guid) },
            { typeof(DateTime), default(DateTime) },
            { typeof(DateTimeOffset), default(DateTimeOffset) },
            { typeof(long), default(long) },
            { typeof(bool), default(bool) },
            { typeof(double), default(double) },
            { typeof(short), default(short) },
            { typeof(float), default(float) },
            { typeof(byte), default(byte) },
            { typeof(char), default(char) },
            { typeof(uint), default(uint) },
            { typeof(ushort), default(ushort) },
            { typeof(ulong), default(ulong) },
            { typeof(sbyte), default(sbyte) }
#pragma warning restore IDE0034 // Simplify 'default' expression
        };

        public static object GetDefaultValue(this Type type)
        {
            if (!type.GetTypeInfo().IsValueType)
            {
                return null;
            }

            // A bit of perf code to avoid calling Activator.CreateInstance for common types and
            // to avoid boxing on every call. This is about 50% faster than just calling CreateInstance
            // for all value types.
            return _commonTypeDictionary.TryGetValue(type, out var value)
                ? value
                : Activator.CreateInstance(type);
        }

        public static IEnumerable<TypeInfo> GetConstructibleTypes(this Assembly assembly)
            => assembly.GetLoadableDefinedTypes().Where(
                t => !t.IsAbstract
                     && !t.IsGenericTypeDefinition);

        public static IEnumerable<TypeInfo> GetLoadableDefinedTypes(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).Select(IntrospectionExtensions.GetTypeInfo);
            }
        }
    }
}
