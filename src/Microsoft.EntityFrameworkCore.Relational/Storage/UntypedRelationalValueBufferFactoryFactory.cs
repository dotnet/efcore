// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Creates instances of the <see cref="IRelationalValueBufferFactory" /> type. <see cref="IRelationalValueBufferFactory" />
    ///         instances are tied to a specific result shape. This factory is responsible for creating the
    ///         <see cref="IRelationalValueBufferFactory" /> for a given result shape.
    ///     </para>
    ///     <para>
    ///         This factory results in value buffers that use the untyped <see cref="DbDataReader.GetValues(object[])" /> API to read
    ///         back individual values from the underlying <see cref="DbDataReader" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
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
                => !ReferenceEquals(null, obj) && obj is CacheKey && Equals((CacheKey)obj);

            public override int GetHashCode()
            {
                unchecked
                {
                    return ValueTypes.Aggregate(0, (t, v) => (t * 397) ^ v.GetHashCode());
                }
            }
        }

        private readonly ConcurrentDictionary<CacheKey, Action<object[]>> _cache
            = new ConcurrentDictionary<CacheKey, Action<object[]>>();

        /// <summary>
        ///     Creates a new <see cref="IRelationalValueBufferFactory" />.
        /// </summary>
        /// <param name="valueTypes">
        ///     The types of values to be returned from the value buffer.
        /// </param>
        /// <param name="indexMap">
        ///     An ordered list of zero-based indexes to be read from the underlying result set (i.e. the first number in this
        ///     list is the index of the underlying result set that will be returned when value 0 is requested from the
        ///     value buffer).
        /// </param>
        /// <returns>
        ///     The newly created <see cref="IRelationalValueBufferFactoryFactory" />.
        /// </returns>
        public virtual IRelationalValueBufferFactory Create(
            IReadOnlyList<Type> valueTypes, IReadOnlyList<int> indexMap)
        {
            var processValuesAction = _cache.GetOrAdd(new CacheKey(valueTypes), _createValueProcessorDelegate);

            return indexMap == null
                ? (IRelationalValueBufferFactory)new UntypedRelationalValueBufferFactory(processValuesAction)
                : new RemappingUntypedRelationalValueBufferFactory(indexMap, processValuesAction);
        }

        private static readonly Func<CacheKey, Action<object[]>> _createValueProcessorDelegate = CreateValueProcessor;

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
