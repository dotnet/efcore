// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class FunctionMapping : TableMappingBase<FunctionColumnMapping>, IFunctionMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public FunctionMapping(IEntityType entityType, StoreFunction storeFunction, IDbFunction dbFunction, bool? includesDerivedTypes)
        : base(entityType, storeFunction, includesDerivedTypes)
    {
        DbFunction = dbFunction;
    }

    /// <inheritdoc />
    public virtual bool IsDefaultFunctionMapping { get; set; }

    /// <inheritdoc />
    public virtual IStoreFunction StoreFunction
        => (IStoreFunction)base.Table;

    /// <inheritdoc />
    public virtual IDbFunction DbFunction { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IFunctionMapping)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IEnumerable<IFunctionColumnMapping> IFunctionMapping.ColumnMappings
    {
        [DebuggerStepThrough]
        get => ColumnMappings;
    }
}
