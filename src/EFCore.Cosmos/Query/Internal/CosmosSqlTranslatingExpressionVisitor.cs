// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CosmosSqlTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IMemberTranslatorProvider _memberTranslatorProvider;
        private readonly SqlTypeMappingVerifyingExpressionVisitor _sqlVerifyingExpressionVisitor;
        private readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression Translate(Expression expression)
        {
            var result = Visit(expression);

            if (result is SqlExpression translation)
            {
                translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(translation);

                if ((translation is SqlConstantExpression
                     || translation is SqlParameterExpression)
                    && translation.TypeMapping == null)
                {
                    // Non-mappable constant/parameter
                    return null;
                }

                _sqlVerifyingExpressionVisitor.Visit(translation);

                return translation;
            }

            return null;
        }

        private class SqlTypeMappingVerifyingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression node)
            {
                if (node is SqlExpression sqlExpression
                    && sqlExpression.TypeMapping == null)
                {
                    throw new InvalidOperationException("Null TypeMapping in Sql Tree");
                }

                return base.VisitExtension(node);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);

            if (TryBindMember(innerExpression, MemberIdentity.Create(memberExpression.Member), out var result))
            {
                return result;
            }

            return TranslationFailed(memberExpression.Expression, innerExpression)
                ? null
                : _memberTranslatorProvider.Translate((SqlExpression)innerExpression, memberExpression.Member, memberExpression.Type);
        }

        private bool TryBindMember(Expression source, MemberIdentity member, out Expression expression)
        {
            Type convertedType = null;
            if (source is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert)
            {
                if (unaryExpression.Type != typeof(object))
                {
                    convertedType = unaryExpression.Type;
                }
                source = unaryExpression.Operand;
            }

            if (source is EntityProjectionExpression entityProjectionExpression)
            {
                if (convertedType != null
                    && convertedType.IsInterface
                    && convertedType.IsAssignableFrom(entityProjectionExpression.Type))
                {
                    convertedType = entityProjectionExpression.Type;
                }

                expression = member.MemberInfo != null
                    ? entityProjectionExpression.BindMember(member.MemberInfo, convertedType, clientEval: false, out _)
                    : entityProjectionExpression.BindMember(member.Name, convertedType, clientEval: false, out _);
                return expression != null;
            }

            if (source is MemberExpression innerMemberExpression
                && TryBindMember(innerMemberExpression, MemberIdentity.Create(innerMemberExpression.Member), out var innerResult))
            {
                if (convertedType != null)
                {
                    innerResult = Expression.Convert(innerResult, convertedType);
                }

                return TryBindMember(innerResult, member, out expression);
            }

            expression = null;
            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                if (!TryBindMember(Visit(source), MemberIdentity.Create(propertyName), out var result))
                {
                    throw new InvalidOperationException($"Property {propertyName} not found on {source}");
                }

                return result;
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
            if (expression is UnaryExpression unaryExpression
                && (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked))
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

            return expression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.Coalesce)
            {
                return Visit(Expression.Condition(
                    Expression.NotEqual(binaryExpression.Left, Expression.Constant(null, binaryExpression.Left.Type)),
                    binaryExpression.Left,
                    binaryExpression.Right));
            }

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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var operand = Visit(unaryExpression.Operand);

            if (operand is EntityProjectionExpression)
            {
                return unaryExpression.Update(operand);
            }

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

        private SqlConstantExpression GetConstantOrNull(Expression expression)
        {
            if (CanEvaluate(expression))
            {
                var value = Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile().Invoke();
                return new SqlConstantExpression(Expression.Constant(value, expression.Type), null);
            }

            return null;
        }

        private static bool CanEvaluate(Expression expression)
        {
            switch (expression)
            {
                case ConstantExpression constantExpression:
                    return true;

                case NewExpression newExpression:
                    return newExpression.Arguments.All(e => CanEvaluate(e));

                case MemberInitExpression memberInitExpression:
                    return CanEvaluate(memberInitExpression.NewExpression)
                        && memberInitExpression.Bindings.All(
                            mb => mb is MemberAssignment memberAssignment && CanEvaluate(memberAssignment.Expression));

                default:
                    return false;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitNew(NewExpression node) => GetConstantOrNull(node);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMemberInit(MemberInitExpression node) => GetConstantOrNull(node);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitNewArray(NewArrayExpression node) => null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitListInit(ListInitExpression node) => null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitInvocation(InvocationExpression node) => null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitLambda<T>(Expression<T> node) => null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => new SqlConstantExpression(constantExpression, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitParameter(ParameterExpression parameterExpression)
            => new SqlParameterExpression(parameterExpression, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case EntityProjectionExpression _:
                case SqlExpression _:
                    return extensionExpression;

                case EntityShaperExpression entityShaperExpression:
                    return Visit(entityShaperExpression.ValueBufferExpression);

                case ProjectionBindingExpression projectionBindingExpression:
                    var selectExpression = (SelectExpression)projectionBindingExpression.QueryExpression;
                    return selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember);

                case NullConditionalExpression nullConditionalExpression:
                    return Visit(nullConditionalExpression.AccessOperation);

                default:
                    return null;
            }
        }

        [DebuggerStepThrough]
        private bool TranslationFailed(Expression original, Expression translation)
            => original != null && !(translation is SqlExpression);
    }
}
