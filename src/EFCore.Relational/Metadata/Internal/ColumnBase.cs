// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public class ColumnBase : Annotatable, IColumnBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ColumnBase([NotNull] string name, [NotNull] string type, [NotNull] TableBase table)
        {
            Name = name;
            StoreType = type;
            Table = table;
        }

        /// <inheritdoc />
        public virtual string Name { get; }

        /// <inheritdoc />
        public virtual ITableBase Table { get; }

        /// <inheritdoc />
        public virtual string StoreType { get; }

        /// <inheritdoc />
        public virtual bool IsNullable { get; set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedSet<ColumnMappingBase> PropertyMappings { get; }
            = new SortedSet<ColumnMappingBase>(ColumnMappingBaseComparer.Instance);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string Format([NotNull] IEnumerable<IColumnBase> columns)
            => "{"
                + string.Join(
                    ", ",
                    columns.Select(p => "'" + p.Name + "'"))
                + "}";

        /// <inheritdoc />
        IEnumerable<IColumnMappingBase> IColumnBase.PropertyMappings
        {
            [DebuggerStepThrough]
            get => PropertyMappings;
        }
    }
}
