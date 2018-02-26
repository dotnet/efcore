// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Initialization
{
    public class InitializationSqliteTests : InitializationTests<ColdStartEnabledSqliteTest>
    {
        protected override ConventionSet CreateConventionSet()
        {
            return SqliteConventionSetBuilder.Build();
        }

        // TODO: Following are disabled due to no database.
        public override void CreateAndDisposeUnusedContext()
        {
            base.CreateAndDisposeUnusedContext();
        }

        public override void InitializeAndQuery_AdventureWorks()
        {
            base.InitializeAndQuery_AdventureWorks();
        }

        public override void InitializeAndSaveChanges_AdventureWorks()
        {
            base.InitializeAndSaveChanges_AdventureWorks();
        }
    }
}
