// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ViewMapping : TableMappingBase, IViewMapping
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ViewMapping(
            IEntityType entityType,
            View view,
            bool includesDerivedTypes)
            : base(entityType, view, includesDerivedTypes)
        {
        }

        /// <inheritdoc />
        public virtual IView View
            => (IView)base.Table;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => ((IViewMapping)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <inheritdoc />
        IEnumerable<IViewColumnMapping> IViewMapping.ColumnMappings
        {
            [DebuggerStepThrough]
            get => ColumnMappings.Cast<IViewColumnMapping>();
        }
    }
}
