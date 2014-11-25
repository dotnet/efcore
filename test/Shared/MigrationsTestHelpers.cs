// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Tests;
using Moq;

namespace Microsoft.Data.Entity.Migrations.Tests
{
    public static class MigrationsTestHelpers
    {
        public static Mock<MigrationOperationSqlGenerator> MockSqlGenerator(bool callBase = false)
        {
            return new Mock<MigrationOperationSqlGenerator>(RelationalTestHelpers.ExtensionProvider(), new RelationalTypeMapper()) { CallBase = callBase };
        }
    }
}
