// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Specifies which metadata to read from the database.
    /// </summary>
    public class DatabaseModelFactoryOptions
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseModelFactoryOptions" /> class.
        /// </summary>
        /// <param name="tables"> A list of tables to include. Empty to include all tables. </param>
        /// <param name="schemas"> A list of schemas to include. Empty to include all schemas. </param>
        public DatabaseModelFactoryOptions(IEnumerable<string>? tables = null, IEnumerable<string>? schemas = null)
        {
            Tables = tables ?? Enumerable.Empty<string>();
            Schemas = schemas ?? Enumerable.Empty<string>();
        }

        /// <summary>
        ///     Gets the list of tables to include. If empty, include all tables.
        /// </summary>
        public virtual IEnumerable<string> Tables { get; }

        /// <summary>
        ///     Gets the list of schemas to include. If empty, include all schemas.
        /// </summary>
        public virtual IEnumerable<string> Schemas { get; }
    }
}
