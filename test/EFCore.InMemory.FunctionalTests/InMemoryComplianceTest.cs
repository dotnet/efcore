// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryComplianceTest : ComplianceTestBase
    {
        protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
        {
            typeof(FunkyDataQueryTestBase<>),
            typeof(OptimisticConcurrencyTestBase<>),
            typeof(StoreGeneratedTestBase<>),
            typeof(LoadTestBase<>),                        // issue #15318
            typeof(MusicStoreTestBase<>),                  // issue #15318
            typeof(ConferencePlannerTestBase<>),           // issue #15318
            typeof(GraphUpdatesTestBase<>),                // issue #15318
            typeof(ProxyGraphUpdatesTestBase<>),           // issue #15318
            typeof(ComplexNavigationsWeakQueryTestBase<>), // issue #15285
            typeof(FiltersInheritanceTestBase<>),          // issue #15264
            typeof(FiltersTestBase<>),                     // issue #15264
            typeof(OwnedQueryTestBase<>),                  // issue #15285
            // Query pipeline
            typeof(SimpleQueryTestBase<>),
            typeof(GroupByQueryTestBase<>),
            typeof(ConcurrencyDetectorTestBase<>),
            typeof(AsNoTrackingTestBase<>),
            typeof(AsTrackingTestBase<>),
            typeof(CompiledQueryTestBase<>),
            typeof(ComplexNavigationsQueryTestBase<>),
            typeof(GearsOfWarQueryTestBase<>),
            typeof(IncludeAsyncTestBase<>),
            typeof(IncludeOneToOneTestBase<>),
            typeof(IncludeTestBase<>),
            typeof(InheritanceRelationshipsQueryTestBase<>),
            typeof(InheritanceTestBase<>),
            typeof(NullKeysTestBase<>),
            typeof(QueryNavigationsTestBase<>),
            typeof(QueryTaggingTestBase<>),
            typeof(SpatialQueryTestBase<>),
            typeof(UpdatesTestBase<>),
            typeof(FindTestBase<>),
            typeof(NotificationEntitiesTestBase<>),
            typeof(PropertyValuesTestBase<>),
        };

        protected override Assembly TargetAssembly { get; } = typeof(InMemoryComplianceTest).Assembly;
    }
}
