// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

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
            [NotNull] IModel model,
            [NotNull] ISqlExpressionFactory sqlExpressionFactory,
            [NotNull] IMemberTranslatorProvider memberTranslatorProvider,
            [NotNull] IMethodCallTranslatorProvider methodCallTranslatorProvider)
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
        public virtual SqlExpression Translate([NotNull] Expression expression)
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

        private sealed class SqlTypeMappingVerifyingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression node)
            {
                Check.NotNull(node, nameof(node));

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
            Check.NotNull(memberExpression, nameof(memberExpression));

            return TryBindMember(memberExpression.Expression, MemberIdentity.Create(memberExpression.Member), out var result)
                ? result
                : TranslationFailed(memberExpression.Expression, Visit(memberExpression.Expression), out var sqlInnerExpression)
                    ? null
                    : _memberTranslatorProvider.Translate(sqlInnerExpression, memberExpression.Member, memberExpression.Type);
        }

        private bool TryBindMember(Expression source, MemberIdentity member, out Expression expression)
        {
            source = source.UnwrapTypeConversion(out var convertedType);
            Expression visitedExpression;
            switch (source)
            {
                case EntityShaperExpression entityShaperExpression:
                    visitedExpression = Visit(entityShaperExpression.ValueBufferExpression);
                    break;

                case MemberExpression memberExpression:
                    TryBindMember(memberExpression.Expression, MemberIdentity.Create(memberExpression.Member), out visitedExpression);
                    break;

                case MethodCallExpression methodCallExpression
                    when methodCallExpression.TryGetEFPropertyArguments(out var innerSource, out var innerPropertyName):
                    TryBindMember(innerSource, MemberIdentity.Create(innerPropertyName), out visitedExpression);
                    break;

                default:
                    visitedExpression = null;
                    break;
            }

            if (visitedExpression is EntityProjectionExpression entityProjectionExpression)
            {
                convertedType ??= entityProjectionExpression.Type;
                expression = member.MemberInfo != null
                    ? entityProjectionExpression.BindMember(member.MemberInfo, convertedType, clientEval: false, out _)
                    : entityProjectionExpression.BindMember(member.Name, convertedType, clientEval: false, out _);

                return expression != null;
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
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
            {
                return TryBindMember(source, MemberIdentity.Create(propertyName), out var result)
                    ? result
                    : null;
            }

            if (TranslationFailed(methodCallExpression.Object, Visit(methodCallExpression.Object), out var sqlObject))
            {
                return null;
            }

            var arguments = new SqlExpression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = methodCallExpression.Arguments[i];
                if (TranslationFailed(argument, Visit(argument), out var sqlArgument))
                {
                    return null;
                }

                arguments[i] = sqlArgument;
            }

            return _methodCallTranslatorProvider.Translate(_model, sqlObject, methodCallExpression.Method, arguments);
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
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            if (binaryExpression.NodeType == ExpressionType.Coalesce)
            {
                return Visit(
                    Expression.Condition(
                        Expression.NotEqual(binaryExpression.Left, Expression.Constant(null, binaryExpression.Left.Type)),
                        binaryExpression.Left,
                        binaryExpression.Right));
            }

            var left = TryRemoveImplicitConvert(binaryExpression.Left);
            var right = TryRemoveImplicitConvert(binaryExpression.Right);

            left = Visit(left);
            right = Visit(right);

            return TranslationFailed(binaryExpression.Left, left, out var sqlLeft)
                || TranslationFailed(binaryExpression.Right, right, out var sqlRight)
                ? null
                : _sqlExpressionFactory.MakeBinary(
                    binaryExpression.NodeType,
                    sqlLeft,
                    sqlRight,
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
            Check.NotNull(conditionalExpression, nameof(conditionalExpression));

            var test = Visit(conditionalExpression.Test);
            var ifTrue = Visit(conditionalExpression.IfTrue);
            var ifFalse = Visit(conditionalExpression.IfFalse);

            return TranslationFailed(conditionalExpression.Test, test, out var sqlTest)
                || TranslationFailed(conditionalExpression.IfTrue, ifTrue, out var sqlIfTrue)
                || TranslationFailed(conditionalExpression.IfFalse, ifFalse, out var sqlIfFalse)
                ? null
                : _sqlExpressionFactory.Condition(sqlTest, sqlIfTrue, sqlIfFalse);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            Check.NotNull(unaryExpression, nameof(unaryExpression));

            var operand = Visit(unaryExpression.Operand);

            if (TranslationFailed(unaryExpression.Operand, operand, out var sqlOperand))
            {
                return null;
            }

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
#pragma warning disable IDE0066 // Convert switch statement to expression
            switch (expression)
#pragma warning restore IDE0066 // Convert switch statement to expression
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
        protected override Expression VisitNew(NewExpression node)
        {
            Check.NotNull(node, nameof(node));

            return GetConstantOrNull(node);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            Check.NotNull(node, nameof(node));

            return GetConstantOrNull(node);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            Check.NotNull(node, nameof(node));

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitListInit(ListInitExpression node)
        {
            Check.NotNull(node, nameof(node));

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            Check.NotNull(node, nameof(node));

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Check.NotNull(node, nameof(node));

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            Check.NotNull(constantExpression, nameof(constantExpression));

            return new SqlConstantExpression(constantExpression, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            Check.NotNull(parameterExpression, nameof(parameterExpression));

            return new SqlParameterExpression(parameterExpression, null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            switch (extensionExpression)
            {
                case EntityProjectionExpression _:
                case SqlExpression _:
                    return extensionExpression;

                case EntityShaperExpression entityShaperExpression:
                    return Visit(entityShaperExpression.ValueBufferExpression);

                case ProjectionBindingExpression projectionBindingExpression:
                    return projectionBindingExpression.ProjectionMember != null
                        ? ((SelectExpression)projectionBindingExpression.QueryExpression)
                            .GetMappedProjection(projectionBindingExpression.ProjectionMember)
                        : null;

                default:
                    return null;
            }
        }

        [DebuggerStepThrough]
        private bool TranslationFailed(Expression original, Expression translation, out SqlExpression castTranslation)
        {
            if (original != null
                && !(translation is SqlExpression))
            {
                castTranslation = null;
                return true;
            }

            castTranslation = translation as SqlExpression;
            return false;
        }
    }
}
