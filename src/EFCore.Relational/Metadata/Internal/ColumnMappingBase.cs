// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ColumnMappingBase : Annotatable, IColumnMappingBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ColumnMappingBase(
            [NotNull] IProperty property,
            [NotNull] ColumnBase column,
            [NotNull] RelationalTypeMapping typeMapping,
            [NotNull] TableMappingBase tableMapping)
        {
            Property = property;
            Column = column;
            TypeMapping = typeMapping;
            TableMapping = tableMapping;
        }

        /// <inheritdoc />
        public virtual IProperty Property { get; }

        /// <inheritdoc />
        public virtual IColumnBase Column { get; }

        /// <inheritdoc />
        public virtual RelationalTypeMapping TypeMapping { get; }

        /// <inheritdoc />
        public virtual ITableMappingBase TableMapping { get; }
    }
}
