// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class TableBase : Annotatable, ITableBase
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

        /// <inheritdoc />
        public virtual string Schema { get; }

        /// <inheritdoc />
        public virtual string Name { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual RelationalModel Model { get; }

        /// <inheritdoc />
        public virtual bool IsShared { get; set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedSet<ITableMappingBase> EntityTypeMappings { get; }
            = new SortedSet<ITableMappingBase>(TableMappingBaseComparer.Instance);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<string, IColumnBase> Columns { get; [param: NotNull] protected set; }
            = new SortedDictionary<string, IColumnBase>(StringComparer.Ordinal);

        /// <inheritdoc />
        public virtual IColumnBase FindColumn(string name)
            => Columns.TryGetValue(name, out var column)
                ? column
                : null;

        /// <inheritdoc />
        public virtual IColumnBase FindColumn(IProperty property)
            => property.GetDefaultColumnMappings()
                .FirstOrDefault(cm => cm.TableMapping.Table == this)
                ?.Column;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<IEntityType, IEnumerable<IForeignKey>> RowInternalForeignKeys { get; [param: NotNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedDictionary<IEntityType, IEnumerable<IForeignKey>> ReferencingRowInternalForeignKeys
        {
            get;
            [param: NotNull] set;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Dictionary<IEntityType, bool> OptionalEntityTypes { get; [param: NotNull] set; }

        /// <inheritdoc />
        public virtual bool IsOptional(IEntityType entityType)
            => OptionalEntityTypes == null
                ? GetMappedEntityType(entityType) == null
                : !OptionalEntityTypes.TryGetValue(entityType, out var optional)
                    ? throw new InvalidOperationException(RelationalStrings.TableNotMappedEntityType(entityType.DisplayName(), Name))
                    : optional;

        private IEntityType GetMappedEntityType(IEntityType entityType)
            => EntityTypeMappings.Any(m => m.EntityType == entityType)
                ? entityType
                : throw new InvalidOperationException(RelationalStrings.TableNotMappedEntityType(entityType.DisplayName(), Name));

        /// <inheritdoc />
        IRelationalModel ITableBase.Model
            => Model;

        /// <inheritdoc />
        IEnumerable<ITableMappingBase> ITableBase.EntityTypeMappings
            => EntityTypeMappings;

        /// <inheritdoc />
        IEnumerable<IColumnBase> ITableBase.Columns
            => Columns.Values;

        /// <inheritdoc />
        IEnumerable<IForeignKey> ITableBase.GetRowInternalForeignKeys(IEntityType entityType)
            => RowInternalForeignKeys != null
                && RowInternalForeignKeys.TryGetValue(entityType, out var foreignKeys)
                    ? foreignKeys
                    : (GetMappedEntityType(entityType) == null)
                        ? null
                        : Enumerable.Empty<IForeignKey>();

        /// <inheritdoc />
        IEnumerable<IForeignKey> ITableBase.GetReferencingRowInternalForeignKeys(IEntityType entityType)
            => ReferencingRowInternalForeignKeys != null
                && ReferencingRowInternalForeignKeys.TryGetValue(entityType, out var foreignKeys)
                    ? foreignKeys
                    : (GetMappedEntityType(entityType) == null)
                        ? null
                        : Enumerable.Empty<IForeignKey>();
    }
}
