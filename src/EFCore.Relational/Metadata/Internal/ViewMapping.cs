// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public class ViewMapping : Annotatable, IViewMapping
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ViewMapping(
            [NotNull] IEntityType entityType,
            [NotNull] View view,
            bool includesDerivedTypes)
        {
            EntityType = entityType;
            View = view;
            IncludesDerivedTypes = includesDerivedTypes;
        }

        /// <inheritdoc/>
        public virtual IEntityType EntityType { get; }

        /// <inheritdoc/>
        public virtual IView View { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedSet<IViewColumnMapping> ColumnMappings { get; }
            = new SortedSet<IViewColumnMapping>(ColumnMappingBaseComparer.Instance);

        /// <inheritdoc/>
        public virtual bool IncludesDerivedTypes { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString() => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <inheritdoc/>
        ITableBase ITableMappingBase.Table
        {
            [DebuggerStepThrough]
            get => View;
        }

        /// <inheritdoc/>
        IEnumerable<IViewColumnMapping> IViewMapping.ColumnMappings
        {
            [DebuggerStepThrough]
            get => ColumnMappings;
        }

        /// <inheritdoc/>
        IEnumerable<IColumnMappingBase> ITableMappingBase.ColumnMappings
        {
            [DebuggerStepThrough]
            get => ColumnMappings;
        }
    }
}
