// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class TypedRelationalValueBufferFactoryFactory : IRelationalValueBufferFactoryFactory
    {
        private static readonly MethodInfo _getFieldValueMethod
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod("GetFieldValue");

        private static readonly MethodInfo _isDbNullMethod
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod("IsDBNull");

        private struct CacheKey
        {
            public CacheKey(IReadOnlyList<Type> valueTypes, IReadOnlyList<int> indexMap)
            {
                ValueTypes = valueTypes;
                IndexMap = indexMap;
            }

            public IReadOnlyList<Type> ValueTypes { get; }
            public IReadOnlyList<int> IndexMap { get; }

            private bool Equals(CacheKey other)
            {
                return ValueTypes.SequenceEqual(other.ValueTypes)
                       && (IndexMap?.SequenceEqual(other.IndexMap) ?? true);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is CacheKey
                       && Equals((CacheKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ValueTypes.Aggregate(0, (t, v) => (t * 397) ^ v.GetHashCode())
                           ^ IndexMap?.Aggregate(0, (t, v) => (t * 397) ^ v.GetHashCode()) ?? 0;
                }
            }
        }

        private readonly ThreadSafeDictionaryCache<CacheKey, Func<DbDataReader, object[]>> _cache
            = new ThreadSafeDictionaryCache<CacheKey, Func<DbDataReader, object[]>>();

        public virtual IRelationalValueBufferFactory Create(
            IReadOnlyList<Type> valueTypes, IReadOnlyList<int> indexMap)
        {
            Check.NotNull(valueTypes, nameof(valueTypes));

            return new TypedRelationalValueBufferFactory(
                _cache.GetOrAdd(
                    new CacheKey(valueTypes.ToArray(), indexMap),
                    CreateArrayInitializer));
        }

        private static Func<DbDataReader, object[]> CreateArrayInitializer(CacheKey cacheKey)
        {
            var dataReaderParam = Expression.Parameter(typeof(DbDataReader), "dataReader");

            return Expression.Lambda<Func<DbDataReader, object[]>>(
                Expression.NewArrayInit(
                    typeof(object),
                    cacheKey.ValueTypes
                        .Select((type, i) =>
                            CreateGetValueExpression(
                                dataReaderParam,
                                type,
                                Expression.Constant(cacheKey.IndexMap?[i] ?? i)))),
                dataReaderParam)
                .Compile();
        }

        private static Expression CreateGetValueExpression(
            Expression dataReaderExpression, Type type, Expression indexExpression)
        {
            var underlyingTargetMemberType = type.UnwrapNullableType().UnwrapEnumType();

            Expression expression = Expression.Call(
                dataReaderExpression,
                _getFieldValueMethod.MakeGenericMethod(underlyingTargetMemberType),
                indexExpression);

            if (underlyingTargetMemberType != type)
            {
                expression = Expression.Convert(expression, type);
            }

            return Expression.Condition(
                Expression.Call(dataReaderExpression, _isDbNullMethod, indexExpression),
                Expression.Constant(null, typeof(object)),
                Expression.Convert(expression, typeof(object)));
        }
    }
}
