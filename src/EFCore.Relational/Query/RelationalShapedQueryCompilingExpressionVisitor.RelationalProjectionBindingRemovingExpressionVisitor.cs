// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor
    {
        private class RelationalProjectionBindingRemovingExpressionVisitor : ExpressionVisitor
        {
            private static readonly MethodInfo _isDbNullMethod =
                typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.IsDBNull), new[] { typeof(int) });

            private readonly SelectExpression _selectExpression;
            private readonly ParameterExpression _dbDataReaderParameter;
            private readonly ParameterExpression _indexMapParameter;

            private readonly IDictionary<ParameterExpression, IDictionary<IProperty, int>> _materializationContextBindings
                = new Dictionary<ParameterExpression, IDictionary<IProperty, int>>();

            public RelationalProjectionBindingRemovingExpressionVisitor(
                SelectExpression selectExpression,
                ParameterExpression dbDataReaderParameter,
                ParameterExpression indexMapParameter,
                bool buffer)
            {
                _selectExpression = selectExpression;
                _dbDataReaderParameter = dbDataReaderParameter;
                _indexMapParameter = indexMapParameter;
                if (buffer)
                {
                    ProjectionColumns = new ReaderColumn[selectExpression.Projection.Count];
                }
            }

            private ReaderColumn[] ProjectionColumns { get; }

            public virtual Expression Visit(Expression node, out IReadOnlyList<ReaderColumn> projectionColumns)
            {
                var result = Visit(node);
                projectionColumns = ProjectionColumns;
                return result;
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
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
                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod)
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
                        methodCallExpression.Type);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
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
                Type clrType)
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

                //var exceptionParameter
                //    = Expression.Parameter(typeof(Exception), name: "e");

                //var property = materializationInfo.Property;

                //if (detailedErrorsEnabled)
                //{
                //    var catchBlock
                //        = Expression
                //            .Catch(
                //                exceptionParameter,
                //                Expression.Call(
                //                    _throwReadValueExceptionMethod
                //                        .MakeGenericMethod(valueExpression.Type),
                //                    exceptionParameter,
                //                    Expression.Call(
                //                        dataReaderExpression,
                //                        _getFieldValueMethod.MakeGenericMethod(typeof(object)),
                //                        indexExpression),
                //                    Expression.Constant(property, typeof(IPropertyBase))));

                //    valueExpression = Expression.TryCatch(valueExpression, catchBlock);
                //}

                //if (box && valueExpression.Type.GetTypeInfo().IsValueType)
                //{
                //    valueExpression = Expression.Convert(valueExpression, typeof(object));
                //}

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
        }
    }
}
