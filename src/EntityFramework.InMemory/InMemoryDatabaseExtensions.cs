// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class InMemoryDatabaseExtensions
    {
        public static InMemoryDatabaseFacade AsInMemory([NotNull] this Database database)
        {
            Check.NotNull(database, nameof(database));

            var inMemoryDatabase = database as InMemoryDatabaseFacade;

            if (inMemoryDatabase == null)
            {
                throw new InvalidOperationException(Strings.InMemoryNotInUse);
            }

            return inMemoryDatabase;
        }
    }
}
