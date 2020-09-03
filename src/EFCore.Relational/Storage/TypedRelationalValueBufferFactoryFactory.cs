// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

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
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class TypedRelationalValueBufferFactoryFactory : IRelationalValueBufferFactoryFactory
    {
        /// <summary>
        ///     The parameter representing the DbDataReader in generated expressions.
        /// </summary>
        public static readonly ParameterExpression DataReaderParameter
            = Expression.Parameter(typeof(DbDataReader), "dataReader");

        private static readonly MethodInfo _getFieldValueMethod =
            typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetFieldValue), new[] { typeof(int) });

        private static readonly MethodInfo _isDbNullMethod =
            typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.IsDBNull), new[] { typeof(int) });

        private static readonly MethodInfo _throwReadValueExceptionMethod
            = typeof(TypedRelationalValueBufferFactoryFactory).GetTypeInfo().GetDeclaredMethod(nameof(ThrowReadValueException));

        /// <summary>
        ///     Initializes a new instance of the <see cref="TypedRelationalValueBufferFactoryFactory" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public TypedRelationalValueBufferFactoryFactory([NotNull] RelationalValueBufferFactoryDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual RelationalValueBufferFactoryDependencies Dependencies { get; }

        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            public CacheKey(IReadOnlyList<TypeMaterializationInfo> materializationInfo)
                => TypeMaterializationInfo = materializationInfo;

            public IReadOnlyList<TypeMaterializationInfo> TypeMaterializationInfo { get; }

            public override bool Equals(object obj)
                => obj is CacheKey cacheKey && Equals(cacheKey);

            public bool Equals(CacheKey other)
                => TypeMaterializationInfo.SequenceEqual(other.TypeMaterializationInfo);

            public override int GetHashCode()
            {
                var hash = new HashCode();
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < TypeMaterializationInfo.Count; i++)
                {
                    hash.Add(TypeMaterializationInfo[i]);
                }

                return hash.ToHashCode();
            }
        }

        private readonly ConcurrentDictionary<CacheKey, TypedRelationalValueBufferFactory> _cache
            = new ConcurrentDictionary<CacheKey, TypedRelationalValueBufferFactory>();

        /// <summary>
        ///     Creates a new <see cref="IRelationalValueBufferFactory" />.
        /// </summary>
        /// <param name="types"> Types and mapping for the values to be read. </param>
        /// <returns> The newly created <see cref="IRelationalValueBufferFactoryFactory" />. </returns>
        public virtual IRelationalValueBufferFactory Create(IReadOnlyList<TypeMaterializationInfo> types)
        {
            Check.NotNull(types, nameof(types));

            return _cache.GetOrAdd(
                new CacheKey(types),
                k => new TypedRelationalValueBufferFactory(
                    Dependencies,
                    CreateArrayInitializer(k, Dependencies.CoreOptions.AreDetailedErrorsEnabled)));
        }

        /// <summary>
        ///     Creates value buffer assignment expressions for the given type information.
        /// </summary>
        /// <param name="types"> Types and mapping for the values to be read. </param>
        /// <returns> The value buffer assignment expressions. </returns>
        [Obsolete]
        public virtual IReadOnlyList<Expression> CreateAssignmentExpressions([NotNull] IReadOnlyList<TypeMaterializationInfo> types)
            => Check.NotNull(types, nameof(types))
                .Select(
                    (mi, i) =>
                        CreateGetValueExpression(
                            DataReaderParameter,
                            i,
                            mi,
                            Dependencies.CoreOptions.AreDetailedErrorsEnabled,
                            box: false)).ToArray();

        private static Func<DbDataReader, object[]> CreateArrayInitializer(CacheKey cacheKey, bool detailedErrorsEnabled)
            => Expression.Lambda<Func<DbDataReader, object[]>>(
                    Expression.NewArrayInit(
                        typeof(object),
                        cacheKey.TypeMaterializationInfo
                            .Select(
                                (mi, i) =>
                                    CreateGetValueExpression(
                                        DataReaderParameter,
                                        i,
                                        mi,
                                        detailedErrorsEnabled))),
                    DataReaderParameter)
                .Compile();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TValue ThrowReadValueException<TValue>(
            Exception exception,
            object value,
            IPropertyBase property = null)
        {
            var expectedType = typeof(TValue);
            var actualType = value?.GetType();

            string message;

            if (property != null)
            {
                var entityType = property.DeclaringType.DisplayName();
                var propertyName = property.Name;

                message
                    = exception is NullReferenceException
                    || Equals(value, DBNull.Value)
                        ? RelationalStrings.ErrorMaterializingPropertyNullReference(entityType, propertyName, expectedType)
                        : exception is InvalidCastException
                            ? CoreStrings.ErrorMaterializingPropertyInvalidCast(entityType, propertyName, expectedType, actualType)
                            : RelationalStrings.ErrorMaterializingProperty(entityType, propertyName);
            }
            else
            {
                message
                    = exception is NullReferenceException
                        ? RelationalStrings.ErrorMaterializingValueNullReference(expectedType)
                        : exception is InvalidCastException
                            ? RelationalStrings.ErrorMaterializingValueInvalidCast(expectedType, actualType)
                            : RelationalStrings.ErrorMaterializingValue;
            }

            throw new InvalidOperationException(message, exception);
        }

        private static Expression CreateGetValueExpression(
            Expression dataReaderExpression,
            int index,
            TypeMaterializationInfo materializationInfo,
            bool detailedErrorsEnabled,
            bool box = true)
        {
            var getMethod = materializationInfo.Mapping.GetDataReaderMethod();

#pragma warning disable CS0612 // Type or member is obsolete
            index = materializationInfo.Index == -1 ? index : materializationInfo.Index;
#pragma warning restore CS0612 // Type or member is obsolete

            var indexExpression = Expression.Constant(index);

            Expression valueExpression
                = Expression.Call(
                    getMethod.DeclaringType != typeof(DbDataReader)
                        ? Expression.Convert(dataReaderExpression, getMethod.DeclaringType)
                        : dataReaderExpression,
                    getMethod,
                    indexExpression);

            valueExpression = materializationInfo.Mapping.CustomizeDataReaderExpression(valueExpression);

            var converter = materializationInfo.Mapping.Converter;

            if (converter != null)
            {
                if (valueExpression.Type != converter.ProviderClrType)
                {
                    valueExpression = Expression.Convert(valueExpression, converter.ProviderClrType);
                }

                valueExpression = ReplacingExpressionVisitor.Replace(
                    converter.ConvertFromProviderExpression.Parameters.Single(),
                    valueExpression,
                    converter.ConvertFromProviderExpression.Body);
            }

            if (valueExpression.Type != materializationInfo.ModelClrType)
            {
                valueExpression = Expression.Convert(valueExpression, materializationInfo.ModelClrType);
            }

            var exceptionParameter
                = Expression.Parameter(typeof(Exception), name: "e");

            var property = materializationInfo.Property;

            if (detailedErrorsEnabled)
            {
                var catchBlock
                    = Expression
                        .Catch(
                            exceptionParameter,
                            Expression.Call(
                                _throwReadValueExceptionMethod
                                    .MakeGenericMethod(valueExpression.Type),
                                exceptionParameter,
                                Expression.Call(
                                    dataReaderExpression,
                                    _getFieldValueMethod.MakeGenericMethod(typeof(object)),
                                    indexExpression),
                                Expression.Constant(property, typeof(IPropertyBase))));

                valueExpression = Expression.TryCatch(valueExpression, catchBlock);
            }

            if (box && valueExpression.Type.IsValueType)
            {
                valueExpression = Expression.Convert(valueExpression, typeof(object));
            }

#pragma warning disable CS0612 // Type or member is obsolete
            if (materializationInfo?.IsNullable != false
                || materializationInfo.IsFromLeftOuterJoin != false)
            {
#pragma warning restore CS0612 // Type or member is obsolete
                valueExpression
                    = Expression.Condition(
                        Expression.Call(dataReaderExpression, _isDbNullMethod, indexExpression),
                        Expression.Default(valueExpression.Type),
                        valueExpression);
            }

            return valueExpression;
        }
    }
}
