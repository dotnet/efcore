// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DebuggerStepThrough]
    public static class ExpressionExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsNullConstantExpression([NotNull] this Expression expression)
            => expression.RemoveConvert() is ConstantExpression constantExpression
               && constantExpression.Value == null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyInfo GetPropertyAccess([NotNull] this LambdaExpression propertyAccessExpression)
        {
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            var parameterExpression = propertyAccessExpression.Parameters.Single();
            var propertyInfo = parameterExpression.MatchSimplePropertyAccess(propertyAccessExpression.Body);

            if (propertyInfo == null)
            {
                throw new ArgumentException(
                    CoreStrings.InvalidPropertyExpression(propertyAccessExpression),
                    nameof(propertyAccessExpression));
            }

            var declaringType = propertyInfo.DeclaringType;
            var parameterType = parameterExpression.Type;

            if (declaringType != null
                && declaringType != parameterType
                && declaringType.GetTypeInfo().IsInterface
                && declaringType.GetTypeInfo().IsAssignableFrom(parameterType.GetTypeInfo()))
            {
                var propertyGetter = propertyInfo.GetMethod;
                var interfaceMapping = parameterType.GetTypeInfo().GetRuntimeInterfaceMap(declaringType);
                var index = Array.FindIndex(interfaceMapping.InterfaceMethods, p => propertyGetter.Equals(p));
                var targetMethod = interfaceMapping.TargetMethods[index];
                foreach (var runtimeProperty in parameterType.GetRuntimeProperties())
                {
                    if (targetMethod.Equals(runtimeProperty.GetMethod))
                    {
                        return runtimeProperty;
                    }
                }
            }

            return propertyInfo;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IReadOnlyList<PropertyInfo> GetPropertyAccessList([NotNull] this LambdaExpression propertyAccessExpression)
        {
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            var propertyPaths
                = MatchPropertyAccessList(propertyAccessExpression, (p, e) => e.MatchSimplePropertyAccess(p));

            if (propertyPaths == null)
            {
                throw new ArgumentException(
                    CoreStrings.InvalidPropertiesExpression(propertyAccessExpression),
                    nameof(propertyAccessExpression));
            }

            return propertyPaths;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private static IReadOnlyList<PropertyInfo> MatchPropertyAccessList(
            this LambdaExpression lambdaExpression, Func<Expression, Expression, PropertyInfo> propertyMatcher)
        {
            Debug.Assert(lambdaExpression.Body != null);

            var parameterExpression
                = lambdaExpression.Parameters.Single();

            if (RemoveConvert(lambdaExpression.Body) is NewExpression newExpression)
            {
                var propertyInfos
                    = newExpression
                        .Arguments
                        .Select(a => propertyMatcher(a, parameterExpression))
                        .Where(p => p != null)
                        .ToList();

                return propertyInfos.Count != newExpression.Arguments.Count ? null : propertyInfos;
            }

            var propertyPath
                = propertyMatcher(lambdaExpression.Body, parameterExpression);

            return propertyPath != null ? new[] { propertyPath } : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private static PropertyInfo MatchSimplePropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            var propertyInfos = MatchPropertyAccess(parameterExpression, propertyAccessExpression);

            return propertyInfos != null && propertyInfos.Count == 1 ? propertyInfos[0] : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IReadOnlyList<PropertyInfo> GetComplexPropertyAccess(
            [NotNull] this LambdaExpression propertyAccessExpression,
            [NotNull] string methodName)
        {
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            var propertyPath
                = propertyAccessExpression
                    .Parameters
                    .Single()
                    .MatchPropertyAccess(propertyAccessExpression.Body);

            if (propertyPath == null)
            {
                throw new ArgumentException(
                    CoreStrings.InvalidIncludeLambdaExpression(methodName, propertyAccessExpression));
            }

            return propertyPath;
        }

        private static IReadOnlyList<PropertyInfo> MatchPropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            var propertyInfos = new List<PropertyInfo>();

            MemberExpression memberExpression;

            do
            {
                memberExpression = RemoveTypeAs(RemoveConvert(propertyAccessExpression)) as MemberExpression;

                var propertyInfo = memberExpression?.Member as PropertyInfo;

                if (propertyInfo == null)
                {
                    return null;
                }

                propertyInfos.Insert(0, propertyInfo);

                propertyAccessExpression = memberExpression.Expression;
            }
            while (RemoveTypeAs(RemoveConvert(memberExpression.Expression)) != parameterExpression);

            return propertyInfos;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Expression RemoveConvert([CanBeNull] this Expression expression)
        {
            while (expression != null
                   && (expression.NodeType == ExpressionType.Convert
                       || expression.NodeType == ExpressionType.ConvertChecked))
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }

            return expression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Expression RemoveTypeAs([CanBeNull] this Expression expression)
        {
            while (expression != null
                   && (expression.NodeType == ExpressionType.TypeAs))
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }

            return expression;
        }


        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static TExpression GetRootExpression<TExpression>([NotNull] this Expression expression)
            where TExpression : Expression
        {
            while (expression is MemberExpression memberExpression)
            {
                expression = memberExpression.Expression;
            }

            return expression as TExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsLogicalOperation([NotNull] this Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return expression.NodeType == ExpressionType.AndAlso
                   || expression.NodeType == ExpressionType.OrElse;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsComparisonOperation([NotNull] this Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return expression.Type == typeof(bool)
                   && (expression.NodeType == ExpressionType.Equal
                       || expression.NodeType == ExpressionType.NotEqual
                       || expression.NodeType == ExpressionType.LessThan
                       || expression.NodeType == ExpressionType.LessThanOrEqual
                       || expression.NodeType == ExpressionType.GreaterThan
                       || expression.NodeType == ExpressionType.GreaterThanOrEqual
                       || expression.NodeType == ExpressionType.Not);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsEntityQueryable([NotNull] this ConstantExpression constantExpression)
            => constantExpression.Type.GetTypeInfo().IsGenericType
            && constantExpression.Type.GetGenericTypeDefinition() == typeof(EntityQueryable<>);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IQuerySource TryGetReferencedQuerySource([NotNull] this Expression expression)
            => (expression as QuerySourceReferenceExpression)?.ReferencedQuerySource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static BinaryExpression CreateAssignExpression(
            [NotNull] this Expression left,
            [NotNull] Expression right)
        {
            var leftType = left.Type;
            if (leftType != right.Type
                && right.Type.GetTypeInfo().IsAssignableFrom(leftType.GetTypeInfo()))
            {
                right = Expression.Convert(right, leftType);
            }

            return Expression.Assign(left, right);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MemberExpression MakeMemberAccess(
            [CanBeNull] this Expression expression,
            [NotNull] MemberInfo member)
        {
            var memberDeclaringClrType = member.DeclaringType;
            if (expression != null
                && memberDeclaringClrType != expression.Type
                && expression.Type.GetTypeInfo().IsAssignableFrom(memberDeclaringClrType.GetTypeInfo()))
            {
                expression = Expression.Convert(expression, memberDeclaringClrType);
            }

            return Expression.MakeMemberAccess(expression, member);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsNullPropagationCandidate(
            [NotNull] this ConditionalExpression conditionalExpression,
            out Expression testExpression,
            out Expression resultExpression)
        {
            Check.NotNull(conditionalExpression, nameof(conditionalExpression));

            testExpression = null;
            resultExpression = null;

            if (!(conditionalExpression.Test is BinaryExpression binaryTest)
                || !(binaryTest.NodeType == ExpressionType.Equal
                     || binaryTest.NodeType == ExpressionType.NotEqual))
            {
                return false;
            }

            var isLeftNullConstant = binaryTest.Left.IsNullConstantExpression();
            var isRightNullConstant = binaryTest.Right.IsNullConstantExpression();

            if (isLeftNullConstant == isRightNullConstant)
            {
                return false;
            }

            if (binaryTest.NodeType == ExpressionType.Equal)
            {
                if (!conditionalExpression.IfTrue.IsNullConstantExpression())
                {
                    return false;
                }
            }
            else
            {
                if (!conditionalExpression.IfFalse.IsNullConstantExpression())
                {
                    return false;
                }
            }

            testExpression = isLeftNullConstant ? binaryTest.Right : binaryTest.Left;
            resultExpression = binaryTest.NodeType == ExpressionType.Equal ? conditionalExpression.IfFalse : conditionalExpression.IfTrue;

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Expression CreateKeyAccessExpression(
            [NotNull] this Expression target,
            [NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotNull(target, nameof(target));
            Check.NotNull(properties, nameof(properties));

            return properties.Count == 1
                ? target.CreateEFPropertyExpression(properties[0])
                : Expression.New(
                    AnonymousObject.AnonymousObjectCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        properties
                            .Select(p =>
                                Expression.Convert(
                                    target.CreateEFPropertyExpression(p),
                                    typeof(object)))
                            .Cast<Expression>()
                            .ToArray()));
        }
    }
}
