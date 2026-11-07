// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Carries a separate set of reader columns for each result set of a batched query command. Flows through
///     <see cref="RelationalCommandParameterObject.ReaderColumns" /> into <see cref="BufferedDataReader" />, which
///     unwraps it to buffer each result set with the columns of the query that produced it.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public sealed class BatchReaderColumns : IReadOnlyList<ReaderColumn?>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public BatchReaderColumns(IReadOnlyList<IReadOnlyList<ReaderColumn?>> resultSetColumns)
        => ResultSetColumns = resultSetColumns;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<ReaderColumn?>> ResultSetColumns { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlyList<ReaderColumn?> GetColumnsForResultSet(int resultSetIndex)
        => resultSetIndex < ResultSetColumns.Count
            ? ResultSetColumns[resultSetIndex]
            : ResultSetColumns[^1];

    int IReadOnlyCollection<ReaderColumn?>.Count
        => ResultSetColumns[0].Count;

    ReaderColumn? IReadOnlyList<ReaderColumn?>.this[int index]
        => ResultSetColumns[0][index];

    IEnumerator<ReaderColumn?> IEnumerable<ReaderColumn?>.GetEnumerator()
        => ResultSetColumns[0].GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ResultSetColumns[0].GetEnumerator();
}
