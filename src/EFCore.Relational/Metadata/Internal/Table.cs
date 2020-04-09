// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class Table : TableBase, ITable
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Table([NotNull] string name, [CanBeNull] string schema, [NotNull] RelationalModel model)
            : base(name, schema, model)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedSet<TableMapping> EntityTypeMappings { get; } = new SortedSet<TableMapping>(TableMappingBaseComparer.Instance);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<string, ForeignKeyConstraint> ForeignKeyConstraints { get; }
            = new SortedDictionary<string, ForeignKeyConstraint>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual UniqueConstraint PrimaryKey { get; [param: NotNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<string, UniqueConstraint> UniqueConstraints { get; }
            = new SortedDictionary<string, UniqueConstraint>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual UniqueConstraint FindUniqueConstraint([NotNull] string name)
            => PrimaryKey != null && PrimaryKey.Name == name
                ? PrimaryKey
                : UniqueConstraints.TryGetValue(name, out var constraint)
                    ? constraint
                    : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<string, TableIndex> Indexes { get; }
            = new SortedDictionary<string, TableIndex>();

        /// <inheritdoc/>
        public virtual bool IsMigratable { get; set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<string, Column> Columns { get; } = new SortedDictionary<string, Column>(StringComparer.Ordinal);

        /// <inheritdoc/>
        public virtual IColumn FindColumn(string name)
            => Columns.TryGetValue(name, out var column)
                ? column
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString() => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <inheritdoc/>
        IEnumerable<IColumnBase> ITableBase.Columns
        {
            [DebuggerStepThrough]
            get => Columns.Values;
        }

        /// <inheritdoc/>
        IEnumerable<IColumn> ITable.Columns
        {
            [DebuggerStepThrough]
            get => Columns.Values;
        }

        /// <inheritdoc/>
        IEnumerable<ITableMapping> ITable.EntityTypeMappings
        {
            [DebuggerStepThrough]
            get => EntityTypeMappings;
        }

        /// <inheritdoc/>
        IEnumerable<ITableMappingBase> ITableBase.EntityTypeMappings
        {
            [DebuggerStepThrough]
            get => EntityTypeMappings;
        }

        /// <inheritdoc/>
        IEnumerable<IForeignKeyConstraint> ITable.ForeignKeyConstraints
        {
            [DebuggerStepThrough]
            get => ForeignKeyConstraints.Values;
        }

        /// <inheritdoc/>
        IUniqueConstraint ITable.PrimaryKey
        {
            [DebuggerStepThrough]
            get => PrimaryKey;
        }

        /// <inheritdoc/>
        IEnumerable<IUniqueConstraint> ITable.UniqueConstraints
        {
            [DebuggerStepThrough]
            get => UniqueConstraints.Values;
        }

        /// <inheritdoc/>
        IEnumerable<ITableIndex> ITable.Indexes
        {
            [DebuggerStepThrough]
            get => Indexes.Values;
        }

        /// <inheritdoc/>
        IColumnBase ITableBase.FindColumn(string name) => FindColumn(name);

        /// <inheritdoc/>
        IEnumerable<IForeignKey> ITableBase.GetInternalForeignKeys(IEntityType entityType)
            => InternalForeignKeys != null
                && InternalForeignKeys.TryGetValue(entityType, out var foreignKeys)
                ? foreignKeys
                : Enumerable.Empty<IForeignKey>();

        /// <inheritdoc/>
        IEnumerable<IForeignKey> ITableBase.GetReferencingInternalForeignKeys(IEntityType entityType)
            => ReferencingInternalForeignKeys != null
                && ReferencingInternalForeignKeys.TryGetValue(entityType, out var foreignKeys)
                ? foreignKeys
                : Enumerable.Empty<IForeignKey>();
    }
}
