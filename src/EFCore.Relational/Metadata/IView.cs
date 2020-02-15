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
        ///     The entity type mappings.
        /// </summary>
        new IEnumerable<IViewMapping> EntityTypeMappings { get; }

        /// <summary>
        ///     The columns defined for this view.
        /// </summary>
        new IEnumerable<IViewColumn> Columns { get; }

        /// <summary>
        ///     Returns the column with a given name. Returns <c>null</c> if no column with the given name is defined.
        /// </summary>
        new IViewColumn FindColumn([NotNull] string name);

        /// <summary>
        ///     The view definition or <c>null</c> if this view is not managed by migrations.
        /// </summary>
        public string ViewDefinition { get; }
    }
}
