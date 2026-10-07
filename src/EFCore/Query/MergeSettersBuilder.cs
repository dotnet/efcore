// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         Base type for the setter builders used by the <c>WhenMatched</c> and <c>WhenNotMatched</c> clauses of ExecuteMerge.
///         Captures the property/value selector pairs as an expression tree.
///     </para>
///     <para>
///         This type does not have any behavior beyond expression-tree capture; it is used inside the LINQ query solely for the purpose
///         of building the expression tree that the query pipeline translates.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public abstract class MergeSettersBuilder
{
    private readonly List<NewExpression> _setters = [];

    private static ConstructorInfo? _setterTupleConstructor;

    private static ConstructorInfo SetterTupleConstructor
        => _setterTupleConstructor ??= typeof(Tuple<Delegate, object>).GetConstructor([typeof(Delegate), typeof(object)])!;

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
    protected virtual void AddSetter(LambdaExpression propertyExpression, LambdaExpression valueExpression)
        => _setters.Add(Expression.New(SetterTupleConstructor, propertyExpression, valueExpression));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual void AddSetter(LambdaExpression propertyExpression, Expression valueExpression)
    {
        // Constant values are boxed to object so the tuple stays uniformly typed; the funcletizer parameterizes them later.
        if (valueExpression.Type.IsValueType)
        {
            valueExpression = Expression.Convert(valueExpression, typeof(object));
        }

        _setters.Add(Expression.New(SetterTupleConstructor, propertyExpression, valueExpression));
    }
}
