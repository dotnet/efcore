// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
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
        private const string _runtimeParameterPrefix = QueryCompilationContext.QueryParameterPrefix + "entity_equality_";

        private static readonly MethodInfo _parameterValueExtractor =
            typeof(CosmosSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterValueExtractor));

        private static readonly MethodInfo _parameterListValueExtractor =
            typeof(CosmosSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterListValueExtractor));

        private static readonly MethodInfo _concatMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(object), typeof(object) });

        private static readonly MethodInfo _stringEqualsWithStringComparison
            = typeof(string).GetRuntimeMethod(nameof(string.Equals), new[] { typeof(string), typeof(StringComparison) });

        private static readonly MethodInfo _stringEqualsWithStringComparisonStatic
            = typeof(string).GetRuntimeMethod(nameof(string.Equals), new[] { typeof(string), typeof(string), typeof(StringComparison) });

        private readonly QueryCompilationContext _queryCompilationContext;
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
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] ISqlExpressionFactory sqlExpressionFactory,
            [NotNull] IMemberTranslatorProvider memberTranslatorProvider,
            [NotNull] IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            _queryCompilationContext = queryCompilationContext;
            _model = queryCompilationContext.Model;
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
        public virtual string TranslationErrorDetails { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void AddTranslationErrorDetails([NotNull] string details)
        {
            Check.NotNull(details, nameof(details));

            if (TranslationErrorDetails == null)
            {
                TranslationErrorDetails = details;
            }
            else
            {
                TranslationErrorDetails += Environment.NewLine + details;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression Translate([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            TranslationErrorDetails = null;

            return TranslateInternal(expression);
        }

        private SqlExpression TranslateInternal(Expression expression)
        {
            var result = Visit(expression);

            if (result is SqlExpression translation)
            {
                translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(translation);

                if (translation.TypeMapping == null)
                {
                    // The return type is not-mappable hence return null
                    return null;
                }

                _sqlVerifyingExpressionVisitor.Visit(translation);

                return translation;
            }

            return null;
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
                var ifTrue = binaryExpression.Left;
                var ifFalse = binaryExpression.Right;
                if (ifTrue.Type != ifFalse.Type)
                {
                    ifFalse = Expression.Convert(ifFalse, ifTrue.Type);
                }

                return Visit(
                    Expression.Condition(
                        Expression.NotEqual(ifTrue, Expression.Constant(null, ifTrue.Type)),
                        ifTrue,
                        ifFalse));
            }

            var left = TryRemoveImplicitConvert(binaryExpression.Left);
            var right = TryRemoveImplicitConvert(binaryExpression.Right);

            // Remove convert-to-object nodes if both sides have them, or if the other side is null constant
            var isLeftConvertToObject = TryUnwrapConvertToObject(left, out var leftOperand);
            var isRightConvertToObject = TryUnwrapConvertToObject(right, out var rightOperand);
            if (isLeftConvertToObject && isRightConvertToObject)
            {
                left = leftOperand;
                right = rightOperand;
            }
            else if (isLeftConvertToObject && right.IsNullConstantExpression())
            {
                left = leftOperand;
            }
            else if (isRightConvertToObject && left.IsNullConstantExpression())
            {
                right = rightOperand;
            }

            var visitedLeft = Visit(left);
            var visitedRight = Visit(right);

            if ((binaryExpression.NodeType == ExpressionType.Equal
                    || binaryExpression.NodeType == ExpressionType.NotEqual)
                // Visited expression could be null, We need to pass MemberInitExpression
                && TryRewriteEntityEquality(
                    binaryExpression.NodeType, visitedLeft ?? left, visitedRight ?? right, equalsMethod: false, out var result))
            {
                return result;
            }

            if (binaryExpression.Method == _concatMethodInfo)
            {
                return null;
            }

            var uncheckedNodeTypeVariant = binaryExpression.NodeType switch
            {
                ExpressionType.AddChecked => ExpressionType.Add,
                ExpressionType.SubtractChecked => ExpressionType.Subtract,
                ExpressionType.MultiplyChecked => ExpressionType.Multiply,
                _ => binaryExpression.NodeType
            };

            return TranslationFailed(binaryExpression.Left, visitedLeft, out var sqlLeft)
                || TranslationFailed(binaryExpression.Right, visitedRight, out var sqlRight)
                    ? null
                    : _sqlExpressionFactory.MakeBinary(
                        uncheckedNodeTypeVariant,
                        sqlLeft,
                        sqlRight,
                        null);

            static bool TryUnwrapConvertToObject(Expression expression, out Expression operand)
            {
                if (expression is UnaryExpression convertExpression
                    && (convertExpression.NodeType == ExpressionType.Convert
                        || convertExpression.NodeType == ExpressionType.ConvertChecked)
                    && expression.Type == typeof(object))
                {
                    operand = convertExpression.Operand;
                    return true;
                }

                operand = null;
                return false;
            }
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
        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => new SqlConstantExpression(Check.NotNull(constantExpression, nameof(constantExpression)), null);

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
                case EntityReferenceExpression _:
                case SqlExpression _:
                    return extensionExpression;

                case EntityShaperExpression entityShaperExpression:
                    var result = Visit(entityShaperExpression.ValueBufferExpression);

                    if (result.NodeType == ExpressionType.Convert
                        && result.Type == typeof(ValueBuffer)
                        && result is UnaryExpression outerUnary
                        && outerUnary.Operand.NodeType == ExpressionType.Convert
                        && outerUnary.Operand.Type == typeof(object))
                    {
                        result = ((UnaryExpression)outerUnary.Operand).Operand;
                    }

                    if (result is EntityProjectionExpression entityProjectionExpression)
                    {
                        return new EntityReferenceExpression(entityProjectionExpression);
                    }

                    return null;

                case ProjectionBindingExpression projectionBindingExpression:
                    return projectionBindingExpression.ProjectionMember != null
                        ? ((SelectExpression)projectionBindingExpression.QueryExpression)
                        .GetMappedProjection(projectionBindingExpression.ProjectionMember)
                        : null;

                default:
                    return null;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitInvocation(InvocationExpression invocationExpression)
            => null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
            => null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitListInit(ListInitExpression listInitExpression)
            => null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            var innerExpression = Visit(memberExpression.Expression);

            return TryBindMember(innerExpression, MemberIdentity.Create(memberExpression.Member))
                ?? (TranslationFailed(memberExpression.Expression, innerExpression, out var sqlInnerExpression)
                    ? null
                    : _memberTranslatorProvider.Translate(
                        sqlInnerExpression, memberExpression.Member, memberExpression.Type, _queryCompilationContext.Logger));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
            => GetConstantOrNull(Check.NotNull(memberInitExpression, nameof(memberInitExpression)));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName)
                || methodCallExpression.TryGetIndexerArguments(_model, out source, out propertyName))
            {
                return TryBindMember(Visit(source), MemberIdentity.Create(propertyName));
            }

            SqlExpression sqlObject = null;
            SqlExpression[] arguments;
            var method = methodCallExpression.Method;

            if (method.Name == nameof(object.Equals)
                && methodCallExpression.Object != null
                && methodCallExpression.Arguments.Count == 1)
            {
                var left = Visit(methodCallExpression.Object);
                var right = Visit(RemoveObjectConvert(methodCallExpression.Arguments[0]));

                if (TryRewriteEntityEquality(
                    ExpressionType.Equal,
                    left ?? methodCallExpression.Object,
                    right ?? methodCallExpression.Arguments[0],
                    equalsMethod: true,
                    out var result))
                {
                    return result;
                }

                if (left is SqlExpression leftSql
                    && right is SqlExpression rightSql)
                {
                    sqlObject = leftSql;
                    arguments = new SqlExpression[1] { rightSql };
                }
                else
                {
                    return null;
                }
            }
            else if (method.Name == nameof(object.Equals)
                && methodCallExpression.Object == null
                && methodCallExpression.Arguments.Count == 2)
            {
                var left = Visit(RemoveObjectConvert(methodCallExpression.Arguments[0]));
                var right = Visit(RemoveObjectConvert(methodCallExpression.Arguments[1]));

                if (TryRewriteEntityEquality(
                    ExpressionType.Equal,
                    left ?? methodCallExpression.Arguments[0],
                    right ?? methodCallExpression.Arguments[1],
                    equalsMethod: true,
                    out var result))
                {
                    return result;
                }

                if (left is SqlExpression leftSql
                    && right is SqlExpression rightSql)
                {
                    arguments = new SqlExpression[2] { leftSql, rightSql };
                }
                else
                {
                    return null;
                }
            }
            else if (method.IsGenericMethod
                && method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains))
            {
                var enumerable = Visit(methodCallExpression.Arguments[0]);
                var item = Visit(methodCallExpression.Arguments[1]);

                if (TryRewriteContainsEntity(enumerable, item ?? methodCallExpression.Arguments[1], out var result))
                {
                    return result;
                }

                if (enumerable is SqlExpression sqlEnumerable
                    && item is SqlExpression sqlItem)
                {
                    arguments = new SqlExpression[2] { sqlEnumerable, sqlItem };
                }
                else
                {
                    return null;
                }
            }
            else if (methodCallExpression.Arguments.Count == 1
                && method.IsContainsMethod())
            {
                var enumerable = Visit(methodCallExpression.Object);
                var item = Visit(methodCallExpression.Arguments[0]);

                if (TryRewriteContainsEntity(enumerable, item ?? methodCallExpression.Arguments[0], out var result))
                {
                    return result;
                }

                if (enumerable is SqlExpression sqlEnumerable
                    && item is SqlExpression sqlItem)
                {
                    sqlObject = sqlEnumerable;
                    arguments = new SqlExpression[1] { sqlItem };
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (TranslationFailed(methodCallExpression.Object, Visit(methodCallExpression.Object), out sqlObject))
                {
                    return null;
                }

                arguments = new SqlExpression[methodCallExpression.Arguments.Count];
                for (var i = 0; i < arguments.Length; i++)
                {
                    var argument = methodCallExpression.Arguments[i];
                    if (TranslationFailed(argument, Visit(argument), out var sqlArgument))
                    {
                        return null;
                    }

                    arguments[i] = sqlArgument;
                }
            }

            var translation = _methodCallTranslatorProvider.Translate(
                _model, sqlObject, methodCallExpression.Method, arguments, _queryCompilationContext.Logger);

            if (translation == null)
            {
                if (methodCallExpression.Method == _stringEqualsWithStringComparison
                    || methodCallExpression.Method == _stringEqualsWithStringComparisonStatic)
                {
                    AddTranslationErrorDetails(CoreStrings.QueryUnableToTranslateStringEqualsWithStringComparison);
                }
                else
                {
                    AddTranslationErrorDetails(
                        CoreStrings.QueryUnableToTranslateMethod(
                            methodCallExpression.Method.DeclaringType?.DisplayName(),
                            methodCallExpression.Method.Name));
                }
            }

            return translation;

            static Expression RemoveObjectConvert(Expression expression)
                => expression is UnaryExpression unaryExpression
                    && (unaryExpression.NodeType == ExpressionType.Convert || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                    && unaryExpression.Type == typeof(object)
                        ? unaryExpression.Operand
                        : expression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitNew(NewExpression newExpression)
            => GetConstantOrNull(Check.NotNull(newExpression, nameof(newExpression)));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
            => null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitParameter(ParameterExpression parameterExpression)
            => parameterExpression.Name?.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal) == true
                ? new SqlParameterExpression(Check.NotNull(parameterExpression, nameof(parameterExpression)), null)
                : null;

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

            if (operand is EntityReferenceExpression entityReferenceExpression
                && (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked
                    || unaryExpression.NodeType == ExpressionType.TypeAs))
            {
                return entityReferenceExpression.Convert(unaryExpression.Type);
            }

            if (TranslationFailed(unaryExpression.Operand, operand, out var sqlOperand))
            {
                return null;
            }

            switch (unaryExpression.NodeType)
            {
                case ExpressionType.Not:
                    return _sqlExpressionFactory.Not(sqlOperand);

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return _sqlExpressionFactory.Negate(sqlOperand);

                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    if (operand.Type.IsInterface
                        && unaryExpression.Type.GetInterfaces().Any(e => e == operand.Type)
                        || unaryExpression.Type.UnwrapNullableType() == operand.Type
                        || unaryExpression.Type.UnwrapNullableType() == typeof(Enum)
                        // Object convert needs to be converted to explicit cast when mismatching types
                        // But we let is pass here since we don't have explicit cast mechanism here and in some cases object convert is due to value types
                        || unaryExpression.Type == typeof(object))
                    {
                        return sqlOperand;
                    }

                    break;
            }

            return null;
        }

        /// <inheritdoc />
        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
        {
            Check.NotNull(typeBinaryExpression, nameof(typeBinaryExpression));

            var innerExpression = Visit(typeBinaryExpression.Expression);

            if (typeBinaryExpression.NodeType == ExpressionType.TypeIs
                && innerExpression is EntityReferenceExpression entityReferenceExpression)
            {
                var entityType = entityReferenceExpression.EntityType;
                if (entityType.GetAllBaseTypesInclusive().Any(et => et.ClrType == typeBinaryExpression.TypeOperand))
                {
                    return _sqlExpressionFactory.Constant(true);
                }

                var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == typeBinaryExpression.TypeOperand);
                if (derivedType != null
                    && TryBindMember(
                        entityReferenceExpression,
                        MemberIdentity.Create(entityType.GetDiscriminatorProperty().Name)) is SqlExpression discriminatorColumn)
                {
                    var concreteEntityTypes = derivedType.GetConcreteDerivedTypesInclusive().ToList();

                    return concreteEntityTypes.Count == 1
                        ? _sqlExpressionFactory.Equal(
                            discriminatorColumn,
                            _sqlExpressionFactory.Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
                        : (SqlExpression)_sqlExpressionFactory.In(
                            discriminatorColumn,
                            _sqlExpressionFactory.Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()),
                            negated: false);
                }
            }

            return null;
        }

        private Expression TryBindMember(Expression source, MemberIdentity member)
        {
            if (!(source is EntityReferenceExpression entityReferenceExpression))
            {
                return null;
            }

            var result = member.MemberInfo != null
                ? entityReferenceExpression.ParameterEntity.BindMember(
                    member.MemberInfo, entityReferenceExpression.Type, clientEval: false, out _)
                : entityReferenceExpression.ParameterEntity.BindMember(
                    member.Name, entityReferenceExpression.Type, clientEval: false, out _);

            if (result == null)
            {
                AddTranslationErrorDetails(
                    CoreStrings.QueryUnableToTranslateMember(
                        member.Name,
                        entityReferenceExpression.EntityType.DisplayName()));
            }

            return result switch
            {
                EntityProjectionExpression entityProjectionExpression => new EntityReferenceExpression(entityProjectionExpression),
                ObjectArrayProjectionExpression objectArrayProjectionExpression
                => new EntityReferenceExpression(objectArrayProjectionExpression.InnerProjection),
                _ => result
            };
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

        private bool TryRewriteContainsEntity(Expression source, Expression item, out Expression result)
        {
            result = null;

            if (!(item is EntityReferenceExpression itemEntityReference))
            {
                return false;
            }

            var entityType = itemEntityReference.EntityType;
            var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties;
            if (primaryKeyProperties == null)
            {
                throw new InvalidOperationException(CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                    nameof(Queryable.Contains), entityType.DisplayName()));
            }

            if (primaryKeyProperties.Count > 1)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported(nameof(Queryable.Contains), entityType.DisplayName()));
            }

            var property = primaryKeyProperties[0];
            Expression rewrittenSource;
            switch (source)
            {
                case SqlConstantExpression sqlConstantExpression:
                    var values = (IEnumerable)sqlConstantExpression.Value;
                    var propertyValueList =
                        (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(property.ClrType.MakeNullable()));
                    var propertyGetter = property.GetGetter();
                    foreach (var value in values)
                    {
                        propertyValueList.Add(propertyGetter.GetClrValue(value));
                    }

                    rewrittenSource = Expression.Constant(propertyValueList);
                    break;

                case SqlParameterExpression sqlParameterExpression
                    when sqlParameterExpression.Name.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal):
                    var lambda = Expression.Lambda(
                        Expression.Call(
                            _parameterListValueExtractor.MakeGenericMethod(entityType.ClrType, property.ClrType.MakeNullable()),
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(sqlParameterExpression.Name, typeof(string)),
                            Expression.Constant(property, typeof(IProperty))),
                        QueryCompilationContext.QueryContextParameter
                    );

                    var newParameterName =
                        $"{_runtimeParameterPrefix}"
                        + $"{sqlParameterExpression.Name.Substring(QueryCompilationContext.QueryParameterPrefix.Length)}_{property.Name}";

                    rewrittenSource = _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
                    break;

                default:
                    return false;
            }

            result = Visit(
                Expression.Call(
                    EnumerableMethods.Contains.MakeGenericMethod(property.ClrType.MakeNullable()),
                    rewrittenSource,
                    CreatePropertyAccessExpression(item, property)));

            return true;
        }

        private bool TryRewriteEntityEquality(ExpressionType nodeType, Expression left, Expression right, bool equalsMethod, out Expression result)
        {
            var leftEntityReference = left as EntityReferenceExpression;
            var rightEntityReference = right as EntityReferenceExpression;

            if (leftEntityReference == null
                && rightEntityReference == null)
            {
                result = null;
                return false;
            }

            if (IsNullSqlConstantExpression(left)
                || IsNullSqlConstantExpression(right))
            {
                var nonNullEntityReference = IsNullSqlConstantExpression(left) ? rightEntityReference : leftEntityReference;
                var entityType1 = nonNullEntityReference.EntityType;
                var primaryKeyProperties1 = entityType1.FindPrimaryKey()?.Properties;
                if (primaryKeyProperties1 == null)
                {
                    throw new InvalidOperationException(CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                        nodeType == ExpressionType.Equal
                            ? equalsMethod ? nameof(object.Equals) : "=="
                            : equalsMethod ? "!" + nameof(object.Equals) : "!=",
                        entityType1.DisplayName()));
                }

                result = Visit(
                    primaryKeyProperties1.Select(
                            p =>
                                Expression.MakeBinary(
                                    nodeType, CreatePropertyAccessExpression(nonNullEntityReference, p),
                                    Expression.Constant(null, p.ClrType.MakeNullable())))
                        .Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.OrElse(l, r) : Expression.AndAlso(l, r)));

                return true;
            }

            var leftEntityType = leftEntityReference?.EntityType;
            var rightEntityType = rightEntityReference?.EntityType;
            var entityType = leftEntityType ?? rightEntityType;

            Debug.Assert(entityType != null, "At least either side should be entityReference so entityType should be non-null.");

            if (leftEntityType != null
                && rightEntityType != null
                && leftEntityType.GetRootType() != rightEntityType.GetRootType())
            {
                result = _sqlExpressionFactory.Constant(false);
                return true;
            }

            var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties;
            if (primaryKeyProperties == null)
            {
                throw new InvalidOperationException(CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                    nodeType == ExpressionType.Equal
                        ? equalsMethod ? nameof(object.Equals) : "=="
                        : equalsMethod ? "!" + nameof(object.Equals) : "!=",
                    entityType.DisplayName()));
            }

            result = Visit(
                primaryKeyProperties.Select(
                        p =>
                            Expression.MakeBinary(
                                nodeType,
                                CreatePropertyAccessExpression(left, p),
                                CreatePropertyAccessExpression(right, p)))
                    .Aggregate((l, r) => Expression.AndAlso(l, r)));

            return true;
        }

        private Expression CreatePropertyAccessExpression(Expression target, IProperty property)
        {
            switch (target)
            {
                case SqlConstantExpression sqlConstantExpression:
                    return Expression.Constant(
                        property.GetGetter().GetClrValue(sqlConstantExpression.Value), property.ClrType.MakeNullable());

                case SqlParameterExpression sqlParameterExpression
                    when sqlParameterExpression.Name.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal):
                    var lambda = Expression.Lambda(
                        Expression.Call(
                            _parameterValueExtractor.MakeGenericMethod(property.ClrType.MakeNullable()),
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(sqlParameterExpression.Name, typeof(string)),
                            Expression.Constant(property, typeof(IProperty))),
                        QueryCompilationContext.QueryContextParameter);

                    var newParameterName =
                        $"{_runtimeParameterPrefix}"
                        + $"{sqlParameterExpression.Name.Substring(QueryCompilationContext.QueryParameterPrefix.Length)}_{property.Name}";

                    return _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);

                case MemberInitExpression memberInitExpression
                    when memberInitExpression.Bindings.SingleOrDefault(
                        mb => mb.Member.Name == property.Name) is MemberAssignment memberAssignment:
                    return memberAssignment.Expression;

                default:
                    return target.CreateEFPropertyExpression(property);
            }
        }

        private static T ParameterValueExtractor<T>(QueryContext context, string baseParameterName, IProperty property)
        {
            var baseParameter = context.ParameterValues[baseParameterName];
            return baseParameter == null ? (T)(object)null : (T)property.GetGetter().GetClrValue(baseParameter);
        }

        private static List<TProperty> ParameterListValueExtractor<TEntity, TProperty>(
            QueryContext context,
            string baseParameterName,
            IProperty property)
        {
            if (!(context.ParameterValues[baseParameterName] is IEnumerable<TEntity> baseListParameter))
            {
                return null;
            }

            var getter = property.GetGetter();
            return baseListParameter.Select(e => e != null ? (TProperty)getter.GetClrValue(e) : (TProperty)(object)null).ToList();
        }

        private static bool IsNullSqlConstantExpression(Expression expression)
            => expression is SqlConstantExpression sqlConstant && sqlConstant.Value == null;

        private SqlConstantExpression GetConstantOrNull(Expression expression)
            => CanEvaluate(expression)
                ? new SqlConstantExpression(
                    Expression.Constant(
                        Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile().Invoke(),
                        expression.Type),
                    null)
                : null;

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

        private sealed class EntityReferenceExpression : Expression
        {
            public EntityReferenceExpression(EntityProjectionExpression parameter)
            {
                ParameterEntity = parameter;
                EntityType = parameter.EntityType;
                Type = EntityType.ClrType;
            }

            private EntityReferenceExpression(EntityProjectionExpression parameter, Type type)
            {
                ParameterEntity = parameter;
                EntityType = parameter.EntityType;
                Type = type;
            }

            public EntityProjectionExpression ParameterEntity { get; }
            public IEntityType EntityType { get; }

            public override Type Type { get; }

            public override ExpressionType NodeType
                => ExpressionType.Extension;

            public Expression Convert(Type type)
            {
                return type == typeof(object) // Ignore object conversion
                    || type.IsAssignableFrom(Type) // Ignore conversion to base/interface
                        ? this
                        : new EntityReferenceExpression(ParameterEntity, type);
            }
        }

        private sealed class SqlTypeMappingVerifyingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression extensionExpression)
            {
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                if (extensionExpression is SqlExpression sqlExpression
                    && sqlExpression.TypeMapping == null)
                {
                    throw new InvalidOperationException(CosmosStrings.NullTypeMappingInSqlTree(sqlExpression.Print()));
                }

                return base.VisitExtension(extensionExpression);
            }
        }
    }
}
