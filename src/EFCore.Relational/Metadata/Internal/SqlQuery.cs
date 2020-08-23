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
    public class SqlQuery : TableBase, ISqlQuery
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlQuery([NotNull] string name, [NotNull] RelationalModel model)
            : base(name, null, model)
        {
        }

        /// <inheritdoc />
        public virtual string Sql { get; [param: NotNull] set; }

        /// <inheritdoc />
        public override IColumnBase FindColumn(IProperty property)
            => property.GetSqlQueryColumnMappings()
                .FirstOrDefault(cm => cm.TableMapping.Table == this)
                ?.Column;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <inheritdoc />
        IEnumerable<ISqlQueryMapping> ISqlQuery.EntityTypeMappings
        {
            [DebuggerStepThrough]
            get => EntityTypeMappings.Cast<ISqlQueryMapping>();
        }

        /// <inheritdoc />
        IEnumerable<ISqlQueryColumn> ISqlQuery.Columns
        {
            [DebuggerStepThrough]
            get => Columns.Values.Cast<ISqlQueryColumn>();
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        ISqlQueryColumn ISqlQuery.FindColumn(string name)
            => (ISqlQueryColumn)base.FindColumn(name);

        /// <inheritdoc />
        [DebuggerStepThrough]
        ISqlQueryColumn ISqlQuery.FindColumn(IProperty property)
            => (ISqlQueryColumn)FindColumn(property);
    }
}
