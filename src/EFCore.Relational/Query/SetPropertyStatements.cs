// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
/// <typeparam name="TSource">The type of source element on which ExecuteUpdate operation is being applied.</typeparam>
public sealed class SetPropertyStatements<TSource>
{
    /// <summary>
    ///     Specifies a property and corresponding value it should be updated to in ExecuteUpdate method.
    /// </summary>
    /// <typeparam name="TProperty">The type of property.</typeparam>
    /// <param name="propertyExpression">A property access expression.</param>
    /// <param name="valueExpression">A value expression.</param>
    /// <returns>The same instance so that multiple calls to <see cref="SetProperty{TProperty}(Expression{Func{TSource, TProperty}}, Expression{Func{TSource, TProperty}})"/> can be chained.</returns>
    public SetPropertyStatements<TSource> SetProperty<TProperty>(
            Expression<Func<TSource, TProperty>> propertyExpression,
            Expression<Func<TSource, TProperty>> valueExpression)
    {
        throw new InvalidOperationException(RelationalStrings.SetPropertyMethodInvoked);
    }
}
