// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ViewMapping : TableMappingBase<ViewColumnMapping>, IViewMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ViewMapping(
        IEntityType entityType,
        View view,
        bool? includesDerivedTypes)
        : base(entityType, view, includesDerivedTypes)
    {
    }

    /// <inheritdoc />
    public virtual IView View
        => (IView)base.Table;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void SetIsSharedTablePrincipal(bool isSharedTablePrincipal)
    {
        if (IsSharedTablePrincipal == isSharedTablePrincipal)
        {
            return;
        }

        ((View)View).EntityTypeMappings.Remove(this);

        var removedColumnMappings = new List<ViewColumnMapping>();
        foreach (ViewColumnMapping columnMapping in ((IViewMapping)this).ColumnMappings)
        {
            ((ViewColumn)columnMapping.Column).RemovePropertyMapping(columnMapping);
            var columnMappings = (SortedSet<ViewColumnMapping>)columnMapping.Property.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.ViewColumnMappings)!;
            columnMappings.Remove(columnMapping);

            removedColumnMappings.Add(columnMapping);
        }

        IsSharedTablePrincipal = isSharedTablePrincipal;

        // Re-add the mappings to update the order
        ((View)View).EntityTypeMappings.Add(this);

        foreach (var columnMapping in removedColumnMappings)
        {
            ((ViewColumn)columnMapping.Column).AddPropertyMapping(columnMapping);
            var columnMappings = (SortedSet<ViewColumnMapping>)columnMapping.Property.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.ViewColumnMappings)!;
            columnMappings.Add(columnMapping);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IViewMapping)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IEnumerable<IViewColumnMapping> IViewMapping.ColumnMappings
    {
        [DebuggerStepThrough]
        get => ColumnMappings;
    }
}
