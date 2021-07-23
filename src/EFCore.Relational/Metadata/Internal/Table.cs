// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private UniqueConstraint? _primaryKey;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Table(string name, string? schema, RelationalModel model)
            : base(name, schema, model)
        {
            Columns = new SortedDictionary<string, ColumnBase>(new ColumnNameComparer(this));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<string, ForeignKeyConstraint> ForeignKeyConstraints { get; }
            = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual UniqueConstraint? PrimaryKey
        {
            get => _primaryKey;
            set
            {
                var oldPrimaryKey = _primaryKey;
                if (oldPrimaryKey != null)
                {
                    foreach (var column in oldPrimaryKey.Columns)
                    {
                        Columns.Remove(column.Name);
                    }
                }

                if (value != null)
                {
                    foreach (var column in value.Columns)
                    {
                        Columns.Remove(column.Name);
                    }
                }

                _primaryKey = value;

                if (oldPrimaryKey != null)
                {
                    foreach (var column in oldPrimaryKey.Columns)
                    {
                        Columns.TryAdd(column.Name, column);
                    }
                }

                if (value != null)
                {
                    foreach (var column in value.Columns)
                    {
                        Columns.TryAdd(column.Name, column);
                    }
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<string, UniqueConstraint> UniqueConstraints { get; }
            = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual UniqueConstraint? FindUniqueConstraint(string name)
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
            = new();

        /// <inheritdoc />
        public virtual bool IsExcludedFromMigrations => EntityTypeMappings.First().EntityType.IsTableExcludedFromMigrations();

        /// <inheritdoc />
        public override IColumnBase? FindColumn(IProperty property)
            => property.GetTableColumnMappings()
                .FirstOrDefault(cm => cm.TableMapping.Table == this)
                ?.Column;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => ((ITable)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <inheritdoc />
        IEnumerable<ITableMapping> ITable.EntityTypeMappings
        {
            [DebuggerStepThrough]
            get => base.EntityTypeMappings.Cast<ITableMapping>();
        }

        /// <inheritdoc />
        IEnumerable<IColumn> ITable.Columns
        {
            [DebuggerStepThrough]
            get => base.Columns.Values.Cast<IColumn>();
        }

        /// <inheritdoc />
        IEnumerable<IForeignKeyConstraint> ITable.ForeignKeyConstraints
        {
            [DebuggerStepThrough]
            get => ForeignKeyConstraints.Values;
        }

        /// <inheritdoc />
        IPrimaryKeyConstraint? ITable.PrimaryKey
        {
            [DebuggerStepThrough]
            get => PrimaryKey;
        }

        /// <inheritdoc />
        IEnumerable<IUniqueConstraint> ITable.UniqueConstraints
        {
            [DebuggerStepThrough]
            get => UniqueConstraints.Values;
        }

        /// <inheritdoc />
        IEnumerable<ITableIndex> ITable.Indexes
        {
            [DebuggerStepThrough]
            get => Indexes.Values;
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        IColumn? ITable.FindColumn(string name)
            => (IColumn?)base.FindColumn(name);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IColumn? ITable.FindColumn(IProperty property)
            => (IColumn?)FindColumn(property);
    }
}
