// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.Query.Relationships;
using Microsoft.EntityFrameworkCore.Query.Relationships.ComplexProperties;
using Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;
using Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

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

        // Relationships tests - not implemented for InMemory
        typeof(RelationshipsProjectionTestBase<>),
        typeof(RelationshipsCollectionTestBase<>),
        typeof(RelationshipsMiscellaneousTestBase<>),
        typeof(RelationshipsStructuralEqualityTestBase<>),
        typeof(RelationshipsSetOperationsTestBase<>),
        typeof(NavigationsIncludeTestBase<>),
        typeof(NavigationsProjectionTestBase<>),
        typeof(NavigationsCollectionTestBase<>),
        typeof(NavigationsMiscellaneousTestBase<>),
        typeof(NavigationsStructuralEqualityTestBase<>),
        typeof(NavigationsSetOperationsTestBase<>),
        typeof(OwnedNavigationsProjectionTestBase<>),
        typeof(OwnedNavigationsCollectionTestBase<>),
        typeof(OwnedNavigationsMiscellaneousTestBase<>),
        typeof(OwnedNavigationsStructuralEqualityTestBase<>),
        typeof(OwnedNavigationsSetOperationsTestBase<>),
        typeof(ComplexPropertiesProjectionTestBase<>),
        typeof(ComplexPropertiesCollectionTestBase<>),
        typeof(ComplexPropertiesMiscellaneousTestBase<>),
        typeof(ComplexPropertiesStructuralEqualityTestBase<>),
        typeof(ComplexPropertiesSetOperationsTestBase<>)
    };

    protected override Assembly TargetAssembly { get; } = typeof(InMemoryComplianceTest).Assembly;
}
