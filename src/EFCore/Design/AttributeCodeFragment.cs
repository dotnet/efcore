// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Represents usage of an attribute.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class AttributeCodeFragment
{
    private readonly List<object?> _arguments;
    private readonly Dictionary<string, object?> _namedArguments;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AttributeCodeFragment" /> class.
    /// </summary>
    /// <param name="type">The attribute's CLR type.</param>
    /// <param name="arguments">The attribute's arguments.</param>
    public AttributeCodeFragment(Type type, params object?[] arguments)
        : this(type, arguments, new Dictionary<string, object?>(0))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AttributeCodeFragment" /> class.
    /// </summary>
    /// <param name="type">The attribute's CLR type.</param>
    /// <param name="arguments">The attribute's positional arguments.</param>
    /// <param name="namedArguments">The attribute's named arguments.</param>
    public AttributeCodeFragment(Type type, IEnumerable<object?> arguments, IDictionary<string, object?> namedArguments)
    {
        Type = type;
        _arguments = [..arguments];
        _namedArguments = new Dictionary<string, object?>(namedArguments);
    }

    /// <summary>
    ///     Gets or sets the attribute's type.
    /// </summary>
    /// <value> The attribute's type. </value>
    public virtual Type Type { get; }

    /// <summary>
    ///     Gets the attribute's positional arguments.
    /// </summary>
    /// <value> The arguments. </value>
    public virtual IReadOnlyList<object?> Arguments
        => _arguments;

    /// <summary>
    ///     Gets the attribute's named arguments.
    /// </summary>
    /// <value>The arguments.</value>
    public virtual IReadOnlyDictionary<string, object?> NamedArguments
        => _namedArguments;
}
