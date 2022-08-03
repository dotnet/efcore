// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StoredProcedureResultColumnMapping : ColumnMappingBase, IStoredProcedureResultColumnMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StoredProcedureResultColumnMapping(
        IProperty property,
        StoreStoredProcedureResultColumn column,
        StoredProcedureMapping storedProcedureMapping)
        : base(property, column, storedProcedureMapping)
    {
    }

    /// <inheritdoc />
    public virtual IStoredProcedureMapping StoredProcedureMapping
        => (IStoredProcedureMapping)TableMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping GetTypeMapping()
        => Property.FindRelationalTypeMapping(StoredProcedureMapping.StoredProcedureIdentifier)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IStoredProcedureResultColumnMapping)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IStoreStoredProcedureResultColumn IStoredProcedureResultColumnMapping.Column
    {
        [DebuggerStepThrough]
        get => (IStoreStoredProcedureResultColumn)Column;
    }
}
