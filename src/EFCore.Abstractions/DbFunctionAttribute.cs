// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Maps a static CLR method to a database function so that the CLR method may be used in LINQ queries.
///     By convention uses the .NET method name as name of the database function and the default schema.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
#pragma warning disable CA1813 // Avoid unsealed attributes
// Already shipped unsealed
public class DbFunctionAttribute : Attribute
#pragma warning restore CA1813 // Avoid unsealed attributes
{
    private string? _name;
    private string? _schema;
    private bool _builtIn;
    private bool? _nullable;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbFunctionAttribute" /> class.
    /// </summary>
    public DbFunctionAttribute()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbFunctionAttribute" /> class.
    /// </summary>
    /// <param name="name">The name of the function in the database.</param>
    /// <param name="schema">The schema of the function in the database.</param>
    public DbFunctionAttribute(string name, string? schema = null)
    {
        Check.NotEmpty(name, nameof(name));

        _name = name;
        _schema = schema;
    }

    /// <summary>
    ///     The name of the function in the database.
    /// </summary>
    [DisallowNull]
    public virtual string? Name
    {
        get => _name;
        set
        {
            Check.NotEmpty(value, nameof(value));

            _name = value;
        }
    }

    /// <summary>
    ///     The schema of the function in the database.
    /// </summary>
    public virtual string? Schema
    {
        get => _schema;
        set => _schema = value;
    }

    /// <summary>
    ///     The value indicating whether the database function is built-in or not.
    /// </summary>
    public virtual bool IsBuiltIn
    {
        get => _builtIn;
        set => _builtIn = value;
    }

    /// <summary>
    ///     The value indicating whether the database function can return null result or not.
    /// </summary>
    public virtual bool IsNullable
    {
        get => _nullable ?? true;
        set => _nullable = value;
    }

    /// <summary>
    ///     Checks whether <see cref="IsNullable" /> has been explicitly set to a value.
    /// </summary>
    public bool IsNullableHasValue
        => _nullable.HasValue;
}
