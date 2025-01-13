// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public sealed class UpdateSettersBuilder<TSource> : UpdateSettersBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public UpdateSettersBuilder()
    {
    }

    /// <summary>
    ///     Specifies a property and corresponding value it should be updated to in ExecuteUpdate method.
    /// </summary>
    /// <typeparam name="TProperty">The type of property.</typeparam>
    /// <param name="propertyExpression">A property access expression.</param>
    /// <param name="valueExpression">A value expression.</param>
    /// <returns>
    ///     The same instance so that multiple calls to
    ///     <see cref="UpdateSettersBuilder{TSource}.SetProperty{TProperty}(Expression{Func{TSource, TProperty}}, Expression{Func{TSource, TProperty}})" />
    ///     can be chained.
    /// </returns>
    public UpdateSettersBuilder<TSource> SetProperty<TProperty>(
        Expression<Func<TSource, TProperty>> propertyExpression,
        Expression<Func<TSource, TProperty>> valueExpression)
    {
        SetProperty((LambdaExpression)propertyExpression, valueExpression);
        return this;
    }

    /// <summary>
    ///     Specifies a property and corresponding value it should be updated to in ExecuteUpdate method.
    /// </summary>
    /// <typeparam name="TProperty">The type of property.</typeparam>
    /// <param name="propertyExpression">A property access expression.</param>
    /// <param name="valueExpression">A value expression.</param>
    /// <returns>
    ///     The same instance so that multiple calls to
    ///     <see cref="UpdateSettersBuilder{TSource}.SetProperty{TProperty}(Expression{Func{TSource, TProperty}}, TProperty)" /> can be chained.
    /// </returns>
    public UpdateSettersBuilder<TSource> SetProperty<TProperty>(
        Expression<Func<TSource, TProperty>> propertyExpression,
        TProperty valueExpression)
    {
        // We pass in the value as a ConstantExpression; but it will get parameterized by the funcletizer as any method argument that isn't
        // inside a lambda (just like parameters to Skip/Take).
        SetProperty(propertyExpression, Expression.Constant(valueExpression, typeof(TProperty)));
        return this;
    }
}
