// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    /// <summary>
    ///     A simple model for a database view used when reverse engineering an existing database.
    /// </summary>
    public class DatabaseView : DatabaseTable
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DatabaseView" /> class.
        /// </summary>
        /// <param name="database"> The database containing this view. </param>
        /// <param name="name"> The name of the view. </param>
        public DatabaseView([NotNull] DatabaseModel database, [NotNull] string name)
            : base(database, name)
        {
        }
    }
}
