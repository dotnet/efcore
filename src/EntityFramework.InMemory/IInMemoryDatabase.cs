// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.InMemory
{
    public interface IInMemoryDatabase : IEnumerable<InMemoryDatabase.InMemoryTable>
    {
        /// <summary>
        ///     Returns true just after the database has been created, false thereafter
        /// </summary>
        /// <returns>
        ///     true if the database has just been created, false otherwise
        /// </returns>
        bool EnsureCreated([NotNull] IModel model);

        void Clear();

        IEnumerable<InMemoryDatabase.InMemoryTable> GetTables([NotNull] IEntityType entityType);

        int ExecuteTransaction([NotNull] IEnumerable<InternalEntityEntry> entries);
    }
}
