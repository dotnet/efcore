// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Creates instances of the <see cref="IRelationalValueBufferFactory" /> type. <see cref="IRelationalValueBufferFactory" />
    ///         instances are tied to a specific result shape. This factory is responsible for creating the
    ///         <see cref="IRelationalValueBufferFactory" /> for a given result shape.
    ///     </para>
    ///     <para>
    ///         This factory results in value buffers that use they strongly typed APIs to read back individual values from the
    ///         underlying <see cref="DbDataReader" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class TypedRelationalValueBufferFactoryFactory : IRelationalValueBufferFactoryFactory
    {
        private static readonly MethodInfo _getFieldValueMethod
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetFieldValue));

        private static readonly MethodInfo _isDbNullMethod
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

        private static readonly IDictionary<Type, MethodInfo> _getXMethods
            = new Dictionary<Type, MethodInfo>
            {
                { typeof(bool), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetBoolean)) },
                { typeof(byte), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetByte)) },
                { typeof(char), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetChar)) },
                { typeof(DateTime), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetDateTime)) },
                { typeof(decimal), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetDecimal)) },
                { typeof(double), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetDouble)) },
                { typeof(float), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetFloat)) },
                { typeof(Guid), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetGuid)) },
                { typeof(short), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetInt16)) },
                { typeof(int), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetInt32)) },
                { typeof(long), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetInt64)) },
                { typeof(string), typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetString)) }
            };

        private struct CacheKey
        {
            public CacheKey(IReadOnlyList<Type> valueTypes, IReadOnlyList<int> indexMap)
            {
                ValueTypes = valueTypes;
                IndexMap = indexMap;
            }

            public IReadOnlyList<Type> ValueTypes { get; }
            public IReadOnlyList<int> IndexMap { get; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is CacheKey
                       && Equals((CacheKey)obj);
            }

            private bool Equals(CacheKey other)
            {
                if (!ValueTypes.SequenceEqual(other.ValueTypes))
                {
                    return false;
                }

                if (IndexMap == null)
                {
                    return other.IndexMap == null;
                }

                return (other.IndexMap != null)
                       && IndexMap.SequenceEqual(other.IndexMap);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ValueTypes.Aggregate(0, (t, v) => (t * 397) ^ v.GetHashCode())
                           ^ (IndexMap?.Aggregate(0, (t, v) => (t * 397) ^ v.GetHashCode()) ?? 0);
                }
            }
        }

        private readonly ConcurrentDictionary<CacheKey, Func<DbDataReader, object[]>> _cache
            = new ConcurrentDictionary<CacheKey, Func<DbDataReader, object[]>>();

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
            Check.NotNull(valueTypes, nameof(valueTypes));

            return new TypedRelationalValueBufferFactory(
                _cache.GetOrAdd(
                    new CacheKey(valueTypes, indexMap),
                    CreateArrayInitializer));
        }

        private static Func<DbDataReader, object[]> CreateArrayInitializer(CacheKey cacheKey)
        {
            var dataReaderParameter = Expression.Parameter(typeof(DbDataReader), "dataReader");

            return Expression.Lambda<Func<DbDataReader, object[]>>(
                    Expression.NewArrayInit(
                        typeof(object),
                        cacheKey.ValueTypes
                            .Select((type, i) =>
                                CreateGetValueExpression(
                                    dataReaderParameter,
                                    type,
                                    Expression.Constant(cacheKey.IndexMap?[i] ?? i)))),
                    dataReaderParameter)
                .Compile();
        }

        private static Expression CreateGetValueExpression(
            Expression dataReaderExpression, Type type, Expression indexExpression)
        {
            var underlyingType = type.UnwrapNullableType().UnwrapEnumType();

            MethodInfo getMethod;
            if (!_getXMethods.TryGetValue(underlyingType, out getMethod))
            {
                getMethod = _getFieldValueMethod.MakeGenericMethod(underlyingType);
            }

            Expression expression
                = Expression.Call(dataReaderExpression, getMethod, indexExpression);

            if (expression.Type != type)
            {
                expression = Expression.Convert(expression, type);
            }

            var exceptionParameter
                = Expression.Parameter(typeof(Exception), "e");

            var catchBlock
                = Expression
                    .Catch(exceptionParameter,
                        Expression.Call(
                            EntityMaterializerSource
                                .ThrowReadValueExceptionMethod
                                .MakeGenericMethod(expression.Type),
                            exceptionParameter,
                            Expression.Call(
                                dataReaderExpression,
                                _getFieldValueMethod.MakeGenericMethod(typeof(object)),
                                indexExpression),
                            Expression.Constant(null, typeof(IPropertyBase))));

            expression = Expression.TryCatch(expression, catchBlock);

            if (expression.Type.GetTypeInfo().IsValueType)
            {
                expression = Expression.Convert(expression, typeof(object));
            }

            if (expression.Type.IsNullableType())
            {
                expression
                    = Expression.Condition(
                        Expression.Call(dataReaderExpression, _isDbNullMethod, indexExpression),
                        Expression.Default(expression.Type),
                        expression);
            }
            
            return expression;
        }
    }
}
