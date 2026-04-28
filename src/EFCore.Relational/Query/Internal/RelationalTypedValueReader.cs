// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     An <see cref="ITypedValueReader{TState}" /> implementation that reads from a <see cref="DbDataReader" />
///     at a fixed column ordinal, using a <see cref="RelationalTypeMapping" /> for typed dispatch.
///     Immutable: the column ordinal and type mapping are fixed at construction time; the <see cref="DbDataReader" />
///     is passed per-call as state, so instances are safe to share across threads.
/// </remarks>
public sealed class RelationalTypedValueReader(int ordinal, RelationalTypeMapping typeMapping)
    : ITypedValueReader<DbDataReader>
{
    /// <inheritdoc />
    public T Read<T>(DbDataReader reader)
        => typeMapping.Read<T>(reader, ordinal);
}
