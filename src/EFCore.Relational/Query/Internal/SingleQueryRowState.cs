// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Represents single-query row state for the relational multi-call materialization protocol.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public sealed class SingleQueryRowState
{
    private SingleQueryRowStateValue _state = SingleQueryRowStateValue.PendingCurrentRow;

    private enum SingleQueryRowStateValue
    {
        ReadyCurrentRow,
        PendingCurrentRow,
        ReadyWithNextRow,
        PendingWithNextRow,
        ReadyNoMoreRows,
        PendingNoMoreRows
    }

    /// <summary>
    ///     Gets a value indicating whether the current materialization result is complete and can be returned.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public bool ResultReady
        => _state is SingleQueryRowStateValue.ReadyCurrentRow
            or SingleQueryRowStateValue.ReadyWithNextRow
            or SingleQueryRowStateValue.ReadyNoMoreRows;

    /// <summary>
    ///     Gets whether an additional buffered row exists for the current result.
    ///     <see langword="null" /> means the current row is still being consumed, <see langword="true" /> means the next
    ///     row has already been buffered, and <see langword="false" /> means no more rows exist for the current result.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public bool? HasNext
        => _state switch
        {
            SingleQueryRowStateValue.ReadyCurrentRow => null,
            SingleQueryRowStateValue.PendingCurrentRow => null,
            SingleQueryRowStateValue.ReadyWithNextRow => true,
            SingleQueryRowStateValue.PendingWithNextRow => true,
            SingleQueryRowStateValue.ReadyNoMoreRows => false,
            SingleQueryRowStateValue.PendingNoMoreRows => false,

            _ => throw new UnreachableException($"Unexpected row state: {_state}")
        };

    /// <summary>
    ///     Gets a value indicating whether the next row has already been buffered.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public bool HasBufferedNextRow
        => HasNext == true;

    /// <summary>
    ///     Gets a value indicating whether row reading for the current result has been exhausted.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public bool IsCurrentResultReaderExhausted
        => HasNext == false;

    /// <summary>
    ///     Sets whether the current materialization result is ready while preserving the current row-buffer state.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    /// <param name="resultReady">A value indicating whether the current result should be marked as ready.</param>
    public void SetResultReady(bool resultReady)
        => SetState(resultReady, HasNext);

    /// <summary>
    ///     Sets the current row-buffer state while preserving whether the current materialization result is ready.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    /// <param name="hasNext">
    ///     The buffered-row state: <see langword="null" /> for current row in progress, <see langword="true" /> for buffered next row,
    ///     or <see langword="false" /> for no remaining rows for this result.
    /// </param>
    public void SetHasNext(bool? hasNext)
        => SetState(ResultReady, hasNext);

    /// <summary>
    ///     Initializes state for a new result at the current row.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public void BeginResult()
        => SetState(resultReady: true, hasNext: null);

    /// <summary>
    ///     Marks the current result as pending additional materialization work.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public void MarkResultPending()
        => SetState(resultReady: false, hasNext: HasNext);

    /// <summary>
    ///     Marks the current result as ready to be returned.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public void MarkResultReady()
        => SetState(resultReady: true, hasNext: HasNext);

    /// <summary>
    ///     Marks that the next row has already been buffered for the following result.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public void MarkRowForNextResult()
        => SetState(resultReady: ResultReady, hasNext: true);

    /// <summary>
    ///     Marks the current row as consumed, clearing any buffered-next indication.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public void MarkCurrentRowConsumed()
        => SetState(resultReady: ResultReady, hasNext: null);

    /// <summary>
    ///     Marks that no more rows remain for the current result.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public void MarkNoMoreRowsForCurrentResult()
        => SetState(resultReady: ResultReady, hasNext: false);

    private void SetState(bool resultReady, bool? hasNext)
        => _state = (resultReady, hasNext) switch
        {
            (true, null) => SingleQueryRowStateValue.ReadyCurrentRow,
            (false, null) => SingleQueryRowStateValue.PendingCurrentRow,
            (true, true) => SingleQueryRowStateValue.ReadyWithNextRow,
            (false, true) => SingleQueryRowStateValue.PendingWithNextRow,
            (true, false) => SingleQueryRowStateValue.ReadyNoMoreRows,
            (false, false) => SingleQueryRowStateValue.PendingNoMoreRows
        };
}
