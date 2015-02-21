// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlServerDatabaseExtensions
    {
        public static SqlServerDatabase AsSqlServer([NotNull] this Database database)
        {
            Check.NotNull(database, nameof(database));

            var sqlServerDatabase = database as SqlServerDatabase;

            if (sqlServerDatabase == null)
            {
                throw new InvalidOperationException(Strings.SqlServerNotInUse);
            }

            return sqlServerDatabase;
        }
    }
}
