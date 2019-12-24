// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    /// <summary>
    ///     Base class used by database providers to reverse engineer a database into a <see cref="DatabaseModel" />.
    /// </summary>
    public abstract class DatabaseModelFactory : IDatabaseModelFactory
    {
        /// <summary>
        ///     Connects to the database using the given connection string and creates a <see cref="DatabaseModel" />
        ///     for the database.
        /// </summary>
        /// <param name="connectionString"> The connection string for the database to reverse engineer. </param>
        /// <param name="options"> The options specifying which metadata to read. </param>
        /// <returns> The database model. </returns>
        public abstract DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options);

        /// <summary>
        ///     Connects to the database using the given connection and creates a <see cref="DatabaseModel" />
        ///     for the database.
        /// </summary>
        /// <param name="connection"> The connection to the database to reverse engineer. </param>
        /// <param name="options"> The options specifying which metadata to read. </param>
        /// <returns> The database model. </returns>
        public abstract DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options);
    }
}
