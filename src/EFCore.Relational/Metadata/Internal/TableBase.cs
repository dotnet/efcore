// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
    public abstract class TableBase : Annotatable, ITableBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public TableBase([NotNull] string name, [CanBeNull] string schema, [NotNull] RelationalModel model)
        {
            Schema = schema;
            Name = name;
            Model = model;
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
        public virtual RelationalModel Model { get; }

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
        IEnumerable<ITableMappingBase> ITableBase.EntityTypeMappings => throw new NotImplementedException();

        /// <inheritdoc/>
        IEnumerable<IColumnBase> ITableBase.Columns => throw new NotImplementedException();

        /// <inheritdoc/>
        IRelationalModel ITableBase.Model => Model;

        /// <inheritdoc/>
        IColumnBase ITableBase.FindColumn(string name) => throw new NotImplementedException();

        /// <inheritdoc/>
        IEnumerable<IForeignKey> ITableBase.GetInternalForeignKeys(IEntityType entityType)
            => InternalForeignKeys != null
                && InternalForeignKeys.TryGetValue(entityType, out var foreignKeys)
                ? foreignKeys
                : null;

        /// <inheritdoc/>
        IEnumerable<IForeignKey> ITableBase.GetReferencingInternalForeignKeys(IEntityType entityType)
            => ReferencingInternalForeignKeys != null
                && ReferencingInternalForeignKeys.TryGetValue(entityType, out var foreignKeys)
                ? foreignKeys
                : null;
    }
}
