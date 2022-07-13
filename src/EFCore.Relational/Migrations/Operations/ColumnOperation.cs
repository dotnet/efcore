// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A <see cref="MigrationOperation" /> for operations on columns.
///     See also <see cref="AddColumnOperation" /> and <see cref="AlterColumnOperation" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
/// </remarks>
public abstract class ColumnOperation : MigrationOperation, ITableMigrationOperation
{
    /// <summary>
    ///     The name of the column.
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    ///     The schema that contains the table, or <see langword="null" /> if the default schema should be used.
    /// </summary>
    public virtual string? Schema { get; set; }

    /// <summary>
    ///     The table which contains the column.
    /// </summary>
    public virtual string Table { get; set; } = null!;

    /// <summary>
    ///     The CLR <see cref="Type" /> of the property or properties mapped to the column.
    /// </summary>
    public virtual Type ClrType { get; set; } = null!;

    /// <summary>
    ///     The store type of the column--for example, 'nvarchar(max)'.
    /// </summary>
    public virtual string? ColumnType { get; set; }

    /// <summary>
    ///     Indicates whether or not the column can contain Unicode data, or <see langword="null" /> if this is not specified or does
    ///     not apply to this column type.
    /// </summary>
    public virtual bool? IsUnicode { get; set; }

    /// <summary>
    ///     Indicates whether or not the column is constrained to fixed-length data.
    /// </summary>
    public virtual bool? IsFixedLength { get; set; }

    /// <summary>
    ///     The maximum amount of data that the column can store, or <see langword="null" /> if this is not specified or does
    ///     not apply to this column type.
    /// </summary>
    public virtual int? MaxLength { get; set; }

    /// <summary>
    ///     The maximum number of digits that the column can store, or <see langword="null" />
    ///     if this is not specified or does not apply to this column type.
    /// </summary>
    public virtual int? Precision { get; set; }

    /// <summary>
    ///     The maximum number of decimal places that the column can store, or <see langword="null" />
    ///     if this is not specified or does not apply to this column type.
    /// </summary>
    public virtual int? Scale { get; set; }

    /// <summary>
    ///     Indicates whether or not this column acts as an automatic concurrency token in the same vein
    ///     as 'rowversion'/'timestamp' columns on SQL Server.
    /// </summary>
    public virtual bool IsRowVersion { get; set; }

    /// <summary>
    ///     Indicates whether or not th column can store <see langword="null" /> values.
    /// </summary>
    public virtual bool IsNullable { get; set; }

    /// <summary>
    ///     The default value for rows inserted without an explicit value for this column, or
    ///     <see langword="null" /> if there is no default.
    /// </summary>
    public virtual object? DefaultValue { get; set; }

    /// <summary>
    ///     The SQL expression to use as the default constraint when creating the column,
    ///     or <see langword="null" /> if there is no default constraint.
    /// </summary>
    public virtual string? DefaultValueSql { get; set; }

    /// <summary>
    ///     The SQL expression to use to compute the column value, <see langword="null" /> if the column
    ///     is not computed.
    /// </summary>
    public virtual string? ComputedColumnSql { get; set; }

    /// <summary>
    ///     Whether the value of the computed column this property is mapped to is stored in the database, or calculated when
    ///     it is read.
    /// </summary>
    public virtual bool? IsStored { get; set; }

    /// <summary>
    ///     Comment for this column
    /// </summary>
    public virtual string? Comment { get; set; }

    /// <summary>
    ///     The collation for this column, or <see langword="null" /> if one hasn't been explicitly configured.
    /// </summary>
    public virtual string? Collation { get; set; }
}
