// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Update;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public interface IInMemoryStore
    {
        /// <summary>
        ///     Returns true just after the Store has been created, false thereafter
        /// </summary>
        /// <returns>
        ///     true if the Store has just been created, false otherwise
        /// </returns>
        bool EnsureCreated([NotNull] IModel model);

        bool Clear();

        IReadOnlyList<InMemoryTableSnapshot> GetTables([NotNull] IEntityType entityType);

        int ExecuteTransaction([NotNull] IEnumerable<IUpdateEntry> entries);
    }
}
