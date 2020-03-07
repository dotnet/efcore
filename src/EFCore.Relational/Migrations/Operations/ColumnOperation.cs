// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for operations on columns.
    ///     See also <see cref="AddColumnOperation" /> and <see cref="AlterColumnOperation" />.
    /// </summary>
    public class ColumnOperation : MigrationOperation
    {
        /// <summary>
        ///     The CLR <see cref="Type" /> of the property or properties mapped to the column.
        /// </summary>
        public virtual Type ClrType { get; [param: NotNull] set; }

        /// <summary>
        ///     The store type of the column--for example, 'nvarchar(max)'.
        /// </summary>
        public virtual string ColumnType { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Indicates whether or not the column can contain Unicode data, or <c>null</c> if this is not specified or does
        ///     not apply to this column type.
        /// </summary>
        public virtual bool? IsUnicode { get; set; }

        /// <summary>
        ///     Indicates whether or not the column is constrained to fixed-length data.
        /// </summary>
        public virtual bool? IsFixedLength { get; set; }

        /// <summary>
        ///     The maximum amount of data that the column can store, or <c>null</c> if this is not specified or does
        ///     not apply to this column type.
        /// </summary>
        public virtual int? MaxLength { get; set; }

        /// <summary>
        ///     Indicates whether or not this column acts as an automatic concurrency token in the same vein
        ///     as 'rowversion'/'timestamp' columns on SQL Server.
        /// </summary>
        public virtual bool IsRowVersion { get; set; }

        /// <summary>
        ///     Indicates whether or not th column can store <c>NULL</c> values.
        /// </summary>
        public virtual bool IsNullable { get; set; }

        /// <summary>
        ///     The default value for rows inserted without an explicit value for this column, or
        ///     <c>null</c> if there is no default.
        /// </summary>
        public virtual object DefaultValue { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The SQL expression to use as the default constraint when creating the column,
        ///     or <c>null</c> if there is no default constraint.
        /// </summary>
        public virtual string DefaultValueSql { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The SQL expression to use to compute the column value, <c>null</c> if the column
        ///     is not computed.
        /// </summary>
        public virtual string ComputedColumnSql { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Comment for this column
        /// </summary>
        public virtual string Comment { get; [param: CanBeNull] set; }
    }
}
