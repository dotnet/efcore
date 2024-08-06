// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.BulkUpdates;

namespace Microsoft.EntityFrameworkCore;

public class InMemoryComplianceTest : ComplianceTestBase
{
    protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
    {
        // No in-memory tests
        typeof(ComplexTypeQueryTestBase<>),
        typeof(AdHocComplexTypeQueryTestBase),
        typeof(PrimitiveCollectionsQueryTestBase<>),
        typeof(NonSharedPrimitiveCollectionsQueryTestBase),
        typeof(FunkyDataQueryTestBase<>),
        typeof(StoreGeneratedTestBase<>),
        typeof(ConferencePlannerTestBase<>),
        typeof(ManyToManyQueryTestBase<>),
        typeof(ComplexTypeBulkUpdatesTestBase<>),
        typeof(BulkUpdatesTestBase<>),
        typeof(FiltersInheritanceBulkUpdatesTestBase<>),
        typeof(InheritanceBulkUpdatesTestBase<>),
        typeof(NonSharedModelBulkUpdatesTestBase),
        typeof(NorthwindBulkUpdatesTestBase<>),
        typeof(JsonQueryTestBase<>),
    };

    protected override Assembly TargetAssembly { get; } = typeof(InMemoryComplianceTest).Assembly;
}
