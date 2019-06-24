// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore
{
    public class SqliteComplianceTest : RelationalComplianceTestBase
    {
        protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
        {
            typeof(AsyncFromSqlSprocQueryTestBase<>),
            typeof(FromSqlSprocQueryTestBase<>),
            typeof(SqlExecutorTestBase<>),
            typeof(UdfDbFunctionTestBase<>),
            typeof(LoadTestBase<>),                        // issue #15318
            typeof(GraphUpdatesTestBase<>),                // issue #15318
            typeof(ProxyGraphUpdatesTestBase<>),           // issue #15318
            typeof(ComplexNavigationsWeakQueryTestBase<>), // issue #15285
            typeof(OwnedQueryTestBase<>),                  // issue #15285
            typeof(RelationalOwnedQueryTestBase<>),        // issue #15285
            // Query pipeline
            typeof(ConcurrencyDetectorTestBase<>),
            typeof(CompiledQueryTestBase<>),
            typeof(InheritanceRelationshipsQueryTestBase<>),
            typeof(QueryNavigationsTestBase<>),
            typeof(ConcurrencyDetectorRelationalTestBase<>),
            typeof(QueryTaggingTestBase<>),
            typeof(GearsOfWarFromSqlQueryTestBase<>),
            typeof(QueryNoClientEvalTestBase<>),
            typeof(WarningsTestBase<>),
        };

        protected override Assembly TargetAssembly { get; } = typeof(SqliteComplianceTest).Assembly;
    }
}
