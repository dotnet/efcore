// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerComplianceTest : RelationalComplianceTestBase
    {
        protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
        {
            typeof(LoadTestBase<>),                        // issue #15318
            typeof(GraphUpdatesTestBase<>),                // issue #15318
            typeof(ProxyGraphUpdatesTestBase<>),           // issue #15318
            typeof(ComplexNavigationsWeakQueryTestBase<>), // issue #15285
            typeof(FiltersInheritanceTestBase<>),          // issue #15264
            typeof(FiltersTestBase<>),                     // issue #15264
            typeof(OwnedQueryTestBase<>),                  // issue #15285
            typeof(QueryFilterFuncletizationTestBase<>),   // issue #15264
            typeof(RelationalOwnedQueryTestBase<>),         // issue #15285
            // Query pipeline
            typeof(ConcurrencyDetectorTestBase<>),
            typeof(CompiledQueryTestBase<>),
            typeof(GearsOfWarQueryTestBase<>),
            typeof(IncludeAsyncTestBase<>),
            typeof(IncludeOneToOneTestBase<>),
            typeof(InheritanceRelationshipsQueryTestBase<>),
            typeof(InheritanceTestBase<>),
            typeof(NullKeysTestBase<>),
            typeof(QueryNavigationsTestBase<>),
            typeof(ConcurrencyDetectorRelationalTestBase<>),
            typeof(AsyncFromSqlQueryTestBase<>),
            typeof(QueryTaggingTestBase<>),
            typeof(FromSqlQueryTestBase<>),
            typeof(GearsOfWarFromSqlQueryTestBase<>),
            typeof(InheritanceRelationalTestBase<>),
            typeof(NullSemanticsQueryTestBase<>),
            typeof(QueryNoClientEvalTestBase<>),
            typeof(WarningsTestBase<>),
            typeof(AsyncFromSqlSprocQueryTestBase<>),
            typeof(FromSqlSprocQueryTestBase<>),
        };

        protected override Assembly TargetAssembly { get; } = typeof(SqlServerComplianceTest).Assembly;
    }
}
