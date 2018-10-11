// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Parsing.ExpressionVisitors;

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
    [Obsolete("Use TypedRelationalValueBufferFactoryFactory instead.")]
    public class UntypedRelationalValueBufferFactoryFactory : IRelationalValueBufferFactoryFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UntypedRelationalValueBufferFactoryFactory" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public UntypedRelationalValueBufferFactoryFactory([NotNull] RelationalValueBufferFactoryDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual RelationalValueBufferFactoryDependencies Dependencies { get; }

        private readonly struct CacheKey
        {
            public CacheKey(IReadOnlyList<TypeMaterializationInfo> typeMaterializationInfo)
                => TypeMaterializationInfo = typeMaterializationInfo;

            public IReadOnlyList<TypeMaterializationInfo> TypeMaterializationInfo { get; }

            public override bool Equals(object obj)
                => !(obj is null)
                   && (obj is CacheKey cacheKey
                       && Equals(cacheKey));

            private bool Equals(CacheKey other)
                => TypeMaterializationInfo.SequenceEqual(other.TypeMaterializationInfo);

            public override int GetHashCode()
                => TypeMaterializationInfo.Aggregate(0, (t, v) => (t * 397) ^ v.GetHashCode());
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
        [Obsolete("Use Create(IReadOnlyList<TypeMaterializationInfo>).")]
        public virtual IRelationalValueBufferFactory Create(
            IReadOnlyList<Type> valueTypes, IReadOnlyList<int> indexMap)
        {
            Check.NotNull(valueTypes, nameof(valueTypes));

            var mappingSource = Dependencies.TypeMappingSource;

            return Create(
                valueTypes.Select(
                    (t, i) => new TypeMaterializationInfo(t, null, mappingSource, indexMap?[i] ?? -1)).ToList());
        }

        /// <summary>
        ///     Creates a new <see cref="IRelationalValueBufferFactory" />.
        /// </summary>
        /// <param name="types"> Types and mapping for the values to be read. </param>
        /// <returns> The newly created <see cref="IRelationalValueBufferFactoryFactory" />. </returns>
        public virtual IRelationalValueBufferFactory Create(IReadOnlyList<TypeMaterializationInfo> types)
        {
            Check.NotNull(types, nameof(types));

            var processValuesAction = _cache.GetOrAdd(new CacheKey(types), _createValueProcessorDelegate);

            return types.Any(t => t.Index >= 0)
                ? (IRelationalValueBufferFactory)new RemappingUntypedRelationalValueBufferFactory(Dependencies, types, processValuesAction)
                : new UntypedRelationalValueBufferFactory(Dependencies, types, processValuesAction);
        }

        private static readonly Func<CacheKey, Action<object[]>> _createValueProcessorDelegate = CreateValueProcessor;

        private static Action<object[]> CreateValueProcessor(CacheKey cacheKey)
        {
            var valuesParam = Expression.Parameter(typeof(object[]), "values");
            var conversions = new List<Expression>();
            var materializationInfo = cacheKey.TypeMaterializationInfo;

            for (var i = 0; i < materializationInfo.Count; i++)
            {
                var converter = materializationInfo[i].Mapping?.Converter;

                var arrayAccess =
                    Expression.ArrayAccess(valuesParam, Expression.Constant(i));

                if (converter != null)
                {
                    Expression valueExpression = Expression.Convert(
                        arrayAccess,
                        converter.ProviderClrType);

                    valueExpression
                        = Expression.Convert(
                            ReplacingExpressionVisitor.Replace(
                                converter.ConvertFromProviderExpression.Parameters.Single(),
                                valueExpression,
                                converter.ConvertFromProviderExpression.Body),
                            typeof(object));

                    valueExpression
                        = Expression.Condition(
                            Expression.ReferenceEqual(
                                arrayAccess,
                                Expression.Constant(DBNull.Value)),
                            Expression.Constant(null),
                            valueExpression);

                    conversions.Add(
                        Expression.Assign(
                            arrayAccess,
                            valueExpression
                        ));
                }
            }

            return conversions.Count == 0
                ? null
                : Expression.Lambda<Action<object[]>>(
                    Expression.Block(
                        conversions),
                    valuesParam)
                .Compile();
        }
    }
}
