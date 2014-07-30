// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Update;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerModificationCommandBatchFactory : ModificationCommandBatchFactory
    {
        public SqlServerModificationCommandBatchFactory(
            [NotNull] SqlServerSqlGenerator sqlGenerator, 
            [NotNull] DbContextConfiguration contextConfiguration)
            : base(sqlGenerator, contextConfiguration)
        {
        }
    }
}
