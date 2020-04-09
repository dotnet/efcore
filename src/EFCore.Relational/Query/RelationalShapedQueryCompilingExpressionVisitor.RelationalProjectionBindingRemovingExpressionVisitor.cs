// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor
    {
        private sealed class RelationalProjectionBindingRemovingExpressionVisitor : ExpressionVisitor
        {
            private static readonly MethodInfo _isDbNullMethod =
                typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.IsDBNull), new[] { typeof(int) });
            private static readonly MethodInfo _getFieldValueMethod =
                typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetFieldValue), new[] { typeof(int) });
            private static readonly MethodInfo _throwReadValueExceptionMethod =
                typeof(RelationalProjectionBindingRemovingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ThrowReadValueException));

            private readonly SelectExpression _selectExpression;
            private readonly ParameterExpression _dbDataReaderParameter;
            private readonly ParameterExpression _indexMapParameter;
            private readonly bool _detailedErrorsEnabled;

            private readonly IDictionary<ParameterExpression, IDictionary<IProperty, int>> _materializationContextBindings
                = new Dictionary<ParameterExpression, IDictionary<IProperty, int>>();

            public RelationalProjectionBindingRemovingExpressionVisitor(
                SelectExpression selectExpression,
                ParameterExpression dbDataReaderParameter,
                ParameterExpression indexMapParameter,
                bool detailedErrorsEnabled,
                bool buffer)
            {
                _selectExpression = selectExpression;
                _dbDataReaderParameter = dbDataReaderParameter;
                _indexMapParameter = indexMapParameter;
                _detailedErrorsEnabled = detailedErrorsEnabled;
                if (buffer)
                {
                    ProjectionColumns = new ReaderColumn[selectExpression.Projection.Count];
                }
            }

            private ReaderColumn[] ProjectionColumns { get; }

            public Expression Visit(Expression node, out IReadOnlyList<ReaderColumn> projectionColumns)
            {
                var result = Visit(node);
                projectionColumns = ProjectionColumns;
                return result;
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                Check.NotNull(binaryExpression, nameof(binaryExpression));

                if (binaryExpression.NodeType == ExpressionType.Assign
                    && binaryExpression.Left is ParameterExpression parameterExpression
                    && parameterExpression.Type == typeof(MaterializationContext))
                {
                    var newExpression = (NewExpression)binaryExpression.Right;
                    var projectionBindingExpression = (ProjectionBindingExpression)newExpression.Arguments[0];

                    _materializationContextBindings[parameterExpression]
                        = (IDictionary<IProperty, int>)GetProjectionIndex(projectionBindingExpression);

                    var updatedExpression = Expression.New(
                        newExpression.Constructor,
                        Expression.Constant(ValueBuffer.Empty),
                        newExpression.Arguments[1]);

                    return Expression.MakeBinary(ExpressionType.Assign, binaryExpression.Left, updatedExpression);
                }

                if (binaryExpression.NodeType == ExpressionType.Assign
                    && binaryExpression.Left is MemberExpression memberExpression
                    && memberExpression.Member is FieldInfo fieldInfo
                    && fieldInfo.IsInitOnly)
                {
                    return memberExpression.Assign(Visit(binaryExpression.Right));
                }

                return base.VisitBinary(binaryExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                Check.NotNull(methodCallExpression, nameof(methodCallExpression));

                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod)
                {
                    var property = (IProperty)((ConstantExpression)methodCallExpression.Arguments[2]).Value;
                    var propertyProjectionMap = methodCallExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression
                        ? (IDictionary<IProperty, int>)GetProjectionIndex(projectionBindingExpression)
                        : _materializationContextBindings[
                            (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object];

                    var projectionIndex = propertyProjectionMap[property];
                    var projection = _selectExpression.Projection[projectionIndex];

                    return CreateGetValueExpression(
                        _dbDataReaderParameter,
                        projectionIndex,
                        IsNullableProjection(projection),
                        property.GetRelationalTypeMapping(),
                        methodCallExpression.Type,
                        property);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    var projectionIndex = (int)GetProjectionIndex(projectionBindingExpression);
                    var projection = _selectExpression.Projection[projectionIndex];

                    return CreateGetValueExpression(
                        _dbDataReaderParameter,
                        projectionIndex,
                        IsNullableProjection(projection),
                        projection.Expression.TypeMapping,
                        projectionBindingExpression.Type);
                }

                return base.VisitExtension(extensionExpression);
            }

            private object GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
                => projectionBindingExpression.ProjectionMember != null
                    ? ((ConstantExpression)_selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)).Value
                    : (projectionBindingExpression.Index != null
                        ? (object)projectionBindingExpression.Index
                        : projectionBindingExpression.IndexMap);

            private static bool IsNullableProjection(ProjectionExpression projection)
                => !(projection.Expression is ColumnExpression column) || column.IsNullable;

            private Expression CreateGetValueExpression(
                ParameterExpression dbDataReader,
                int index,
                bool nullable,
                RelationalTypeMapping typeMapping,
                Type clrType,
                IPropertyBase property = null)
            {
                var getMethod = typeMapping.GetDataReaderMethod();

                Expression indexExpression = Expression.Constant(index);
                if (_indexMapParameter != null)
                {
                    indexExpression = Expression.ArrayIndex(_indexMapParameter, indexExpression);
                }

                Expression valueExpression
                    = Expression.Call(
                        getMethod.DeclaringType != typeof(DbDataReader)
                            ? Expression.Convert(dbDataReader, getMethod.DeclaringType)
                            : (Expression)dbDataReader,
                        getMethod,
                        indexExpression);

                if (ProjectionColumns != null)
                {
                    var columnType = valueExpression.Type;
                    if (!columnType.IsValueType
                        || !BufferedDataReader.IsSupportedValueType(columnType))
                    {
                        columnType = typeof(object);
                        valueExpression = Expression.Convert(valueExpression, typeof(object));
                    }

                    if (ProjectionColumns[index] == null)
                    {
                        ProjectionColumns[index] = ReaderColumn.Create(
                            columnType,
                            nullable,
                            _indexMapParameter != null ? ((ColumnExpression)_selectExpression.Projection[index].Expression).Name : null,
                            Expression.Lambda(
                                valueExpression,
                                dbDataReader,
                                _indexMapParameter ?? Expression.Parameter(typeof(int[]))).Compile());
                    }

                    if (getMethod.DeclaringType != typeof(DbDataReader))
                    {
                        valueExpression
                            = Expression.Call(
                                dbDataReader,
                                RelationalTypeMapping.GetDataReaderMethod(columnType),
                                indexExpression);
                    }
                }

                valueExpression = typeMapping.CustomizeDataReaderExpression(valueExpression);

                var converter = typeMapping.Converter;

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

                if (valueExpression.Type != clrType)
                {
                    valueExpression = Expression.Convert(valueExpression, clrType);
                }

                var exceptionParameter
                    = Expression.Parameter(typeof(Exception), name: "e");

                if (_detailedErrorsEnabled)
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
                                        dbDataReader,
                                        _getFieldValueMethod.MakeGenericMethod(typeof(object)),
                                        indexExpression),
                                    Expression.Constant(property, typeof(IPropertyBase))));

                    valueExpression = Expression.TryCatch(valueExpression, catchBlock);
                }

                if (nullable)
                {
                    valueExpression
                        = Expression.Condition(
                            Expression.Call(dbDataReader, _isDbNullMethod, indexExpression),
                            Expression.Default(valueExpression.Type),
                            valueExpression);
                }

                return valueExpression;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static TValue ThrowReadValueException<TValue>(
                Exception exception, object value, IPropertyBase property = null)
            {
                var expectedType = typeof(TValue);
                var actualType = value?.GetType();

                string message;

                if (property != null)
                {
                    var entityType = property.DeclaringType.DisplayName();
                    var propertyName = property.Name;
                    if (expectedType == typeof(object))
                    {
                        expectedType = property.ClrType;
                    }

                    message = exception is NullReferenceException
                        || Equals(value, DBNull.Value)
                        ? CoreStrings.ErrorMaterializingPropertyNullReference(entityType, propertyName, expectedType)
                        : exception is InvalidCastException
                            ? CoreStrings.ErrorMaterializingPropertyInvalidCast(entityType, propertyName, expectedType, actualType)
                            : CoreStrings.ErrorMaterializingProperty(entityType, propertyName);
                }
                else
                {
                    message = exception is NullReferenceException
                        || Equals(value, DBNull.Value)
                        ? CoreStrings.ErrorMaterializingValueNullReference(expectedType)
                        : exception is InvalidCastException
                            ? CoreStrings.ErrorMaterializingValueInvalidCast(expectedType, actualType)
                            : CoreStrings.ErrorMaterializingValue;
                }

                throw new InvalidOperationException(message, exception);
            }
        }
    }
}
