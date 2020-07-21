// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a view in the database.
    /// </summary>
    public interface IView : ITableBase
    {
        /// <summary>
        ///     Gets the entity type mappings.
        /// </summary>
        new IEnumerable<IViewMapping> EntityTypeMappings { get; }

        /// <summary>
        ///     Gets the columns defined for this view.
        /// </summary>
        new IEnumerable<IViewColumn> Columns { get; }

        /// <summary>
        ///     Gets the column with the given name. Returns <see langword="null" /> if no column with the given name is defined.
        /// </summary>
        new IViewColumn FindColumn([NotNull] string name);

        /// <summary>
        ///     Gets the column mapped to the given property. Returns <see langword="null" /> if no column is mapped to the given property.
        /// </summary>
        new IViewColumn FindColumn([NotNull] IProperty property);

        /// <summary>
        ///     Gets the view definition or <see langword="null" /> if this view is not managed by migrations.
        /// </summary>
        public string ViewDefinitionSql { get; }
    }
}
