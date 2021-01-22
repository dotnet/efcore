// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class TableMappingBase : Annotatable, ITableMappingBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public TableMappingBase(
            [NotNull] IEntityType entityType,
            [NotNull] TableBase table,
            bool includesDerivedTypes)
        {
            EntityType = entityType;
            Table = table;
            IncludesDerivedTypes = includesDerivedTypes;
        }

        /// <inheritdoc />
        public virtual IEntityType EntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TableBase Table { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool IsReadOnly => Table.IsReadOnly;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedSet<ColumnMappingBase> ColumnMappings { get; }
            = new(ColumnMappingBaseComparer.Instance);

        /// <inheritdoc />
        public virtual bool IncludesDerivedTypes { get; }

        /// <inheritdoc />
        public virtual bool IsSharedTablePrincipal { get; set; }

        /// <inheritdoc />
        public virtual bool IsSplitEntityTypePrincipal { get; set; }

        IEnumerable<IColumnMappingBase> ITableMappingBase.ColumnMappings
        {
            [DebuggerStepThrough]
            get => ColumnMappings;
        }

        ITableBase ITableMappingBase.Table
        {
            [DebuggerStepThrough]
            get => Table;
        }
    }
}
