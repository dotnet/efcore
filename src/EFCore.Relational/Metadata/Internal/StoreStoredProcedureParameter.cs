// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StoreStoredProcedureParameter
    : ColumnBase<StoredProcedureParameterMapping>, IStoreStoredProcedureParameter
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StoreStoredProcedureParameter(
        string name,
        string type,
        int position,
        StoreStoredProcedure storedProcedure,
        ParameterDirection direction,
        RelationalTypeMapping? storeTypeMapping = null)
        : base(name, type, storedProcedure, storeTypeMapping)
    {
        Position = position;
        Direction = direction;
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
    public virtual ParameterDirection Direction { get; }

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
    protected override RelationalTypeMapping GetDefaultStoreTypeMapping()
        => PropertyMappings.Count != 0
            ? PropertyMappings[0].TypeMapping
            : (RelationalTypeMapping)Table.Model.Model.GetModelDependencies().TypeMappingSource.FindMapping(typeof(int))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IStoreStoredProcedureParameter)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IStoreStoredProcedureParameter)this).ToDebugString(),
            () => ((IStoreStoredProcedureParameter)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IStoreStoredProcedure IStoreStoredProcedureParameter.StoredProcedure
    {
        [DebuggerStepThrough]
        get => StoredProcedure;
    }

    /// <inheritdoc />
    IReadOnlyList<IStoredProcedureParameterMapping> IStoreStoredProcedureParameter.PropertyMappings
    {
        [DebuggerStepThrough]
        get => PropertyMappings;
    }
}
