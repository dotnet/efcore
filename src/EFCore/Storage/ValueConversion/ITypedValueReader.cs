// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     A reader that can return a typed value, abstracting away the underlying data source.
///     Used by <see cref="ValueConverter" /> to read provider-typed values without coupling
///     to specific data reader implementations.
/// </summary>
/// <typeparam name="TState">
///     The state type passed to <see cref="Read{T}" />. Callers provide the state per-call rather than
///     storing it in the reader, which keeps reader instances immutable and safe to share across threads.
///     For relational providers this is <see cref="System.Data.Common.DbDataReader" />.
/// </typeparam>
public interface ITypedValueReader<TState>
{
    /// <summary>
    ///     Reads a value of type <typeparamref name="T" /> from the underlying data source.
    /// </summary>
    /// <typeparam name="T">The type of value to read.</typeparam>
    /// <param name="state">The data-source state for this read (e.g. the current <see cref="System.Data.Common.DbDataReader" />).</param>
    /// <returns>The typed value.</returns>
    T Read<T>(TState state);
}
