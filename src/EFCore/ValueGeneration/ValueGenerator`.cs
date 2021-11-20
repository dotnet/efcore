// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

/// <summary>
///     Generates values for properties when an entity is added to a context.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
/// </remarks>
public abstract class ValueGenerator<TValue> : ValueGenerator
{
    /// <summary>
    ///     Template method to be overridden by implementations to perform value generation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
    /// </remarks>
    /// <param name="entry">The change tracking entry of the entity for which the value is being generated.</param>
    /// <returns>The generated value.</returns>
    public new abstract TValue Next(EntityEntry entry);

    /// <summary>
    ///     Template method to be overridden by implementations to perform value generation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
    /// </remarks>
    /// <param name="entry">The change tracking entry of the entity for which the value is being generated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The generated value.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public new virtual ValueTask<TValue> NextAsync(
        EntityEntry entry,
        CancellationToken cancellationToken = default)
        => new(Next(entry));

    /// <summary>
    ///     Gets a value to be assigned to a property.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
    /// </remarks>
    /// <param name="entry">The change tracking entry of the entity for which the value is being generated.</param>
    /// <returns>The value to be assigned to a property.</returns>
    protected override object? NextValue(EntityEntry entry)
        => Next(entry);

    /// <summary>
    ///     Gets a value to be assigned to a property.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
    /// </remarks>
    /// <param name="entry">The change tracking entry of the entity for which the value is being generated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The value to be assigned to a property.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    protected override async ValueTask<object?> NextValueAsync(
        EntityEntry entry,
        CancellationToken cancellationToken = default)
        => await NextAsync(entry, cancellationToken).ConfigureAwait(false);
}
