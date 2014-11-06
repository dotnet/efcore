// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AtsDatabase : Database
    {
        public AtsDatabase([NotNull] DbContextConfiguration configuration, [NotNull] ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
        {
        }

        public virtual bool CreateTables()
        {
            return EnsureCreated();
        }

        public virtual bool DeleteTables()
        {
            return EnsureDeleted();
        }
    }
}
