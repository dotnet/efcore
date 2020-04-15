// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
    public class View : TableBase, IView
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public View([NotNull] string name, [CanBeNull] string schema, [NotNull] RelationalModel model)
            : base(name, schema, model)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedSet<IViewMapping> EntityTypeMappings { get; } = new SortedSet<IViewMapping>(TableMappingBaseComparer.Instance);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<string, ViewColumn> Columns { get; }
            = new SortedDictionary<string, ViewColumn>(StringComparer.Ordinal);

        /// <inheritdoc/>
        public virtual string ViewDefinition
            => (string)EntityTypeMappings.Select(m => m.EntityType[RelationalAnnotationNames.ViewDefinition]).FirstOrDefault(d => d != null);

        /// <inheritdoc/>
        public virtual IViewColumn FindColumn(string name)
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
        [DebuggerStepThrough]
        IColumnBase ITableBase.FindColumn(string name) => FindColumn(name);

        /// <inheritdoc/>
        IEnumerable<IColumnBase> ITableBase.Columns
        {
            [DebuggerStepThrough]
            get => Columns.Values;
        }

        /// <inheritdoc/>
        IEnumerable<IViewColumn> IView.Columns
        {
            [DebuggerStepThrough]
            get => Columns.Values;
        }

        /// <inheritdoc/>
        IEnumerable<IViewMapping> IView.EntityTypeMappings
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
