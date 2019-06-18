// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class CosmosSqlTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IMemberTranslatorProvider _memberTranslatorProvider;
        private readonly SqlTypeMappingVerifyingExpressionVisitor _sqlVerifyingExpressionVisitor;
        private readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;

        public CosmosSqlTranslatingExpressionVisitor(
            IModel model,
            ISqlExpressionFactory sqlExpressionFactory,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            _model = model;
            _sqlExpressionFactory = sqlExpressionFactory;
            _memberTranslatorProvider = memberTranslatorProvider;
            _methodCallTranslatorProvider = methodCallTranslatorProvider;
            _sqlVerifyingExpressionVisitor = new SqlTypeMappingVerifyingExpressionVisitor();
        }

        public SqlExpression Translate(Expression expression)
        {
            var translation = (SqlExpression)Visit(expression);

            translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(translation);

            _sqlVerifyingExpressionVisitor.Visit(translation);

            return translation;
        }

        private class SqlTypeMappingVerifyingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression node)
            {
                if (node is SqlExpression sqlExpression)
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
            if (memberExpression.Expression is EntityShaperExpression)
            {
                return BindProperty(memberExpression.Expression, memberExpression.Member.GetSimpleMemberName());
            }

            var innerExpression = Visit(memberExpression.Expression);

            return TranslationFailed(memberExpression.Expression, innerExpression)
                ? null
                : _memberTranslatorProvider.Translate((SqlExpression)innerExpression, memberExpression.Member, memberExpression.Type);
        }

        private SqlExpression BindProperty(Expression source, string propertyName)
        {
            if (source is EntityShaperExpression entityShaper)
            {
                var entityType = entityShaper.EntityType;

                return BindProperty(entityShaper, entityType.FindProperty(propertyName));
            }

            throw new InvalidOperationException();
        }

        private SqlExpression BindProperty(EntityShaperExpression entityShaper, IProperty property)
        {
            return ((SelectExpression)entityShaper.ValueBufferExpression.QueryExpression)
                .BindProperty(entityShaper.ValueBufferExpression, property);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                return BindProperty(source, propertyName);
            }

            //if (methodCallExpression.Method.DeclaringType == typeof(Queryable))
            //{
            //    var translation = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);

            //    var subquery = (SelectExpression)translation.QueryExpression;
            //    subquery.ApplyProjection();

            //    if (methodCallExpression.Method.Name == nameof(Queryable.Any)
            //        || methodCallExpression.Method.Name == nameof(Queryable.All)
            //        || methodCallExpression.Method.Name == nameof(Queryable.Contains))
            //    {
            //        if (subquery.Tables.Count == 0
            //            && subquery.Projection.Count == 1)
            //        {
            //            return subquery.Projection[0].Expression;
            //        }

            //        throw new InvalidOperationException();
            //    }

            //    return new SubSelectExpression(subquery);
            //}

            var @object = Visit(methodCallExpression.Object);
            if (TranslationFailed(methodCallExpression.Object, @object))
            {
                return null;
            }

            var arguments = new SqlExpression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = Visit(methodCallExpression.Arguments[i]);
                if (TranslationFailed(methodCallExpression.Arguments[i], argument))
                {
                    return null;
                }
                arguments[i] = (SqlExpression)argument;
            }

            return _methodCallTranslatorProvider.Translate(_model, (SqlExpression)@object, methodCallExpression.Method, arguments);
        }

        private static Expression TryRemoveImplicitConvert(Expression expression)
        {
            if (expression is UnaryExpression unaryExpression)
            {
                if (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                {
                    var innerType = unaryExpression.Operand.Type.UnwrapNullableType();
                    if (innerType.IsEnum)
                    {
                        innerType = Enum.GetUnderlyingType(innerType);
                    }
                    var convertedType = unaryExpression.Type.UnwrapNullableType();

                    if (innerType == convertedType
                        || (convertedType == typeof(int)
                            && (innerType == typeof(byte)
                                || innerType == typeof(sbyte)
                                || innerType == typeof(char)
                                || innerType == typeof(short)
                                || innerType == typeof(ushort))))
                    {
                        return TryRemoveImplicitConvert(unaryExpression.Operand);
                    }
                }
            }

            return expression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var left = TryRemoveImplicitConvert(binaryExpression.Left);
            var right = TryRemoveImplicitConvert(binaryExpression.Right);

            left = Visit(left);
            right = Visit(right);

            if (TranslationFailed(binaryExpression.Left, left)
                || TranslationFailed(binaryExpression.Right, right))
            {
                return null;
            }

            return _sqlExpressionFactory.MakeBinary(
                binaryExpression.NodeType,
                (SqlExpression)left,
                (SqlExpression)right,
                null);
        }

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            var test = Visit(conditionalExpression.Test);
            var ifTrue = Visit(conditionalExpression.IfTrue);
            var ifFalse = Visit(conditionalExpression.IfFalse);

            if (TranslationFailed(conditionalExpression.Test, test)
                || TranslationFailed(conditionalExpression.IfTrue, ifTrue)
                || TranslationFailed(conditionalExpression.IfFalse, ifFalse))
            {
                return null;
            }

            return _sqlExpressionFactory.Condition((SqlExpression)test, (SqlExpression)ifTrue, (SqlExpression)ifFalse);
        }

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

                    //// Introduce explicit cast only if the target type is mapped else we need to client eval
                    //if (unaryExpression.Type == typeof(object)
                    //    || _sqlExpressionFactory.FindMapping(unaryExpression.Type) != null)
                    //{
                    //    sqlOperand = _sqlExpressionFactory.ApplyDefaultTypeMapping(sqlOperand);

                    //    return _sqlExpressionFactory.Convert(sqlOperand, unaryExpression.Type);
                    //}

                    break;
            }

            return null;
        }

        protected override Expression VisitNew(NewExpression node) => null;

        protected override Expression VisitMemberInit(MemberInitExpression node) => null;

        protected override Expression VisitNewArray(NewArrayExpression node) => null;

        protected override Expression VisitListInit(ListInitExpression node) => null;

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

                return selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember);
            }

            if (extensionExpression is NullConditionalExpression nullConditionalExpression)
            {
                return Visit(nullConditionalExpression.AccessOperation);
            }

            return base.VisitExtension(extensionExpression);
        }

        [DebuggerStepThrough]
        private bool TranslationFailed(Expression original, Expression translation)
            => original != null && !(translation is SqlExpression);
    }
}
