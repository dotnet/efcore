// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     An <see cref="ITypedValueReader{TState}" /> that wraps an inner reader and applies a value conversion.
///     The inner reader returns <typeparamref name="TProvider" /> values; this reader converts them
///     to <typeparamref name="TModel" /> before returning.
///     Created once per converted property at query-compilation time and reused for every row.
/// </remarks>
[EntityFrameworkInternal]
public sealed class ConvertingTypedValueReader<TModel, TProvider, TState> : ITypedValueReader<TState>
{
    private readonly ITypedValueReader<TState> _inner;
    private readonly Func<TProvider, TModel> _convert;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ConvertingTypedValueReader(ITypedValueReader<TState> inner, Func<TProvider, TModel> convert)
    {
        _inner = inner;
        _convert = convert;
    }

    /// <inheritdoc />
    public T Read<T>(TState state)
    {
        var providerValue = _inner.Read<TProvider>(state);
        var modelValue = _convert(providerValue);
        return (T)(object)modelValue!;
    }
}
