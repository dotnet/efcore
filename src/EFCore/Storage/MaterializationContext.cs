// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Parameter object containing context needed for materialization of an entity.
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
public readonly struct MaterializationContext
{
    /// <summary>
    ///     The <see cref="MethodInfo" /> for the <see cref="ValueBuffer" /> get method.
    /// </summary>
    public static readonly MethodInfo GetValueBufferMethod
        = typeof(MaterializationContext).GetProperty(nameof(ValueBuffer))!.GetMethod!;

    internal static readonly PropertyInfo ContextProperty
        = typeof(MaterializationContext).GetProperty(nameof(Context))!;

    /// <summary>
    ///     Creates a new <see cref="MaterializationContext" /> instance.
    /// </summary>
    /// <param name="valueBuffer">The <see cref="ValueBuffer" /> to use to materialize an entity.</param>
    /// <param name="context">The current <see cref="DbContext" /> instance being used.</param>
    public MaterializationContext(
        in ValueBuffer valueBuffer,
        DbContext context)
    {
        Check.DebugAssert(context != null, "context is null"); // Hot path

        ValueBuffer = valueBuffer;
        Context = context;
    }

    /// <summary>
    ///     The <see cref="ValueBuffer" /> to use to materialize an entity.
    /// </summary>
    public ValueBuffer ValueBuffer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    /// <summary>
    ///     The current <see cref="DbContext" /> instance being used.
    /// </summary>
    public DbContext Context
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }
}
