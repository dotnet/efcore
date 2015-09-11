// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage.Internal;

namespace Microsoft.Data.Entity.Storage
{
    public class UntypedRelationalValueBufferFactoryFactory : IRelationalValueBufferFactoryFactory
    {
        private struct CacheKey
        {
            public CacheKey(IReadOnlyList<Type> valueTypes)
            {
                ValueTypes = valueTypes;
            }

            public IReadOnlyList<Type> ValueTypes { get; }

            private bool Equals(CacheKey other) => ValueTypes.SequenceEqual(other.ValueTypes);

            public override bool Equals(object obj)
                => !ReferenceEquals(null, obj)
                   && (obj is CacheKey
                       && Equals((CacheKey)obj));

            public override int GetHashCode()
            {
                unchecked
                {
                    return ValueTypes.Aggregate(0, (t, v) => (t * 397) ^ v.GetHashCode());
                }
            }
        }

        private readonly ThreadSafeDictionaryCache<CacheKey, Action<object[]>> _cache
            = new ThreadSafeDictionaryCache<CacheKey, Action<object[]>>();

        public virtual IRelationalValueBufferFactory Create(
            IReadOnlyList<Type> valueTypes, IReadOnlyList<int> indexMap)
        {
            var processValuesAction = _cache.GetOrAdd(new CacheKey(valueTypes.ToArray()), CreateValueProcessor);

            return indexMap == null
                ? (IRelationalValueBufferFactory)new UntypedRelationalValueBufferFactory(processValuesAction)
                : new RemappingUntypedRelationalValueBufferFactory(indexMap, processValuesAction);
        }

        private static Action<object[]> CreateValueProcessor(CacheKey cacheKey)
        {
            var valuesParam = Expression.Parameter(typeof(object[]), "values");

            var conversions = new List<Expression>();
            var valueTypes = cacheKey.ValueTypes;

            var valueVariable = Expression.Variable(typeof(object), "value");

            for (var i = 0; i < valueTypes.Count; i++)
            {
                var type = valueTypes[i];

                if (type.UnwrapNullableType().GetTypeInfo().IsEnum)
                {
                    var arrayAccess = Expression.ArrayAccess(valuesParam, Expression.Constant(i));

                    conversions.Add(Expression.Assign(valueVariable, arrayAccess));

                    conversions.Add(
                        Expression.IfThen(
                            Expression.IsFalse(
                                Expression.ReferenceEqual(
                                    valueVariable,
                                    Expression.Constant(DBNull.Value))),
                            Expression.Assign(
                                arrayAccess,
                                Expression.Convert(
                                    Expression.Convert(
                                        Expression.Convert(
                                            valueVariable,
                                            type.UnwrapEnumType()),
                                        type),
                                    typeof(object)))));
                }
            }

            if (conversions.Count == 0)
            {
                return null;
            }

            return Expression.Lambda<Action<object[]>>(
                Expression.Block(
                    new[] { valueVariable },
                    conversions),
                valuesParam)
                .Compile();
        }
    }
}
