// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class Table : Annotatable, ITable
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Table([NotNull] string name, [CanBeNull] string schema)
        {
            Schema = schema;
            Name = name;
        }

        /// <inheritdoc/>
        public virtual string Schema { get; }

        /// <inheritdoc/>
        public virtual string Name { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedSet<ITableMapping> EntityTypeMappings { get; } = new SortedSet<ITableMapping>(TableMappingComparer.Instance);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<string, IColumn> Columns { get; } = new SortedDictionary<string, IColumn>(StringComparer.Ordinal);

        /// <inheritdoc/>
        public virtual bool IsMigratable { get; set; }

        /// <inheritdoc/>
        public virtual bool IsSplit { get; set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<IEntityType, IEnumerable<IForeignKey>> InternalForeignKeys { get; [param: NotNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<IEntityType, IEnumerable<IForeignKey>> ReferencingInternalForeignKeys { get; [param: NotNull] set; }

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

        IEnumerable<IForeignKey> ITable.GetInternalForeignKeys(IEntityType entityType)
            => InternalForeignKeys != null
                && InternalForeignKeys.TryGetValue(entityType, out var foreignKeys)
                ? foreignKeys
                : null;

        IEnumerable<IForeignKey> ITable.GetReferencingInternalForeignKeys(IEntityType entityType)
            => ReferencingInternalForeignKeys != null
                && ReferencingInternalForeignKeys.TryGetValue(entityType, out var foreignKeys)
                ? foreignKeys
                : null;
    }
}
