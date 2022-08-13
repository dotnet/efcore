// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StoreStoredProcedureResultColumn
    : ColumnBase<StoredProcedureResultColumnMapping>, IStoreStoredProcedureResultColumn
{
    private readonly RelationalTypeMapping? _storeTypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StoreStoredProcedureResultColumn(
        string name,
        string type,
        int position,
        StoreStoredProcedure storedProcedure,
        RelationalTypeMapping? storeTypeMapping = null)
        : base(name, type, storedProcedure)
    {
        Position = position;
        _storeTypeMapping = storeTypeMapping;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual StoreStoredProcedure StoredProcedure
        => (StoreStoredProcedure)Table;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int Position { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override RelationalTypeMapping StoreTypeMapping
        => _storeTypeMapping ?? base.StoreTypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IStoreStoredProcedureResultColumn)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IStoreStoredProcedureResultColumn)this).ToDebugString(),
            () => ((IStoreStoredProcedureResultColumn)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IStoreStoredProcedure IStoreStoredProcedureResultColumn.StoredProcedure
    {
        [DebuggerStepThrough]
        get => StoredProcedure;
    }

    /// <inheritdoc />
    IReadOnlyList<IStoredProcedureResultColumnMapping> IStoreStoredProcedureResultColumn.PropertyMappings
    {
        [DebuggerStepThrough]
        get => PropertyMappings;
    }
}
