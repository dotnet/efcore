// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly IQuerySqlGeneratorFactory2 _querySqlGeneratorFactory;

        public RelationalShapedQueryCompilingExpressionVisitor(
            IEntityMaterializerSource entityMaterializerSource,
            IQuerySqlGeneratorFactory2 querySqlGeneratorFactory,
            bool trackQueryResults,
            bool async)
            : base(entityMaterializerSource, trackQueryResults, async)
        {
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        protected override Expression VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
        {
            var shaperLambda = InjectEntityMaterializer(shapedQueryExpression.ShaperExpression);

            var selectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;

            var newBody = new RelationalProjectionBindingRemovingExpressionVisitor(selectExpression)
                .Visit(shaperLambda.Body);

            shaperLambda = Expression.Lambda(
                newBody,
                QueryCompilationContext2.QueryContextParameter,
                RelationalProjectionBindingRemovingExpressionVisitor.DataReaderParameter);

            if (Async)
            {
                return Expression.New(
                    typeof(AsyncQueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType.GetGenericArguments().Single()).GetConstructors()[0],
                    Expression.Convert(QueryCompilationContext2.QueryContextParameter, typeof(RelationalQueryContext)),
                    Expression.Constant(_querySqlGeneratorFactory.Create()),
                    Expression.Constant(selectExpression),
                    Expression.Constant(shaperLambda.Compile()));
            }

            return Expression.New(
                typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                Expression.Convert(QueryCompilationContext2.QueryContextParameter, typeof(RelationalQueryContext)),
                Expression.Constant(_querySqlGeneratorFactory.Create()),
                Expression.Constant(selectExpression),
                Expression.Constant(shaperLambda.Compile()));
        }

        private class AsyncQueryingEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly RelationalQueryContext _relationalQueryContext;
            private readonly SelectExpression _selectExpression;
            private readonly Func<QueryContext, DbDataReader, Task<T>> _shaper;
            private readonly QuerySqlGenerator _querySqlGenerator;

            public AsyncQueryingEnumerable(RelationalQueryContext relationalQueryContext,
                QuerySqlGenerator querySqlGenerator,
                SelectExpression selectExpression,
                Func<QueryContext, DbDataReader, Task<T>> shaper)
            {
                _relationalQueryContext = relationalQueryContext;
                _querySqlGenerator = querySqlGenerator;
                _selectExpression = selectExpression;
                _shaper = shaper;
            }

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return new AsyncEnumerator(this);
            }

            private sealed class AsyncEnumerator : IAsyncEnumerator<T>
            {
                private RelationalDataReader _dataReader;
                private readonly RelationalQueryContext _relationalQueryContext;
                private readonly SelectExpression _selectExpression;
                private readonly Func<QueryContext, DbDataReader, Task<T>> _shaper;
                private readonly QuerySqlGenerator _querySqlGenerator;

                public AsyncEnumerator(AsyncQueryingEnumerable<T> queryingEnumerable)
                {
                    _relationalQueryContext = queryingEnumerable._relationalQueryContext;
                    _shaper = queryingEnumerable._shaper;
                    _selectExpression = queryingEnumerable._selectExpression;
                    _querySqlGenerator = queryingEnumerable._querySqlGenerator;
                }

                public T Current { get; private set; }

                public void Dispose()
                {
                    _dataReader?.Dispose();
                    _dataReader = null;
                    _relationalQueryContext.Connection.Close();
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    if (_dataReader == null)
                    {
                        await _relationalQueryContext.Connection.OpenAsync(cancellationToken);

                        try
                        {
                            var relationalCommand = _querySqlGenerator
                                .GetCommand(
                                    _selectExpression,
                                    _relationalQueryContext.ParameterValues,
                                    _relationalQueryContext.CommandLogger);

                            _dataReader
                                = await relationalCommand.ExecuteReaderAsync(
                                    _relationalQueryContext.Connection,
                                    _relationalQueryContext.ParameterValues,
                                    _relationalQueryContext.CommandLogger,
                                    cancellationToken);
                        }
                        catch
                        {
                            // If failure happens creating the data reader, then it won't be available to
                            // handle closing the connection, so do it explicitly here to preserve ref counting.
                            _relationalQueryContext.Connection.Close();

                            throw;
                        }
                    }

                    var hasNext = await _dataReader.ReadAsync(cancellationToken);

                    Current
                        = hasNext
                            ? await _shaper(_relationalQueryContext, _dataReader.DbDataReader)
                            : default;

                    return hasNext;
                }
            }
        }

        private class QueryingEnumerable<T> : IEnumerable<T>
        {
            private readonly RelationalQueryContext _relationalQueryContext;
            private readonly SelectExpression _selectExpression;
            private readonly Func<QueryContext, DbDataReader, T> _shaper;
            private readonly QuerySqlGenerator _querySqlGenerator;

            public QueryingEnumerable(RelationalQueryContext relationalQueryContext,
                QuerySqlGenerator querySqlGenerator,
                SelectExpression selectExpression,
                Func<QueryContext, DbDataReader, T> shaper)
            {
                _relationalQueryContext = relationalQueryContext;
                _querySqlGenerator = querySqlGenerator;
                _selectExpression = selectExpression;
                _shaper = shaper;
            }

            public IEnumerator<T> GetEnumerator() => new Enumerator(this);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private sealed class Enumerator : IEnumerator<T>
            {
                private RelationalDataReader _dataReader;
                private readonly RelationalQueryContext _relationalQueryContext;
                private readonly SelectExpression _selectExpression;
                private readonly Func<QueryContext, DbDataReader, T> _shaper;
                private readonly QuerySqlGenerator _querySqlGenerator;

                public Enumerator(QueryingEnumerable<T> queryingEnumerable)
                {
                    _relationalQueryContext = queryingEnumerable._relationalQueryContext;
                    _shaper = queryingEnumerable._shaper;
                    _selectExpression = queryingEnumerable._selectExpression;
                    _querySqlGenerator = queryingEnumerable._querySqlGenerator;
                }

                public T Current { get; private set; }

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                    _dataReader?.Dispose();
                    _dataReader = null;
                    _relationalQueryContext.Connection.Close();
                }

                public bool MoveNext()
                {
                    if (_dataReader == null)
                    {
                        _relationalQueryContext.Connection.Open();

                        try
                        {
                            var relationalCommand = _querySqlGenerator
                                .GetCommand(
                                    _selectExpression,
                                    _relationalQueryContext.ParameterValues,
                                    _relationalQueryContext.CommandLogger);

                            _dataReader
                                = relationalCommand.ExecuteReader(
                                    _relationalQueryContext.Connection,
                                    _relationalQueryContext.ParameterValues,
                                    _relationalQueryContext.CommandLogger);
                        }
                        catch
                        {
                            // If failure happens creating the data reader, then it won't be available to
                            // handle closing the connection, so do it explicitly here to preserve ref counting.
                            _relationalQueryContext.Connection.Close();

                            throw;
                        }
                    }

                    var hasNext = _dataReader.Read();

                    Current
                        = hasNext
                            ? _shaper(_relationalQueryContext, _dataReader.DbDataReader)
                            : default;

                    return hasNext;
                }

                public void Reset() => throw new NotImplementedException();
            }
        }

        private class RelationalProjectionBindingRemovingExpressionVisitor : ExpressionVisitor
        {
            public static readonly ParameterExpression DataReaderParameter
                = Expression.Parameter(typeof(DbDataReader), "dataReader");

            private readonly IDictionary<ParameterExpression, int> _materializationContextBindings
                = new Dictionary<ParameterExpression, int>();

            public RelationalProjectionBindingRemovingExpressionVisitor(SelectExpression selectExpression)
            {
                _selectExpression = selectExpression;
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.Assign
                    && binaryExpression.Left is ParameterExpression parameterExpression
                    && parameterExpression.Type == typeof(MaterializationContext))
                {
                    var newExpression = (NewExpression)binaryExpression.Right;

                    _materializationContextBindings[parameterExpression]
                        = (int)((ConstantExpression)_selectExpression.GetProjectionExpression(((ProjectionBindingExpression)newExpression.Arguments[0]).ProjectionMember)).Value;

                    var updatedExpression = Expression.New(newExpression.Constructor,
                        Expression.Constant(ValueBuffer.Empty),
                        newExpression.Arguments[1]);

                    return Expression.MakeBinary(ExpressionType.Assign, binaryExpression.Left, updatedExpression);
                }

                return base.VisitBinary(binaryExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == EntityMaterializerSource.TryReadValueMethod)
                {
                    var originalIndex = (int)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                    var indexOffset = methodCallExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression
                        ? (int)((ConstantExpression)_selectExpression.GetProjectionExpression(projectionBindingExpression.ProjectionMember)).Value
                        : _materializationContextBindings[(ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object];

                    var property = (IProperty)((ConstantExpression)methodCallExpression.Arguments[2]).Value;

                    var projectionIndex = originalIndex + indexOffset;
                    var projection = _selectExpression.Projection[projectionIndex];

                    return CreateGetValueExpression(
                        projectionIndex,
                        IsNullableProjection(projection),
                        property.FindRelationalMapping(),
                        methodCallExpression.Type);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    var projectionIndex = (int)((ConstantExpression)_selectExpression.GetProjectionExpression(projectionBindingExpression.ProjectionMember)).Value;
                    var projection = _selectExpression.Projection[projectionIndex];

                    return CreateGetValueExpression(
                        projectionIndex,
                        IsNullableProjection(projection),
                        projection.Expression.TypeMapping,
                        projectionBindingExpression.Type);
                }

                return base.VisitExtension(extensionExpression);
            }

            private static bool IsNullableProjection(ProjectionExpression projection)
            {
                return projection.Expression is ColumnExpression column ? column.Nullable : true;
            }

            private static Expression CreateGetValueExpression(
                int index,
                bool nullable,
                RelationalTypeMapping typeMapping,
                Type clrType)
            {
                var getMethod = typeMapping.GetDataReaderMethod();

                var indexExpression = Expression.Constant(index);

                Expression valueExpression
                    = Expression.Call(
                        getMethod.DeclaringType != typeof(DbDataReader)
                            ? Expression.Convert(DataReaderParameter, getMethod.DeclaringType)
                            : (Expression)DataReaderParameter,
                        getMethod,
                        indexExpression);

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
                            Expression.Call(DataReaderParameter, _isDbNullMethod, indexExpression),
                            Expression.Default(valueExpression.Type),
                            valueExpression);
                }

                return valueExpression;
            }

            private static readonly MethodInfo _isDbNullMethod =
                typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.IsDBNull), new[] { typeof(int) });

            private readonly SelectExpression _selectExpression;
        }
    }
}
