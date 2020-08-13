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
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Extension methods for <see cref="Expression" /> types.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        ///     Creates a printable string representation of the given expression.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <param name="characterLimit"> An optional limit to the number of characters included. </param>
        /// <returns> The printable representation. </returns>
        public static string Print([NotNull] this Expression expression, int? characterLimit = null)
            => new ExpressionPrinter().Print(Check.NotNull(expression, nameof(expression)), characterLimit);

        /// <summary>
        ///     Creates a <see cref="MemberExpression"></see> that represents accessing either a field or a property.
        /// </summary>
        /// <param name="expression"> An <see cref="Expression"></see> that represents the object that the member belongs to. </param>
        /// <param name="member"> The <see cref="MemberInfo"></see> that describes the field or property to be accessed. </param>
        /// <returns> The <see cref="MemberExpression"></see> that results from calling the appropriate factory method. </returns>
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
        ///     Creates a <see cref="BinaryExpression"></see> that represents an assignment operation.
        /// </summary>
        /// <param name="memberExpression"> The member to which assignment will be made. </param>
        /// <param name="valueExpression"> The value that will be assigned. </param>
        /// <returns> The <see cref="BinaryExpression" /> representing the assignment binding. </returns>
        public static Expression Assign(
            [NotNull] this MemberExpression memberExpression,
            [NotNull] Expression valueExpression)
        {
            if (memberExpression.Member is FieldInfo fieldInfo
                && fieldInfo.IsInitOnly)
            {
                if (new FrameworkName(AppContext.TargetFrameworkName).Identifier == ".NETFramework")
                {
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

                return (BinaryExpression)Activator.CreateInstance(
                    _assignBinaryExpressionType,
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new object[] { memberExpression, valueExpression },
                    null);
            }

            return Expression.Assign(memberExpression, valueExpression);
        }

        private static readonly Type _assignBinaryExpressionType
            = typeof(Expression).Assembly.GetType("System.Linq.Expressions.AssignBinaryExpression");

        private static readonly MethodInfo _fieldInfoSetValueMethod
            = typeof(FieldInfo)
                .GetTypeInfo()
                .GetDeclaredMethods(nameof(FieldInfo.SetValue))
                .Single(m => m.GetParameters().Length == 2);

        /// <summary>
        ///     If the given a method-call expression represents a call to <see cref="EF.Property{TProperty}" />, then this
        ///     method extracts the entity expression and property name.
        /// </summary>
        /// <param name="methodCallExpression"> The method-call expression for <see cref="EF.Property{TProperty}" /> </param>
        /// <param name="entityExpression"> The extracted entity access expression. </param>
        /// <param name="propertyName"> The accessed property name. </param>
        /// <returns> True if the method-call was for <see cref="EF.Property{TProperty}" />; false otherwise. </returns>
        public static bool TryGetEFPropertyArguments(
            [NotNull] this MethodCallExpression methodCallExpression,
            out Expression entityExpression,
            out string propertyName)
        {
            if (methodCallExpression.Method.IsEFPropertyMethod()
                && methodCallExpression.Arguments[1] is ConstantExpression propertyNameExpression)
            {
                entityExpression = methodCallExpression.Arguments[0];
                propertyName = (string)propertyNameExpression.Value;
                return true;
            }

            (entityExpression, propertyName) = (null, null);
            return false;
        }

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyInfo" /> represented by a simple property-access expression.
        ///     </para>
        ///     <para>
        ///         This method is typically used to parse property access lambdas from fluent APIs.
        ///     </para>
        /// </summary>
        /// <param name="propertyAccessExpression"> The expression. </param>
        /// <returns> The <see cref="PropertyInfo" />. </returns>
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
        ///         Returns a list of <see cref="PropertyInfo" /> extracted from the given simple
        ///         <see cref="LambdaExpression" />.
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
        ///         Returns a new expression with any see <see cref="ExpressionType.Convert" /> or
        ///         <see cref="ExpressionType.ConvertChecked" /> nodes removed from the head of the
        ///         given expression tree/
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <returns> A new expression with converts at the head removed. </returns>
        [Obsolete("Unwrap each convert manually by evaluating how they are used.")]
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
