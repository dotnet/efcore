// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Supports specifying the properties and values to set on the target row in the <c>WhenMatched</c> clause of ExecuteMerge.
///     The value selector receives both the existing target row and the incoming source row.
/// </summary>
/// <typeparam name="TTarget">The type of the target entity.</typeparam>
/// <typeparam name="TSource">The type of the source rows.</typeparam>
public sealed class MergeUpdateSettersBuilder<TTarget, TSource> : MergeSettersBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public MergeUpdateSettersBuilder()
    {
    }

    /// <summary>
    ///     Specifies a target property and the value it should be updated to when a source row matches an existing target row.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="propertyExpression">A target property access expression.</param>
    /// <param name="valueExpression">A value expression referencing the existing target row and the incoming source row.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public MergeUpdateSettersBuilder<TTarget, TSource> SetProperty<TProperty>(
        Expression<Func<TTarget, TProperty>> propertyExpression,
        Expression<Func<TTarget, TSource, TProperty>> valueExpression)
    {
        AddSetter(propertyExpression, valueExpression);
        return this;
    }

    /// <summary>
    ///     Specifies a target property and a constant value it should be updated to when a source row matches an existing target row.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="propertyExpression">A target property access expression.</param>
    /// <param name="valueExpression">The value to set.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public MergeUpdateSettersBuilder<TTarget, TSource> SetProperty<TProperty>(
        Expression<Func<TTarget, TProperty>> propertyExpression,
        TProperty valueExpression)
    {
        AddSetter(propertyExpression, Expression.Constant(valueExpression, typeof(TProperty)));
        return this;
    }
}
