// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Extension methods for <see cref="Expression"/> types.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public static class ExpressionExtensions
    {
        public static bool IsEFProperty([NotNull] this MethodCallExpression methodCallExpression)
            => methodCallExpression.Method.IsEFPropertyMethod();

        public static bool TryGetEFPropertyArguments(
            [NotNull] this MethodCallExpression methodCallExpression,
            out Expression entityExpression,
            out string propertyName)
        {
            if (IsEFProperty(methodCallExpression)
                && methodCallExpression.Arguments[1] is ConstantExpression propertyNameExpression)
            {
                entityExpression = methodCallExpression.Arguments[0];
                propertyName = (string)propertyNameExpression.Value;
                return true;
            }

            (entityExpression, propertyName) = (null, null);
            return false;
        }

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
        ///     <para>
        ///         Returns a list of <see cref="PropertyInfo"/> extracted from the given simple
        ///         <see cref="LambdaExpression"/>.
        ///     </para>
        ///     <para>
        ///         Only simple expressions are supported, such as those used to reference a property.
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="propertyAccessExpression"> The expression. </param>
        /// <returns> The list of referenced properties. </returns>
        public static IReadOnlyList<PropertyInfo> GetPropertyAccessList([NotNull] this LambdaExpression propertyAccessExpression)
        {
            Check.NotNull(propertyAccessExpression, nameof(propertyAccessExpression));

            if (propertyAccessExpression.Parameters.Count != 1)
            {
                throw new ArgumentException(
                    CoreStrings.InvalidPropertiesExpression(propertyAccessExpression),
                    nameof(propertyAccessExpression));
            }

            var propertyPaths
                = propertyAccessExpression.MatchPropertyAccessList((p, e) => e.MatchSimplePropertyAccess(p));

            if (propertyPaths == null)
            {
                throw new ArgumentException(
                    CoreStrings.InvalidPropertiesExpression(propertyAccessExpression),
                    nameof(propertyAccessExpression));
            }

            return propertyPaths;
        }

        /// <summary>
        ///     <para>
        ///         Returns a new expression with any see <see cref="ExpressionType.Convert"/> or
        ///         <see cref="ExpressionType.ConvertChecked"/> nodes removed from the head of the
        ///         given expression tree/
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <returns> A new expression with converts at the head removed. </returns>
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
    }
}
