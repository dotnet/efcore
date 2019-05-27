// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalSqlTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly IModel _model;
        private readonly IQueryableMethodTranslatingExpressionVisitorFactory _queryableMethodTranslatingExpressionVisitorFactory;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IMemberTranslatorProvider _memberTranslatorProvider;
        private readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;
        private readonly SqlTypeMappingVerifyingExpressionVisitor _sqlVerifyingExpressionVisitor;

        public RelationalSqlTranslatingExpressionVisitor(
            IModel model,
            IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory,
            ISqlExpressionFactory sqlExpressionFactory,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            _model = model;
            _queryableMethodTranslatingExpressionVisitorFactory = queryableMethodTranslatingExpressionVisitorFactory;
            _sqlExpressionFactory = sqlExpressionFactory;
            _memberTranslatorProvider = memberTranslatorProvider;
            _methodCallTranslatorProvider = methodCallTranslatorProvider;
            _sqlVerifyingExpressionVisitor = new SqlTypeMappingVerifyingExpressionVisitor();
        }

        public SqlExpression Translate(Expression expression)
        {
            var translation = (SqlExpression)Visit(expression);

            if (translation is SqlUnaryExpression sqlUnaryExpression
                && sqlUnaryExpression.OperatorType == ExpressionType.Convert
                && sqlUnaryExpression.Type == typeof(object))
            {
                translation = sqlUnaryExpression.Operand;
            }

            translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(translation);

            _sqlVerifyingExpressionVisitor.Visit(translation);

            return translation;
        }

        private class SqlTypeMappingVerifyingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression node)
            {
                if (node is SqlExpression sqlExpression
                    && !(node is SqlFragmentExpression))
                {
                    if (sqlExpression.TypeMapping == null)
                    {
                        throw new InvalidOperationException("Null TypeMapping in Sql Tree");
                    }
                }

                return base.VisitExtension(node);
            }
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);
            if (innerExpression is EntityShaperExpression entityShaper)
            {
                return BindProperty(entityShaper, memberExpression.Member.GetSimpleMemberName());
            }

            return TranslationFailed(memberExpression.Expression, innerExpression)
                ? null
                : _memberTranslatorProvider.Translate((SqlExpression)innerExpression, memberExpression.Member, memberExpression.Type);
        }

        private Expression BindProperty(EntityShaperExpression entityShaper, string propertyName)
        {
            var property = entityShaper.EntityType.FindProperty(propertyName);
            var selectExpression = (SelectExpression)entityShaper.ValueBufferExpression.QueryExpression;

            return selectExpression.BindProperty(entityShaper.ValueBufferExpression, property);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                // In certain cases EF.Property would have convert node around the source.
                if (source is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Convert
                    && unaryExpression.Type == typeof(object))
                {
                    source = unaryExpression.Operand;
                }

                if (source is EntityShaperExpression entityShaper)
                {
                    return BindProperty(entityShaper, propertyName);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            if (methodCallExpression.Method.DeclaringType == typeof(Queryable))
            {
                var translation = _queryableMethodTranslatingExpressionVisitorFactory.Create(_model).Visit(methodCallExpression);

                if (translation is ShapedQueryExpression shapedQuery)
                {
                    var subquery = (SelectExpression)shapedQuery.QueryExpression;
                    subquery.ApplyProjection();

                    if (methodCallExpression.Method.Name == nameof(Queryable.Any)
                        || methodCallExpression.Method.Name == nameof(Queryable.All)
                        || methodCallExpression.Method.Name == nameof(Queryable.Contains))
                    {
                        if (subquery.Tables.Count == 0
                            && subquery.Projection.Count == 1)
                        {
                            return subquery.Projection[0].Expression;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    else
                    {
                        return new SubSelectExpression(subquery);
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            var @object = (SqlExpression)Visit(methodCallExpression.Object);
            var failed = TranslationFailed(methodCallExpression.Object, @object);
            var arguments = new SqlExpression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = (SqlExpression)Visit(methodCallExpression.Arguments[i]);
                failed |= (methodCallExpression.Arguments[i] != null && arguments[i] == null);
            }

            return failed
                ? null
                : _methodCallTranslatorProvider.Translate(_model, @object, methodCallExpression.Method, arguments);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var left = (SqlExpression)Visit(binaryExpression.Left);
            var right = (SqlExpression)Visit(binaryExpression.Right);

            if (TranslationFailed(binaryExpression.Left, left)
                || TranslationFailed(binaryExpression.Right, right))
            {
                return null;
            }

            return _sqlExpressionFactory.MakeBinary(
                binaryExpression.NodeType,
                left,
                right,
                null);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            return null;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            return null;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            return null;
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => new SqlConstantExpression(constantExpression, null);

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
            => new SqlParameterExpression(parameterExpression, null);


        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityShaperExpression)
            {
                return extensionExpression;
            }

            if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
            {
                var selectExpression = (SelectExpression)projectionBindingExpression.QueryExpression;

                return selectExpression.GetProjectionExpression(projectionBindingExpression.ProjectionMember);
            }

            if (extensionExpression is NullConditionalExpression nullConditionalExpression)
            {
                return Visit(nullConditionalExpression.AccessOperation);
            }

            if (extensionExpression is CorrelationPredicateExpression correlationPredicateExpression)
            {
                return Visit(correlationPredicateExpression.EqualExpression);
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            var test = (SqlExpression)Visit(conditionalExpression.Test);
            var ifTrue = (SqlExpression)Visit(conditionalExpression.IfTrue);
            var ifFalse = (SqlExpression)Visit(conditionalExpression.IfFalse);

            if (TranslationFailed(conditionalExpression.Test, test)
                || TranslationFailed(conditionalExpression.IfTrue, ifTrue)
                || TranslationFailed(conditionalExpression.IfFalse, ifFalse))
            {
                return null;
            }

            return _sqlExpressionFactory.Case(
                new[]
                {
                    new CaseWhenClause(test, ifTrue)
                },
                ifFalse);
        }

        //protected override Expression VisitNew(NewExpression newExpression)
        //{
        //    if (newExpression.Members == null
        //        || newExpression.Arguments.Count == 0)
        //    {
        //        return null;
        //    }

        //    var bindings = new Expression[newExpression.Arguments.Count];

        //    for (var i = 0; i < bindings.Length; i++)
        //    {
        //        var translation = Visit(newExpression.Arguments[i]);

        //        if (translation == null)
        //        {
        //            return null;
        //        }

        //        bindings[i] = translation;
        //    }

        //    return Expression.Constant(bindings);
        //}

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var operand = Visit(unaryExpression.Operand);

            if (TranslationFailed(unaryExpression.Operand, operand))
            {
                return null;
            }



            var sqlOperand = (SqlExpression)operand;

            switch (unaryExpression.NodeType)
            {

                case ExpressionType.Not:
                    return _sqlExpressionFactory.Not(sqlOperand);

                case ExpressionType.Negate:
                    return _sqlExpressionFactory.Negate(sqlOperand);

                case ExpressionType.Convert:
                    // Object convert needs to be converted to explicit cast when mismatching types
                    if (operand.Type.IsInterface
                            && unaryExpression.Type.GetInterfaces().Any(e => e == operand.Type)
                        || unaryExpression.Type.UnwrapNullableType() == operand.Type
                        || unaryExpression.Type.UnwrapNullableType() == typeof(Enum))
                    {
                        return sqlOperand;
                    }
                    sqlOperand = _sqlExpressionFactory.ApplyDefaultTypeMapping(sqlOperand);

                    return _sqlExpressionFactory.Convert(sqlOperand, unaryExpression.Type);
            }

            return null;
        }

        [DebuggerStepThrough]
        private bool TranslationFailed(Expression original, Expression translation)
        {
            return original != null && translation == null;
        }
    }
}
