// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ColumnMapping : ColumnMappingBase, IColumnMapping
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ColumnMapping(
            [NotNull] IProperty property,
            [NotNull] Column column,
            [NotNull] TableMapping tableMapping)
            : base(property, column, tableMapping)
        {
        }

        /// <inheritdoc />
        public new virtual ITableMapping TableMapping
            => (ITableMapping)base.TableMapping;

        /// <inheritdoc />
        public override RelationalTypeMapping TypeMapping => Property.FindRelationalTypeMapping(
            StoreObjectIdentifier.Table(TableMapping.Table.Name, TableMapping.Table.Schema))!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <inheritdoc />
        IColumn IColumnMapping.Column
        {
            [DebuggerStepThrough]
            get => (IColumn)Column;
        }
    }
}
