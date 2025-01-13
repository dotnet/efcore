// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         Supports specifying property and value to be set in ExecuteUpdate method with chaining multiple calls for updating
///         multiple columns.
///     </para>
///     <para>
///         This type does not have any constructor or implementation since it is used inside LINQ query solely for the purpose of
///         creating expression tree.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public class UpdateSettersBuilder
{
    private readonly List<NewExpression> _setters = new();

    private static ConstructorInfo? _setterTupleConstructor;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual NewArrayExpression BuildSettersExpression()
        => Expression.NewArrayInit(typeof(ITuple), _setters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual UpdateSettersBuilder SetProperty(LambdaExpression propertyExpression, LambdaExpression valueExpression)
    {
        _setters.Add(
            Expression.New(
                _setterTupleConstructor ??= typeof(Tuple<Delegate, object>).GetConstructor([typeof(Delegate), typeof(object)])!,
                propertyExpression,
                valueExpression));

        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual UpdateSettersBuilder SetProperty(LambdaExpression propertyExpression, Expression valueExpression)
    {
        if (valueExpression.Type.IsValueType)
        {
            valueExpression = Expression.Convert(valueExpression, typeof(object));
        }

        _setters.Add(
            Expression.New(
                _setterTupleConstructor ??= typeof(Tuple<Delegate, object>).GetConstructor([typeof(Delegate), typeof(object)])!,
                propertyExpression,
                valueExpression));

        return this;
    }
}
