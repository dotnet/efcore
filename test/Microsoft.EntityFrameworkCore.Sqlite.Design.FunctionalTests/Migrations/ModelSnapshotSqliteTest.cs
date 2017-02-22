// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.FunctionalTests.Migrations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class ModelSnapshotSqliteTest : ModelSnapshotTest
    {
        protected override ModelBuilder CreateConventionalModelBuilder() => new ModelBuilder(SqliteConventionSetBuilder.Build());
    }
}
