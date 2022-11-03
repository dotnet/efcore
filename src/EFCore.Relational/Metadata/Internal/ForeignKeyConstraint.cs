// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ForeignKeyConstraint : Annotatable, IForeignKeyConstraint
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ForeignKeyConstraint(
        string name,
        Table table,
        Table principalTable,
        IReadOnlyList<Column> columns,
        UniqueConstraint principalUniqueConstraint,
        ReferentialAction onDeleteAction)
    {
        Name = name;
        Table = table;
        PrincipalTable = principalTable;
        Columns = columns;
        PrincipalUniqueConstraint = principalUniqueConstraint;
        OnDeleteAction = onDeleteAction;
    }

    /// <inheritdoc />
    public virtual string Name { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SortedSet<IForeignKey> MappedForeignKeys { get; } = new(ForeignKeyComparer.Instance);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Table Table { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Table PrincipalTable { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<Column> Columns { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<Column> PrincipalColumns
        => PrincipalUniqueConstraint.Columns;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual UniqueConstraint PrincipalUniqueConstraint { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsReadOnly
        => Table.Model.IsReadOnly;

    /// <inheritdoc />
    public virtual ReferentialAction OnDeleteAction { get; set; }

    private IRowForeignKeyValueFactory? _foreignKeyRowValueFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IRowForeignKeyValueFactory GetRowForeignKeyValueFactory()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _foreignKeyRowValueFactory, this,
            static constraint
                => constraint.Table.Model.Model.GetRelationalDependencies().RowForeignKeyValueFactoryFactory.Create(constraint));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IForeignKeyConstraint)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IEnumerable<IForeignKey> IForeignKeyConstraint.MappedForeignKeys
        => MappedForeignKeys;

    /// <inheritdoc />
    ITable IForeignKeyConstraint.Table
        => Table;

    /// <inheritdoc />
    ITable IForeignKeyConstraint.PrincipalTable
        => PrincipalTable;

    /// <inheritdoc />
    IReadOnlyList<IColumn> IForeignKeyConstraint.Columns
        => Columns;

    /// <inheritdoc />
    IReadOnlyList<IColumn> IForeignKeyConstraint.PrincipalColumns
        => PrincipalColumns;

    /// <inheritdoc />
    IUniqueConstraint IForeignKeyConstraint.PrincipalUniqueConstraint
        => PrincipalUniqueConstraint;
}
