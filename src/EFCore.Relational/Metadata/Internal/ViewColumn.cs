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
    public class ViewColumn : Annotatable, IViewColumn
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ViewColumn([NotNull] string name, [NotNull] string type, [NotNull] View view)
        {
            Name = name;
            Type = type;
            View = view;
        }

        /// <inheritdoc/>
        public virtual string Name { get; }

        /// <inheritdoc/>
        public virtual IView View { get; }

        /// <inheritdoc/>
        public virtual string Type { get; }

        /// <inheritdoc/>
        public virtual bool IsNullable { get; set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SortedSet<ViewColumnMapping> PropertyMappings { get; }
            = new SortedSet<ViewColumnMapping>(ColumnMappingBaseComparer.Instance);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString() => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <inheritdoc/>
        IEnumerable<IViewColumnMapping> IViewColumn.PropertyMappings
        {
            [DebuggerStepThrough]
            get => PropertyMappings;
        }

        /// <inheritdoc/>
        IEnumerable<IColumnMappingBase> IColumnBase.PropertyMappings
        {
            [DebuggerStepThrough]
            get => PropertyMappings;
        }

        /// <inheritdoc/>
        ITableBase IColumnBase.Table
        {
            [DebuggerStepThrough]
            get => View;
        }
    }
}
