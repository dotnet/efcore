// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         An arbitrary piece of metadata that can be stored on an object that implements <see cref="IReadOnlyAnnotatable" />.
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
public class Annotation : IAnnotation
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Annotation" /> class.
    /// </summary>
    /// <param name="name">The key of this annotation.</param>
    /// <param name="value">The value assigned to this annotation.</param>
    public Annotation(string name, object? value)
    {
        Check.NotEmpty(name, nameof(name));

        Name = name;
        Value = value;
    }

    /// <summary>
    ///     Gets the key of this annotation.
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    ///     Gets the value assigned to this annotation.
    /// </summary>
    public virtual object? Value { get; }
}
