// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
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
        /// <param name="characterLimit"> An optional limit to the number of characters included. Additional output will be truncated. </param>
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
                && expression.Type.IsAssignableFrom(memberDeclaringClrType))
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

        /// <summary>
        ///     If the given a method-call expression represents a call to <see cref="EF.Property{TProperty}" />, then this
        ///     method extracts the entity expression and property name.
        /// </summary>
        /// <param name="methodCallExpression"> The method-call expression for <see cref="EF.Property{TProperty}" /> </param>
        /// <param name="entityExpression"> The extracted entity access expression. </param>
        /// <param name="propertyName"> The accessed property name. </param>
        /// <returns> <see langword="true" /> if the method-call was for <see cref="EF.Property{TProperty}" />; <see langword="false" /> otherwise. </returns>
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
        ///     If the given a method-call expression represents a call to indexer on the entity, then this
        ///     method extracts the entity expression and property name.
        /// </summary>
        /// <param name="methodCallExpression"> The method-call expression for indexer. </param>
        /// <param name="model"> The model to use. </param>
        /// <param name="entityExpression"> The extracted entity access expression. </param>
        /// <param name="propertyName"> The accessed property name. </param>
        /// <returns> <see langword="true" /> if the method-call was for indexer; <see langword="false" /> otherwise. </returns>
        public static bool TryGetIndexerArguments(
            [NotNull] this MethodCallExpression methodCallExpression,
            [NotNull] IModel model,
            out Expression entityExpression,
            out string propertyName)
        {
            if (model.IsIndexerMethod(methodCallExpression.Method)
                && methodCallExpression.Arguments[0] is ConstantExpression propertyNameExpression)
            {
                entityExpression = methodCallExpression.Object;
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
            => GetInternalMemberAccess<PropertyInfo>(propertyAccessExpression);

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="MemberInfo" /> represented by a simple member-access expression.
        ///     </para>
        ///     <para>
        ///         This method is typically used to parse member access lambdas from fluent APIs.
        ///     </para>
        /// </summary>
        /// <param name="memberAccessExpression"> The expression. </param>
        /// <returns> The <see cref="MemberInfo" />. </returns>
        public static MemberInfo GetMemberAccess([NotNull] this LambdaExpression memberAccessExpression)
            => GetInternalMemberAccess<MemberInfo>(memberAccessExpression);

        private static TMemberInfo GetInternalMemberAccess<TMemberInfo>([NotNull] this LambdaExpression memberAccessExpression)
            where TMemberInfo : MemberInfo
        {
            Check.DebugAssert(
                memberAccessExpression.Parameters.Count == 1,
                $"Parameters.Count is {memberAccessExpression.Parameters.Count}");

            var parameterExpression = memberAccessExpression.Parameters[0];
            var memberInfo = parameterExpression.MatchSimpleMemberAccess<TMemberInfo>(memberAccessExpression.Body);

            if (memberInfo == null)
            {
                throw new ArgumentException(
                    CoreStrings.InvalidMemberExpression(memberAccessExpression),
                    nameof(memberAccessExpression));
            }

            var declaringType = memberInfo.DeclaringType;
            var parameterType = parameterExpression.Type;

            if (declaringType != null
                && declaringType != parameterType
                && declaringType.IsInterface
                && declaringType.IsAssignableFrom(parameterType)
                && memberInfo is PropertyInfo propertyInfo)
            {
                var propertyGetter = propertyInfo.GetMethod;
                var interfaceMapping = parameterType.GetTypeInfo().GetRuntimeInterfaceMap(declaringType);
                var index = Array.FindIndex(interfaceMapping.InterfaceMethods, p => propertyGetter.Equals(p));
                var targetMethod = interfaceMapping.TargetMethods[index];
                foreach (var runtimeProperty in parameterType.GetRuntimeProperties())
                {
                    if (targetMethod.Equals(runtimeProperty.GetMethod))
                    {
                        return runtimeProperty as TMemberInfo;
                    }
                }
            }

            return memberInfo;
        }

        /// <summary>
        ///     <para>
        ///         Returns a list of <see cref="PropertyInfo" /> extracted from the given simple
        ///         <see cref="LambdaExpression" />.
        ///     </para>
        ///     <para>
        ///         Only simple expressions are supported, such as those used to reference a property.
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
                    CoreStrings.InvalidMembersExpression(propertyAccessExpression),
                    nameof(propertyAccessExpression));
            }

            var propertyPaths = propertyAccessExpression
                .MatchMemberAccessList((p, e) => e.MatchSimpleMemberAccess<PropertyInfo>(p));

            if (propertyPaths == null)
            {
                throw new ArgumentException(
                    CoreStrings.InvalidMembersExpression(propertyAccessExpression),
                    nameof(propertyAccessExpression));
            }

            return propertyPaths;
        }

        /// <summary>
        ///     <para>
        ///         Returns a list of <see cref="MemberInfo" /> extracted from the given simple
        ///         <see cref="LambdaExpression" />.
        ///     </para>
        ///     <para>
        ///         Only simple expressions are supported, such as those used to reference a member.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="memberAccessExpression"> The expression. </param>
        /// <returns> The list of referenced members. </returns>
        public static IReadOnlyList<MemberInfo> GetMemberAccessList([NotNull] this LambdaExpression memberAccessExpression)
        {
            Check.NotNull(memberAccessExpression, nameof(memberAccessExpression));

            var memberPaths = memberAccessExpression
                .MatchMemberAccessList((p, e) => e.MatchSimpleMemberAccess<MemberInfo>(p));

            if (memberPaths == null)
            {
                throw new ArgumentException(
                    CoreStrings.InvalidMembersExpression(memberAccessExpression),
                    nameof(memberAccessExpression));
            }

            return memberPaths;
        }

        /// <summary>
        ///     <para>
        ///         Creates an <see cref="Expression" /> tree representing reading a value from a <see cref="ValueBuffer" />
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="valueBuffer"> The expression that exposes the <see cref="ValueBuffer" />. </param>
        /// <param name="type"> The type to read. </param>
        /// <param name="index"> The index in the buffer to read from. </param>
        /// <param name="property"> The IPropertyBase being read if any. </param>
        /// <returns> An expression to read the value. </returns>
        public static Expression CreateValueBufferReadValueExpression(
            [NotNull] this Expression valueBuffer,
            [NotNull] Type type,
            int index,
            [CanBeNull] IPropertyBase property)
            => Expression.Call(
                ValueBufferTryReadValueMethod.MakeGenericMethod(type),
                valueBuffer,
                Expression.Constant(index),
                Expression.Constant(property, typeof(IPropertyBase)));

        /// <summary>
        ///     <para>
        ///         MethodInfo which is used to generate an <see cref="Expression" /> tree representing reading a value from a
        ///         <see cref="ValueBuffer" />
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        public static readonly MethodInfo ValueBufferTryReadValueMethod
            = typeof(ExpressionExtensions).GetTypeInfo()
                .GetDeclaredMethod(nameof(ValueBufferTryReadValue));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TValue ValueBufferTryReadValue<TValue>(
#pragma warning disable IDE0060 // Remove unused parameter
            in ValueBuffer valueBuffer,
            int index,
            IPropertyBase property)
