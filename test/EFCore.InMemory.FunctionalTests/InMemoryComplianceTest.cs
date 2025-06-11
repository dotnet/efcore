// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.Query.Relationships.Include;
using Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

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
        typeof(AdHocJsonQueryTestBase),

        // TODO: implement later once things are baked
        typeof(NavigationNoTrackingProjectionTestBase<>),
        typeof(NavigationProjectionTestBase<>),
        typeof(OwnedJsonNoTrackingProjectionTestBase<>),
        typeof(OwnedJsonProjectionTestBase<>),
        typeof(OwnedNoTrackingProjectionTestBase<>),
        typeof(OwnedProjectionTestBase<>),
        typeof(ProjectionTestBase<>),
        typeof(NavigationIncludeTestBase<>),

        typeof(ComplexNoTrackingProjectionTestBase<>),
        typeof(ComplexProjectionTestBase<>),
        typeof(NavigationReferenceNoTrackingProjectionTestBase<>),
        typeof(NavigationReferenceProjectionTestBase<>),
        typeof(OwnedJsonReferenceNoTrackingProjectionTestBase<>),
        typeof(OwnedJsonReferenceProjectionTestBase<>),
        typeof(OwnedReferenceNoTrackingProjectionTestBase<>),
        typeof(OwnedReferenceProjectionTestBase<>),
        typeof(ReferenceProjectionTestBase<>),
    };

    protected override Assembly TargetAssembly { get; } = typeof(InMemoryComplianceTest).Assembly;
}
