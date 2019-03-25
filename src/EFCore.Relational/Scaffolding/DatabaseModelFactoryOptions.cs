// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    /// Specifies which metadata to read from the database.
    /// </summary>
    public class DatabaseModelFactoryOptions
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseModelFactoryOptions" /> class.
        /// </summary>
        /// <param name="tables"> A list of tables to include. Empty to include all tables. </param>
        /// <param name="schemas"> A list of schemas to include. Empty to include all schemas. </param>
        public DatabaseModelFactoryOptions([NotNull] IEnumerable<string> tables, [NotNull] IEnumerable<string> schemas)
        {
            Check.NotNull(tables, nameof(tables));
            Check.NotNull(schemas, nameof(schemas));

            Tables = tables;
            Schemas = schemas;
        }

        /// <summary>
        /// Gets the list of tables to include. If empty, include all tables.
        /// </summary>
        public virtual IEnumerable<string> Tables { get; }

        /// <summary>
        /// Gets the list of schemas to include. If empty, include all schemas.
        /// </summary>
        public virtual IEnumerable<string> Schemas { get; }
    }
}
