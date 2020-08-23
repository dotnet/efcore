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
    public class View : TableBase, IView
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public View([NotNull] string name, [CanBeNull] string schema, [NotNull] RelationalModel model)
            : base(name, schema, model)
        {
        }

        /// <inheritdoc />
        public virtual string ViewDefinitionSql
            => (string)EntityTypeMappings.Select(m => m.EntityType[RelationalAnnotationNames.ViewDefinitionSql])
                .FirstOrDefault(d => d != null);

        /// <inheritdoc />
        public override IColumnBase FindColumn(IProperty property)
            => property.GetViewColumnMappings()
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
        IEnumerable<IViewMapping> IView.EntityTypeMappings
        {
            [DebuggerStepThrough]
            get => EntityTypeMappings.Cast<IViewMapping>();
        }

        /// <inheritdoc />
        IEnumerable<IViewColumn> IView.Columns
        {
            [DebuggerStepThrough]
            get => Columns.Values.Cast<IViewColumn>();
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        IViewColumn IView.FindColumn(string name)
            => (IViewColumn)base.FindColumn(name);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IViewColumn IView.FindColumn(IProperty property)
            => (IViewColumn)FindColumn(property);
    }
}
