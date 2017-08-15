// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     A service typically implemented by database providers to reverse engineer a database into
    ///     a <see cref="DatabaseModel" />.
    /// </summary>
    public interface IDatabaseModelFactory
    {
        /// <summary>
        ///     Connects to the database using the given connection string and creates a <see cref="DatabaseModel" />
        ///     for the database.
        /// </summary>
        /// <param name="connectionString"> The connection string for the database to reverse engineer. </param>
        /// <param name="tables"> The tables to include in the model, or an empty enumerable to include all. </param>
        /// <param name="schemas"> The schema to include in the model, or an empty enumerable to include all. </param>
        /// <returns> The database model. </returns>
        DatabaseModel Create([NotNull] string connectionString, [NotNull] IEnumerable<string> tables, [NotNull] IEnumerable<string> schemas);

        /// <summary>
        ///     Connects to the database using the given connection and creates a <see cref="DatabaseModel" />
        ///     for the database.
        /// </summary>
        /// <param name="connection"> The connection to the database to reverse engineer. </param>
        /// <param name="tables"> The tables to include in the model, or an empty enumerable to include all. </param>
        /// <param name="schemas"> The schema to include in the model, or an empty enumerable to include all. </param>
        /// <returns> The database model. </returns>
        DatabaseModel Create([NotNull] DbConnection connection, [NotNull] IEnumerable<string> tables, [NotNull] IEnumerable<string> schemas);
    }
}
