// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         Extension methods for <see cref="Expression" /> types.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public static class ExpressionExtensions
{
    /// <summary>
    ///     Creates a printable string representation of the given expression.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="characterLimit">An optional limit to the number of characters included. Additional output will be truncated.</param>
    /// <returns>The printable representation.</returns>
    public static string Print(this Expression expression, int? characterLimit = null)
        => new ExpressionPrinter().PrintExpression(expression, characterLimit);

    /// <summary>
    ///     Creates a <see cref="MemberExpression"></see> that represents accessing either a field or a property.
    /// </summary>
    /// <param name="expression">An <see cref="Expression"></see> that represents the object that the member belongs to.</param>
    /// <param name="member">The <see cref="MemberInfo"></see> that describes the field or property to be accessed.</param>
    /// <returns>The <see cref="MemberExpression"></see> that results from calling the appropriate factory method.</returns>
    public static MemberExpression MakeMemberAccess(
        this Expression? expression,
        MemberInfo member)
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
    /// <param name="memberExpression">The member to which assignment will be made.</param>
    /// <param name="valueExpression">The value that will be assigned.</param>
    /// <returns>The <see cref="BinaryExpression" /> representing the assignment binding.</returns>
    [UnconditionalSuppressMessage(
        "ReflectionAnalysis", "IL2077",
        Justification = "AssignBinaryExpression is preserved via DynamicDependency below")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "System.Linq.Expressions.AssignBinaryExpression", "System.Linq.Expressions")]
    public static Expression Assign(
        this MemberExpression memberExpression,
        Expression valueExpression)
    {
        if (memberExpression.Member is FieldInfo { IsInitOnly: true })
        {
            return (BinaryExpression)Activator.CreateInstance(
                GetAssignBinaryExpressionType(),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                [memberExpression, valueExpression],
                null)!;
        }

        return Expression.Assign(memberExpression, valueExpression);

        [UnconditionalSuppressMessage(
            "ReflectionAnalysis", "IL2026",
            Justification = "DynamicDependency ensures AssignBinaryExpression isn't trimmed")]
        static Type GetAssignBinaryExpressionType()
            => typeof(Expression).Assembly.GetType("System.Linq.Expressions.AssignBinaryExpression", throwOnError: true)!;
    }

    /// <summary>
    ///     If the given a method-call expression represents a call to <see cref="EF.Property{TProperty}" />, then this
    ///     method extracts the entity expression and property name.
    /// </summary>
    /// <param name="methodCallExpression">The method-call expression for <see cref="EF.Property{TProperty}" /></param>
    /// <param name="entityExpression">The extracted entity access expression.</param>
    /// <param name="propertyName">The accessed property name.</param>
    /// <returns><see langword="true" /> if the method-call was for <see cref="EF.Property{TProperty}" />; <see langword="false" /> otherwise.</returns>
    public static bool TryGetEFPropertyArguments(
        this MethodCallExpression methodCallExpression,
        [NotNullWhen(true)] out Expression? entityExpression,
        [NotNullWhen(true)] out string? propertyName)
    {
        if (methodCallExpression.Method.IsEFPropertyMethod()
            && methodCallExpression.Arguments[1] is ConstantExpression propertyNameExpression)
        {
            entityExpression = methodCallExpression.Arguments[0];
            propertyName = (string)propertyNameExpression.Value!;
            return true;
        }

        (entityExpression, propertyName) = (null, null);
        return false;
    }

    /// <summary>
    ///     If the given a method-call expression represents a call to indexer on the entity, then this
    ///     method extracts the entity expression and property name.
    /// </summary>
    /// <param name="methodCallExpression">The method-call expression for indexer.</param>
    /// <param name="model">The model to use.</param>
    /// <param name="entityExpression">The extracted entity access expression.</param>
    /// <param name="propertyName">The accessed property name.</param>
    /// <returns><see langword="true" /> if the method-call was for indexer; <see langword="false" /> otherwise.</returns>
    public static bool TryGetIndexerArguments(
        this MethodCallExpression methodCallExpression,
        IModel model,
        [NotNullWhen(true)] out Expression? entityExpression,
        [NotNullWhen(true)] out string? propertyName)
    {
        if (model.IsIndexerMethod(methodCallExpression.Method)
            && methodCallExpression.Arguments[0] is ConstantExpression propertyNameExpression)
        {
            entityExpression = methodCallExpression.Object!;
            propertyName = (string)propertyNameExpression.Value!;

            return true;
        }

        (entityExpression, propertyName) = (null, null);
        return false;
    }

    /// <summary>
    ///     Gets the <see cref="PropertyInfo" /> represented by a simple property-access expression.
    /// </summary>
    /// <remarks>
    ///     This method is typically used to parse property access lambdas from fluent APIs.
    /// </remarks>
    /// <param name="propertyAccessExpression">The expression.</param>
    /// <returns>The <see cref="PropertyInfo" />.</returns>
    public static PropertyInfo GetPropertyAccess(this LambdaExpression propertyAccessExpression)
        => GetInternalMemberAccess<PropertyInfo>(propertyAccessExpression);

    /// <summary>
    ///     Gets the <see cref="MemberInfo" /> represented by a simple member-access expression.
    /// </summary>
    /// <remarks>
    ///     This method is typically used to parse member access lambdas from fluent APIs.
    /// </remarks>
    /// <param name="memberAccessExpression">The expression.</param>
    /// <returns>The <see cref="MemberInfo" />.</returns>
    public static MemberInfo GetMemberAccess(this LambdaExpression memberAccessExpression)
        => GetInternalMemberAccess<MemberInfo>(memberAccessExpression);

    private static TMemberInfo GetInternalMemberAccess<TMemberInfo>(this LambdaExpression memberAccessExpression)
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
            var index = Array.FindIndex(interfaceMapping.InterfaceMethods, p => p.Equals(propertyGetter));
            var targetMethod = interfaceMapping.TargetMethods[index];
            foreach (var runtimeProperty in parameterType.GetRuntimeProperties())
            {
                if (targetMethod.Equals(runtimeProperty.GetMethod))
                {
                    return (TMemberInfo)(object)runtimeProperty;
                }
            }
        }

        return memberInfo;
    }

    /// <summary>
    ///     Returns a list of <see cref="PropertyInfo" /> extracted from the given simple
    ///     <see cref="LambdaExpression" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Only simple expressions are supported, such as those used to reference a property.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </remarks>
    /// <param name="propertyAccessExpression">The expression.</param>
    /// <returns>The list of referenced properties.</returns>
    public static IReadOnlyList<PropertyInfo> GetPropertyAccessList(this LambdaExpression propertyAccessExpression)
    {
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
    ///     Returns a list of <see cref="MemberInfo" /> extracted from the given simple
    ///     <see cref="LambdaExpression" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Only simple expressions are supported, such as those used to reference a member.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </remarks>
    /// <param name="memberAccessExpression">The expression.</param>
    /// <returns>The list of referenced members.</returns>
    public static IReadOnlyList<MemberInfo> GetMemberAccessList(this LambdaExpression memberAccessExpression)
    {
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
    /// <param name="valueBuffer">The expression that exposes the <see cref="ValueBuffer" />.</param>
    /// <param name="type">The type to read.</param>
    /// <param name="index">The index in the buffer to read from.</param>
    /// <param name="property">The IPropertyBase being read if any.</param>
    /// <returns>An expression to read the value.</returns>
    public static Expression CreateValueBufferReadValueExpression(
        this Expression valueBuffer,
        Type type,
        int index,
        IPropertyBase? property)
        => property is INavigationBase
            ? Expression.Constant(null, typeof(object))
            : Expression.Call(
                MakeValueBufferTryReadValueMethod(type),
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
        = typeof(ExpressionExtensions).GetTypeInfo().GetDeclaredMethod(nameof(ValueBufferTryReadValue))!;

    [UnconditionalSuppressMessage(
        "ReflectionAnalysis", "IL2060",
        Justification = "ValueBufferTryReadValueMethod has no DynamicallyAccessedMembers annotations and is safe to construct.")]
    private static MethodInfo MakeValueBufferTryReadValueMethod(Type type)
        => ValueBufferTryReadValueMethod.MakeGenericMethod(type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TValue ValueBufferTryReadValue<TValue>(
#pragma warning disable IDE0060 // Remove unused parameter
        in ValueBuffer valueBuffer,
        int index,
        IPropertyBase property)
#pragma warning restore IDE0060 // Remove unused parameter
        => (TValue)valueBuffer[index]!;

    /// <summary>
    ///     <para>
    ///         Creates an <see cref="Expression" /> tree representing reading of a key values on given expression.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="target">The expression that will be root for generated read operation.</param>
    /// <param name="properties">The list of properties to use to generate key values.</param>
    /// <param name="makeNullable">A value indicating if the key values should be read nullable.</param>
    /// <returns>An expression to read the key values.</returns>
    public static Expression CreateKeyValuesExpression(
        this Expression target,
        IReadOnlyList<IProperty> properties,
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
    /// <param name="target">The expression that will be root for generated read operation.</param>
    /// <param name="property">The property to access.</param>
    /// <param name="makeNullable">A value indicating if the value can be nullable.</param>
    /// <returns>An expression to access EF property on given expression.</returns>
    public static Expression CreateEFPropertyExpression(
        this Expression target,
        IPropertyBase property,
        bool makeNullable = true) // No shadow entities in runtime
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

        // EF.Property expects an object as its first argument. If the target is a struct (complex type), we need an explicit up-cast to
        // object.
        if (target.Type.IsValueType)
        {
            target = Expression.Convert(target, typeof(object));
        }

        return Expression.Call(
            EF.MakePropertyMethod(propertyType),
            target,
            Expression.Constant(propertyName));
    }

    private static readonly MethodInfo ObjectEqualsMethodInfo
        = typeof(object).GetRuntimeMethod(nameof(object.Equals), [typeof(object), typeof(object)])!;

    /// <summary>
    ///     <para>
    ///         Creates an <see cref="Expression" /> tree representing equality comparison between 2 expressions using
    ///         <see cref="object.Equals(object?, object?)" /> method.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="left">The left expression in equality comparison.</param>
    /// <param name="right">The right expression in equality comparison.</param>
    /// <param name="negated">If the comparison is non-equality.</param>
    /// <returns>An expression to compare left and right expressions.</returns>
    public static Expression CreateEqualsExpression(
        Expression left,
        Expression right,
        bool negated = false)
    {
        var result = Expression.Call(ObjectEqualsMethodInfo, AddConvertToObject(left), AddConvertToObject(right));

        return negated
            ? Expression.Not(result)
            : result;

        static Expression AddConvertToObject(Expression expression)
            => expression.Type.IsValueType
                ? Expression.Convert(expression, typeof(object))
                : expression;
    }
}
