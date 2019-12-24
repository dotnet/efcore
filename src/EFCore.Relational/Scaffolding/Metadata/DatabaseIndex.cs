// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    /// <summary>
    ///     A simple model for a database index used when reverse engineering an existing database.
    /// </summary>
    public class DatabaseIndex : Annotatable
    {
        /// <summary>
        ///     The table that contains the index.
        /// </summary>
        public virtual DatabaseTable Table { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The index name.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The ordered list of columns that make up the index.
        /// </summary>
        public virtual IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();

        /// <summary>
        ///     Indicates whether or not the index constrains uniqueness.
        /// </summary>
        public virtual bool IsUnique { get; set; }

        /// <summary>
        ///     The filter expression, or <c>null</c> if the index has no filter.
        /// </summary>
        public virtual string Filter { get; [param: CanBeNull] set; }
    }
}
