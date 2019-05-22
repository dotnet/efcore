// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public static class ExpressionExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsNullConstantExpression([NotNull] this Expression expression)
            => expression.RemoveConvert() is ConstantExpression constantExpression
               && constantExpression.Value == null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyList<PropertyInfo> MatchPropertyAccessList(
            [NotNull] this LambdaExpression lambdaExpression, [NotNull] Func<Expression, Expression, PropertyInfo> propertyMatcher)
        {
            Debug.Assert(lambdaExpression.Body != null);

            var parameterExpression
                = lambdaExpression.Parameters.Single();

            if (lambdaExpression.Body.RemoveConvert() is NewExpression newExpression)
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static PropertyInfo MatchSimplePropertyAccess(
            [NotNull] this Expression parameterExpression, [NotNull] Expression propertyAccessExpression)
        {
            var propertyInfos = MatchPropertyAccess(parameterExpression, propertyAccessExpression);

            return propertyInfos?.Count == 1 ? propertyInfos[0] : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyList<PropertyInfo> GetComplexPropertyAccess(
            [NotNull] this LambdaExpression propertyAccessExpression,
            [NotNull] string methodName)
        {
            if (!TryGetComplexPropertyAccess(propertyAccessExpression, out var propertyPath))
            {
                throw new ArgumentException(
                    CoreStrings.InvalidIncludeLambdaExpression(methodName, propertyAccessExpression));
            }

            return propertyPath;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool TryGetComplexPropertyAccess(
            [NotNull] this LambdaExpression propertyAccessExpression,
            out IReadOnlyList<PropertyInfo> propertyPath)
        {
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            propertyPath
                = propertyAccessExpression
                    .Parameters
                    .Single()
                    .MatchPropertyAccess(propertyAccessExpression.Body);

            return propertyPath != null;
        }

        private static IReadOnlyList<PropertyInfo> MatchPropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression)
        {
            var propertyInfos = new List<PropertyInfo>();

            MemberExpression memberExpression;

            do
            {
                memberExpression = RemoveTypeAs(propertyAccessExpression.RemoveConvert()) as MemberExpression;

                if (!(memberExpression?.Member is PropertyInfo propertyInfo))
                {
                    return null;
                }

                propertyInfos.Insert(0, propertyInfo);

                propertyAccessExpression = memberExpression.Expression;
            }
            while (RemoveTypeAs(memberExpression.Expression.RemoveConvert()) != parameterExpression);

            return propertyInfos;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Expression RemoveNullConditional([CanBeNull] this Expression expression)
            => expression is NullConditionalExpression conditional ? conditional.AccessOperation : expression;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Expression RemoveTypeAs([CanBeNull] this Expression expression)
        {
            while ((expression?.NodeType == ExpressionType.TypeAs))
            {
                expression = ((UnaryExpression)expression.RemoveConvert()).Operand;
            }

            return expression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsLogicalOperation([NotNull] this Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return expression.NodeType == ExpressionType.AndAlso
                   || expression.NodeType == ExpressionType.OrElse;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsEntityQueryable([NotNull] this ConstantExpression constantExpression)
            => constantExpression.Type.GetTypeInfo().IsGenericType
               && constantExpression.Type.GetGenericTypeDefinition() == typeof(EntityQueryable<>);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IQuerySource TryGetReferencedQuerySource([NotNull] this Expression expression)
            => (expression as QuerySourceReferenceExpression)?.ReferencedQuerySource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Expression CreateAssignExpression(
            [NotNull] this MemberExpression left,
            [NotNull] Expression right)
        {
            var leftType = left.Type;
            if (leftType != right.Type
                && right.Type.GetTypeInfo().IsAssignableFrom(leftType.GetTypeInfo()))
            {
                right = Expression.Convert(right, leftType);
            }

            return left.Assign(right);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                            .Select(
                                p =>
                                    Expression.Convert(
                                        target.CreateEFPropertyExpression(p),
                                        typeof(object)))
                            .Cast<Expression>()
                            .ToArray()));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Expression Assign(
            [NotNull] this MemberExpression memberExpression,
            [NotNull] Expression valueExpression)
        {
            if (memberExpression.Member is FieldInfo fieldInfo
                && fieldInfo.IsInitOnly)
            {
                if (new FrameworkName(AppContext.TargetFrameworkName).Identifier == ".NETCoreApp")
                {
                    return (BinaryExpression)Activator.CreateInstance(
                        _assignBinaryExpressionType,
                        BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        new object[] { memberExpression, valueExpression },
                        null);
                }

                // On .NET Framework the compiler refuses to compile an expression tree with IsInitOnly access,
                // so use Reflection's SetValue instead.
                return Expression.Call(
                    Expression.Constant(fieldInfo),
                    _fieldInfoSetValueMethod,
                    memberExpression.Expression,
                    Expression.Convert(
                        valueExpression,
                        typeof(object)));
            }

            return Expression.Assign(memberExpression, valueExpression);
        }

        private static readonly Type _assignBinaryExpressionType
            = typeof(Expression).Assembly.GetType("System.Linq.Expressions.AssignBinaryExpression");

        private static readonly MethodInfo _fieldInfoSetValueMethod
            = typeof(FieldInfo).GetRuntimeMethod(nameof(FieldInfo.SetValue), new[] { typeof(object), typeof(object) });
    }
}
