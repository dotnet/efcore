// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalSqlTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;
        private readonly IMemberTranslatorProvider _memberTranslatorProvider;
        private readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;

        private SelectExpression _selectExpression;

        public RelationalSqlTranslatingExpressionVisitor(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
            _memberTranslatorProvider = memberTranslatorProvider;
            _methodCallTranslatorProvider = methodCallTranslatorProvider;
        }

        public SqlExpression Translate(SelectExpression selectExpression, Expression expression)
        {
            _selectExpression = selectExpression;

            var translation = (SqlExpression)Visit(expression);

            _selectExpression = null;

            return _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(translation, _typeMappingSource.FindMapping(expression.Type));
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);
            if (innerExpression is EntityShaperExpression entityShaper)
            {
                var entityType = entityShaper.EntityType;
                var property = entityType.FindProperty(memberExpression.Member.GetSimpleMemberName());

                return _selectExpression.BindProperty(entityShaper.ValueBufferExpression, property);
            }

            return TranslationFailed(memberExpression.Expression, innerExpression)
                ? null
                : _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                    _memberTranslatorProvider.Translate(
                        (SqlExpression)innerExpression, memberExpression.Member, memberExpression.Type),
                    null);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsEFPropertyMethod())
            {
                if (Visit(methodCallExpression.Arguments[0]) is EntityShaperExpression entityShaper)
                {
                    var entityType = entityShaper.EntityType;
                    var property = entityType.FindProperty((string)((ConstantExpression)methodCallExpression.Arguments[1]).Value);

                    return _selectExpression.BindProperty(entityShaper.ValueBufferExpression, property);
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
                : _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                    _methodCallTranslatorProvider.Translate(@object, methodCallExpression.Method, arguments),
                    null);
        }

        private static readonly MethodInfo _stringConcatObjectMethodInfo
            = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(object), typeof(object) });

        private static readonly MethodInfo _stringConcatStringMethodInfo
            = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var left = (SqlExpression)Visit(binaryExpression.Left);
            var right = (SqlExpression)Visit(binaryExpression.Right);

            if (TranslationFailed(binaryExpression.Left, left)
                || TranslationFailed(binaryExpression.Right, right))
            {
                return null;
            }

            if (binaryExpression.NodeType == ExpressionType.Add
                && (_stringConcatObjectMethodInfo.Equals(binaryExpression.Method)
                    || _stringConcatStringMethodInfo.Equals(binaryExpression.Method)))
            {
                return _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                    _methodCallTranslatorProvider.Translate(null, binaryExpression.Method, new[] { left, right }), null);
            }

            var newExpression = new SqlBinaryExpression(
                binaryExpression.NodeType,
                left,
                right,
                binaryExpression.Type,
                null);

            return _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(newExpression, null);
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
                return ((SelectExpression)projectionBindingExpression.QueryExpression)
                    .GetProjectionExpression(projectionBindingExpression.ProjectionMember);
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

            var newExpression = new CaseExpression(
                new List<CaseWhenClause>
                {
                    new CaseWhenClause(test, ifTrue)
                },
                ifFalse);

            return _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(newExpression, null);
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

            // In certain cases EF.Property would have convert node around the source.
            if (operand is EntityShaperExpression
                && unaryExpression.Type == typeof(object)
                && unaryExpression.NodeType == ExpressionType.Convert)
            {
                return operand;
            }

            var sqlOperand = (SqlExpression)operand;

            if (unaryExpression.NodeType == ExpressionType.Convert)
            {
                if (operand.Type.IsInterface
                    && unaryExpression.Type.GetInterfaces().Any(e => e == operand.Type)
                    || unaryExpression.Type.UnwrapNullableType() == operand.Type)
                {
                    return sqlOperand;
                }

                sqlOperand = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                        sqlOperand, _typeMappingSource.FindMapping(sqlOperand.Type));

                return new SqlCastExpression(
                    sqlOperand,
                    unaryExpression.Type,
                    null);
            }

            if (unaryExpression.NodeType == ExpressionType.Not)
            {
                return new SqlNotExpression(sqlOperand, _typeMappingSource.FindMapping(typeof(bool)));
            }

            return null;
        }

        private bool TranslationFailed(Expression original, Expression translation)
        {
            return original != null && translation == null;
        }
    }
}
