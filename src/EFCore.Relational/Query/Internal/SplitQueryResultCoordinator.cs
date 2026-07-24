// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class SplitQueryResultCoordinator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SplitQueryResultCoordinator()
        => ResultContext = new ResultContext();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ResultContext ResultContext { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool? HasNext { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IList<SplitQueryCollectionContext?> Collections { get; } = new List<SplitQueryCollectionContext?>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IList<SplitQueryDataReaderContext?> DataReaders { get; } = new List<SplitQueryDataReaderContext?>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void SetDataReader(int collectionId, RelationalDataReader relationalDataReader)
    {
        while (DataReaders.Count <= collectionId)
        {
            DataReaders.Add(null);
        }

        DataReaders[collectionId] = new SplitQueryDataReaderContext(relationalDataReader);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void SetSplitQueryCollectionContext(int collectionId, SplitQueryCollectionContext splitQueryCollectionContext)
    {
        while (Collections.Count <= collectionId)
        {
            Collections.Add(null);
        }

        Collections[collectionId] = splitQueryCollectionContext;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void VerifyNoOrphanedChildRows()
    {
        foreach (var dataReaderContext in DataReaders)
        {
            // Split collection queries correlate child rows to parents by consuming, for each parent (in order), the leading child
            // rows whose parent key matches. HasNext == true here means the split reader is parked on a child row that didn't match
            // the last parent - and since every parent has now been processed, that row (and any after it) belongs to no parent in
            // the parent query's results. This can only happen if the data was modified concurrently between the execution of the
            // parent query and the child query, leaving orphan child rows that would otherwise be silently dropped (see #33826).
            if (dataReaderContext?.HasNext == true)
            {
                throw new DbQueryConcurrencyException(RelationalStrings.SplitQueryConcurrentModification);
            }
        }
    }
}
