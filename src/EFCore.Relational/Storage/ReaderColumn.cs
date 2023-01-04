// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         An expected column in the relational data reader.
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
public abstract class ReaderColumn
{
    private static readonly ConcurrentDictionary<Type, ConstructorInfo> Constructors = new();

    /// <summary>
    ///     Creates a new instance of the <see cref="ReaderColumn" /> class.
    /// </summary>
    /// <param name="type">The CLR type of the column.</param>
    /// <param name="nullable">A value indicating if the column is nullable.</param>
    /// <param name="name">The name of the column.</param>
    /// <param name="property">The property being read if any, null otherwise.</param>
    /// <param name="getFieldValueExpression">A lambda expression to get field value for the column from the reader.</param>
    protected ReaderColumn(Type type, bool nullable, string? name, IPropertyBase? property, LambdaExpression getFieldValueExpression)
    {
        Type = type;
        IsNullable = nullable;
        Name = name;
        Property = property;
        GetFieldValueExpression = getFieldValueExpression;
    }

    /// <summary>
    ///     The CLR type of the column.
    /// </summary>
    public virtual Type Type { get; }

    /// <summary>
    ///     A value indicating if the column is nullable.
    /// </summary>
    public virtual bool IsNullable { get; }

    /// <summary>
    ///     The name of the column.
    /// </summary>
    public virtual string? Name { get; }

    /// <summary>
    ///     The property being read if any, null otherwise.
    /// </summary>
    public virtual IPropertyBase? Property { get; }

    /// <summary>
    ///     A lambda expression to get field value for the column from the reader.
    /// </summary>
    public virtual LambdaExpression GetFieldValueExpression { get; }

    /// <summary>
    ///     Creates an instance of <see cref="ReaderColumn{T}" />.
    /// </summary>
    /// <param name="type">The type of the column.</param>
    /// <param name="nullable">Whether the column can contain <see langword="null" /> values.</param>
    /// <param name="columnName">The column name if it is used to access the column values, <see langword="null" /> otherwise.</param>
    /// <param name="property">The property being read if any, null otherwise.</param>
    /// <param name="readFunc">
    ///     A <see cref="T:System.Func{DbDataReader, Int32[], T}" /> used to get the field value for this column.
    /// </param>
    /// <returns>An instance of <see cref="ReaderColumn{T}" />.</returns>
    public static ReaderColumn Create(
        Type type,
        bool nullable,
        string? columnName,
        IPropertyBase? property,
        LambdaExpression readFunc)
        => (ReaderColumn)GetConstructor(type).Invoke(new object?[] { nullable, columnName, property, readFunc });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static ConstructorInfo GetConstructor(Type type)
        => Constructors.GetOrAdd(
            type, t => typeof(ReaderColumn<>).MakeGenericType(t).GetConstructors().First(ci => ci.GetParameters().Length == 4));
}
