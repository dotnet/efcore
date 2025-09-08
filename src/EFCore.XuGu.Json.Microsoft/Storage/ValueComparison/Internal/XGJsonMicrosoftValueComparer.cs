// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.ValueComparison.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Storage.ValueComparison.Internal
{
    public static class XGJsonMicrosoftValueComparer
    {
        public static ValueComparer Create(Type t, XGJsonChangeTrackingOptions options = XGJsonChangeTrackingOptions.None)
            => (ValueComparer)Activator.CreateInstance(
                typeof(XGJsonMicrosoftValueComparer<>).MakeGenericType(t),
                options);
    }

    public class XGJsonMicrosoftValueComparer<T> : ValueComparer<T>, IXGJsonValueComparer
    {
        public XGJsonMicrosoftValueComparer(
            XGJsonChangeTrackingOptions options = XGJsonChangeTrackingOptions.None)
            : base(
                CreateEqualsExpression(options),
                CreateHashCodeExpression(options),
                CreateSnapshotExpression(options))
        {
        }

        public ValueComparer Clone(XGJsonChangeTrackingOptions options)
            => new XGJsonMicrosoftValueComparer<T>(options);

        private static Expression<Func<T, T, bool>> CreateEqualsExpression(XGJsonChangeTrackingOptions options)
        {
            var type = typeof(T);
            var param1 = Expression.Parameter(type, "v1");
            var param2 = Expression.Parameter(type, "v2");

            if (options.HasFlag(XGJsonChangeTrackingOptions.CompareRootPropertyOnly))
            {
                return CreateDefaultEqualsExpression();
            }

            if (type == typeof(JsonDocument) ||
                type == typeof(JsonElement))
            {
                if (options.HasFlag(XGJsonChangeTrackingOptions.CompareDomSemantically))
                {
                    var jsonComparerEqualsMethod = typeof(JsonElementComparer).GetRuntimeMethods()
                        .First(
                            m => m.ReturnType == typeof(bool) &&
                                 !m.IsStatic &&
                                 nameof(JsonElementComparer.Equals).Equals(m.Name, StringComparison.Ordinal) &&
                                 m.GetParameters().Length == 2 &&
                                 m.GetParameters()[0].ParameterType == typeof(JsonElement) &&
                                 m.GetParameters()[1].ParameterType == typeof(JsonElement));

                    var jsonDocumentRootElement = type.GetRuntimeProperty(nameof(JsonDocument.RootElement));

                    return Expression.Lambda<Func<T, T, bool>>(
                        Expression.Call(
                            Expression.Constant(JsonElementComparer.Instance, typeof(JsonElementComparer)),
                            jsonComparerEqualsMethod,
                            type == typeof(JsonDocument)
                                ? (Expression)Expression.Property(param1, jsonDocumentRootElement)
                                : param1,
                            type == typeof(JsonDocument)
                                ? (Expression)Expression.Property(param2, jsonDocumentRootElement)
                                : param2),
                        param1, param2);
                }

                if (options.HasFlag(XGJsonChangeTrackingOptions.CompareDomRootPropertyByEquals))
                {
                    return CreateDefaultEqualsExpression();
                }
            }

            if (type == typeof(string))
            {
                if (options.HasFlag(XGJsonChangeTrackingOptions.CompareStringRootPropertyByEquals))
                {
                    return CreateDefaultEqualsExpression();
                }

                // Force the string to be parsed, so that e.g. whitespaces are normalized.
                // Just using JsonSerializer with the WriteIndented option will not work, if the source object is
                // already a string.
                // We therefore accomplish this differently by converting the string temorarily to a JsonDocument.
                return (left, right) => XGJsonMicrosoftStringValueConverter.ProcessJsonString(left as string) == XGJsonMicrosoftStringValueConverter.ProcessJsonString(right as string);
            }

            var unwrappedType = type.UnwrapNullableType();
            if (unwrappedType.IsInteger() ||
                unwrappedType == typeof(Guid) ||
                unwrappedType == typeof(bool) ||
                unwrappedType == typeof(decimal) ||
                unwrappedType == typeof(double) ||
                unwrappedType == typeof(float) ||
                unwrappedType == typeof(object))
            {
                return Expression.Lambda<Func<T, T, bool>>(
                    Expression.Equal(param1, param2),
                    param1, param2);
            }

            // The following includes IEquatable<T> implementations.
            var typedEquals = type.GetRuntimeMethods()
                .FirstOrDefault(
                    m => m.ReturnType == typeof(bool) &&
                         !m.IsStatic &&
                         nameof(object.Equals).Equals(m.Name, StringComparison.Ordinal) &&
                         m.GetParameters().Length == 1 &&
                         m.GetParameters()[0].ParameterType == type);

            while (typedEquals == null
                   && type != null)
            {
                var declaredMethods = type.GetTypeInfo().DeclaredMethods;
                typedEquals = declaredMethods.FirstOrDefault(
                    m => m.IsStatic &&
                         m.ReturnType == typeof(bool) &&
                         "op_Equality".Equals(m.Name, StringComparison.Ordinal) &&
                         m.GetParameters().Length == 2 &&
                         m.GetParameters()[0].ParameterType == type &&
                         m.GetParameters()[1].ParameterType == type);

                type = type.BaseType;
            }

            if (typedEquals != null)
            {
                return Expression.Lambda<Func<T, T, bool>>(
                    typedEquals.IsStatic
                        ? Expression.Call(typedEquals, param1, param2)
                        : Expression.Call(param1, typedEquals, param2),
                    param1, param2);
            }

            return (left, right) => object.Equals(JsonSerializer.Serialize(left, (JsonSerializerOptions)null), JsonSerializer.Serialize(right, (JsonSerializerOptions)null));
        }

        private static Expression<Func<T, int>> CreateHashCodeExpression(XGJsonChangeTrackingOptions options)
        {
            var type = typeof(T);

            if (type == typeof(JsonDocument) ||
                type == typeof(JsonElement))
            {
                if (options.HasFlag(XGJsonChangeTrackingOptions.HashDomSemantiallyOptimized) ||
                    options.HasFlag(XGJsonChangeTrackingOptions.HashDomSemantially))
                {
                    var param1 = Expression.Parameter(type, "v1");

                    var jsonComparerGetHashCodeMethod = typeof(JsonElementComparer).GetRuntimeMethods()
                        .First(
                            m => m.ReturnType == typeof(int) &&
                                 !m.IsStatic &&
                                 nameof(JsonElementComparer.GetHashCode).Equals(m.Name, StringComparison.Ordinal) &&
                                 m.GetParameters().Length == 2 &&
                                 m.GetParameters()[0].ParameterType == typeof(JsonElement) &&
                                 m.GetParameters()[1].ParameterType == typeof(int));

                    var paramExpression = type == typeof(JsonDocument)
                        ? (Expression) Expression.Property(
                            param1,
                            type.GetRuntimeProperty(nameof(JsonDocument.RootElement)))
                        : param1;

                    return Expression.Lambda<Func<T, int>>(
                        Expression.Call(
                            Expression.Constant(JsonElementComparer.Instance, typeof(JsonElementComparer)),
                            jsonComparerGetHashCodeMethod,
                            paramExpression,
                            Expression.Constant(
                                options.HasFlag(XGJsonChangeTrackingOptions.HashDomSemantiallyOptimized) ? 3 : -1,
                                typeof(int))),
                        param1);
                }
            }

            return CreateDefaultHashCodeExpression(false);
        }

        private static Expression<Func<T, T>> CreateSnapshotExpression(XGJsonChangeTrackingOptions options)
        {
            var type = typeof(T);

            var unwrappedType = type.UnwrapNullableType();
            if (options.HasFlag(XGJsonChangeTrackingOptions.CompareRootPropertyOnly) ||
                type == typeof(string) ||
                type == typeof(JsonDocument) ||
                type == typeof(JsonElement) ||
                unwrappedType.IsInteger() ||
                unwrappedType == typeof(Guid) ||
                unwrappedType == typeof(bool) ||
                unwrappedType == typeof(decimal) ||
                unwrappedType == typeof(double) ||
                unwrappedType == typeof(float) ||
                unwrappedType == typeof(object))
            {
                return v => v;
            }

            if (options.HasFlag(XGJsonChangeTrackingOptions.SnapshotCallsDeepClone))
            {
                var deepCloneMethod = type.GetRuntimeMethod("DeepClone", Type.EmptyTypes);
                if (deepCloneMethod != null &&
                    (deepCloneMethod.ReturnType == type || deepCloneMethod.ReturnType == typeof(object)))
                {
                    var param1 = Expression.Parameter(type, "v");

                    return Expression.Lambda<Func<T, T>>(
                        deepCloneMethod.ReturnType == typeof(object)
                            ? (Expression)Expression.Convert(
                                Expression.Call(
                                    param1,
                                    deepCloneMethod),
                                type)
                            : Expression.Call(
                                param1,
                                deepCloneMethod),
                        param1);
                }
            }

            if (options.HasFlag(XGJsonChangeTrackingOptions.SnapshotCallsClone))
            {
                // The following includes, but is not limited to, ICloneable implementations.
                // This also includes Array.Clone().
                var cloneMethod = type.GetRuntimeMethod(nameof(ICloneable.Clone), Type.EmptyTypes);
                if (cloneMethod != null &&
                    (cloneMethod.ReturnType == type || cloneMethod.ReturnType == typeof(object)))
                {
                    var param1 = Expression.Parameter(type, "v");

                    return Expression.Lambda<Func<T, T>>(
                        cloneMethod.ReturnType == typeof(object)
                            ? (Expression)Expression.Convert(
                                Expression.Call(
                                    param1,
                                    cloneMethod),
                                type)
                            : Expression.Call(
                                param1,
                                cloneMethod),
                        param1);
                }
            }

            return v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, (JsonSerializerOptions)null), (JsonSerializerOptions)null);
        }

        // REF: Based on https://dotnetfiddle.net/ijrDBZ
        //      For more context, see https://stackoverflow.com/a/60592310/2618319.
        protected class JsonElementComparer : IEqualityComparer<JsonElement>
        {
            private static JsonElementComparer _instance;
            public static JsonElementComparer Instance => _instance ??= new JsonElementComparer();

            public bool Equals(JsonElement x, JsonElement y)
            {
                if (x.ValueKind != y.ValueKind)
                {
                    return false;
                }

                switch (x.ValueKind)
                {
                    case JsonValueKind.Null:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                    case JsonValueKind.Undefined:
                        return true;

                    // Compare the raw values of numbers, and the text of strings.
                    // Note this means that 0.0 will differ from 0.00 -- which may be correct as deserializing either to `decimal` will result in subtly different results.
                    // Newtonsoft's JValue.Compare(JTokenType valueType, object? objA, object? objB) has logic for detecting "equivalent" values,
                    // you may want to examine it to see if anything there is required here.
                    // https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Linq/JValue.cs#L246
                    case JsonValueKind.Number:
                        return x.GetRawText() == y.GetRawText();

                    case JsonValueKind.String:
                        return x.GetString() == y.GetString(); // Do not use GetRawText() here, it does not automatically resolve JSON escape sequences to their corresponding characters.

                    case JsonValueKind.Array:
                        return x.EnumerateArray().SequenceEqual(y.EnumerateArray(), this);

                    case JsonValueKind.Object:
                        {
                            // Surprisingly, JsonDocument fully supports duplicate property names.
                            // I.e. it's perfectly happy to parse {"Value":"a", "Value" : "b"} and will store both
                            // key/value pairs inside the document!
                            // A close reading of https://tools.ietf.org/html/rfc8259#section-4 seems to indicate that
                            // such objects are allowed but not recommended, and when they arise, interpretation of
                            // identically-named properties is order-dependent.
                            // So stably sorting by name then comparing values seems the way to go.
                            var xPropertiesUnsorted = x.EnumerateObject().ToList();
                            var yPropertiesUnsorted = y.EnumerateObject().ToList();
                            if (xPropertiesUnsorted.Count != yPropertiesUnsorted.Count)
                            {
                                return false;
                            }

                            var xProperties = xPropertiesUnsorted.OrderBy(p => p.Name, StringComparer.Ordinal);
                            var yProperties = yPropertiesUnsorted.OrderBy(p => p.Name, StringComparer.Ordinal);

                            foreach (var (px, py) in xProperties.Zip(yProperties, (px, py) => (px, py)))
                            {
                                if (px.Name != py.Name)
                                {
                                    return false;
                                }

                                if (!Equals(px.Value, py.Value))
                                {
                                    return false;
                                }
                            }
                            return true;
                        }

                    default:
                        throw new JsonException($"Unknown JsonValueKind {x.ValueKind}");
                }
            }

            public int GetHashCode(JsonElement obj)
                => GetHashCode(obj, -1);

            public int GetHashCode(JsonElement obj, int maxHashDepth)
            {
                var hash = new HashCode(); // New in .Net core: https://docs.microsoft.com/en-us/dotnet/api/system.hashcode
                ComputeHashCode(obj, ref hash, 0, maxHashDepth);
                return hash.ToHashCode();
            }

            private void ComputeHashCode(JsonElement obj, ref HashCode hash, int depth, int maxHashDepth)
            {
                hash.Add(obj.ValueKind);

                switch (obj.ValueKind)
                {
                    case JsonValueKind.Null:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                    case JsonValueKind.Undefined:
                        break;

                    case JsonValueKind.Number:
                        hash.Add(obj.GetRawText());
                        break;

                    case JsonValueKind.String:
                        hash.Add(obj.GetString());
                        break;

                    case JsonValueKind.Array:
                        if (depth != maxHashDepth)
                        {
                            foreach (var item in obj.EnumerateArray())
                            {
                                ComputeHashCode(item, ref hash, depth + 1, maxHashDepth);
                            }
                        }
                        else
                        {
                            hash.Add(obj.GetArrayLength());
                        }
                        break;

                    case JsonValueKind.Object:
                        foreach (var property in obj.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
                        {
                            hash.Add(property.Name);
                            if (depth != maxHashDepth)
                            {
                                ComputeHashCode(property.Value, ref hash, depth + 1, maxHashDepth);
                            }
                        }
                        break;

                    default:
                        throw new JsonException($"Unknown JsonValueKind {obj.ValueKind}");
                }
            }
        }
    }
}
