// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

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
public sealed class SetPropertyCalls<TSource>
{
    private SetPropertyCalls()
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
    ///     <see cref="SetPropertyCalls{TSource}.SetProperty{TProperty}(Func{TSource, TProperty}, Func{TSource, TProperty})" />
    ///     can be chained.
    /// </returns>
    public SetPropertyCalls<TSource> SetProperty<TProperty>(
        Func<TSource, TProperty> propertyExpression,
        Func<TSource, TProperty> valueExpression)
        => throw new InvalidOperationException(RelationalStrings.SetPropertyMethodInvoked);

    /// <summary>
    ///     Specifies a property and corresponding value it should be updated to in ExecuteUpdate method.
    /// </summary>
    /// <typeparam name="TProperty">The type of property.</typeparam>
    /// <param name="propertyExpression">A property access expression.</param>
    /// <param name="valueExpression">A value expression.</param>
    /// <returns>
    ///     The same instance so that multiple calls to
    ///     <see cref="SetPropertyCalls{TSource}.SetProperty{TProperty}(Func{TSource, TProperty}, TProperty)" /> can be chained.
    /// </returns>
    public SetPropertyCalls<TSource> SetProperty<TProperty>(
        Func<TSource, TProperty> propertyExpression,
        TProperty valueExpression)
        => throw new InvalidOperationException(RelationalStrings.SetPropertyMethodInvoked);

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
