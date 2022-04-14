// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         Base class for types that support reading and writing annotations.
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
public class Annotatable : AnnotatableBase, IMutableAnnotatable
{
    /// <summary>
    ///     Throws if the model is not read-only.
    /// </summary>
    protected override void EnsureReadOnly()
    {
        if (!IsReadOnly)
        {
            throw new InvalidOperationException(CoreStrings.ModelMutable);
        }
    }

    /// <summary>
    ///     Throws if the model is read-only.
    /// </summary>
    protected override void EnsureMutable()
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException(CoreStrings.ModelReadOnly);
        }
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IAnnotation IMutableAnnotatable.AddAnnotation(string name, object? value)
        => AddAnnotation(name, value);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IAnnotation? IMutableAnnotatable.RemoveAnnotation(string name)
        => RemoveAnnotation(name);

    /// <inheritdoc />
    void IMutableAnnotatable.SetOrRemoveAnnotation(string name, object? value)
        => this[name] = value;
}