#pragma warning restore IDE0060 // Remove unused parameter
            => (TValue)valueBuffer[index];

        /// <summary>
        ///     <para>
        ///         Creates an <see cref="Expression" /> tree representing reading of a key values on given expression.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="target"> The expression that will be root for generated read operation. </param>
        /// <param name="properties"> The list of properties to use to generate key values. </param>
        /// <param name="makeNullable"> A value indicating if the key values should be read nullable. </param>
        /// <returns> An expression to read the key values. </returns>
        public static Expression CreateKeyValuesExpression(
            [NotNull] this Expression target,
            [NotNull] IReadOnlyList<IProperty> properties,
            bool makeNullable = false)
            => properties.Count == 1
                ? target.CreateEFPropertyExpression(properties[0], makeNullable)
                : Expression.NewArrayInit(
                    typeof(object),
                    properties
                        .Select(p => Expression.Convert(target.CreateEFPropertyExpression(p, makeNullable), typeof(object))));

        /// <summary>
        ///     <para>
        ///         Creates an <see cref="Expression" /> tree representing EF property access on given expression.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="target"> The expression that will be root for generated read operation. </param>
        /// <param name="property"> The property to access. </param>
        /// <param name="makeNullable"> A value indicating if the value can be nullable. </param>
        /// <returns> An expression to access EF property on given expression. </returns>
        public static Expression CreateEFPropertyExpression(
            [NotNull] this Expression target,
            [NotNull] IPropertyBase property,
            bool makeNullable = true)
            => CreateEFPropertyExpression(target, property.DeclaringType.ClrType, property.ClrType, property.Name, makeNullable);

        private static Expression CreateEFPropertyExpression(
            Expression target,
            Type propertyDeclaringType,
            Type propertyType,
            string propertyName,
            bool makeNullable)
        {
            if (propertyDeclaringType != target.Type
                && target.Type.IsAssignableFrom(propertyDeclaringType))
            {
                target = Expression.Convert(target, propertyDeclaringType);
            }

            if (makeNullable)
            {
                propertyType = propertyType.MakeNullable();
            }

            return Expression.Call(
                EF.PropertyMethod.MakeGenericMethod(propertyType),
                target,
                Expression.Constant(propertyName));
        }
    }
}
