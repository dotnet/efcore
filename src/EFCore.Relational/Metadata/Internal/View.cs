// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class View : TableBase, IView
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public View(string name, string? schema, RelationalModel model)
        : base(name, schema, model)
    {
    }

    /// <inheritdoc />
    public virtual string? ViewDefinitionSql
        => (string?)EntityTypeMappings.Select(m => m.TypeBase[RelationalAnnotationNames.ViewDefinitionSql])
            .FirstOrDefault(d => d != null);

    /// <inheritdoc />
    public override IColumnBase? FindColumn(IProperty property)
        => property.GetViewColumnMappings()
            .FirstOrDefault(cm => cm.TableMapping.Table == this)
            ?.Column;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual ViewColumn? FindColumn(string name)
        => (ViewColumn?)base.FindColumn(name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IView)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IEnumerable<IViewMapping> IView.EntityTypeMappings
    {
        [DebuggerStepThrough]
        get => EntityTypeMappings.Cast<IViewMapping>();
    }

    /// <inheritdoc />
    IEnumerable<IViewColumn> IView.Columns
    {
        [DebuggerStepThrough]
        get => Columns.Values.Cast<IViewColumn>();
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IViewColumn? IView.FindColumn(string name)
        => (IViewColumn?)base.FindColumn(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IViewColumn? IView.FindColumn(IProperty property)
        => (IViewColumn?)FindColumn(property);
}
