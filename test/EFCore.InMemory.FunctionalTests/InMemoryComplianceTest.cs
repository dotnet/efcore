// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.Query.Associations;
using Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;
using Microsoft.EntityFrameworkCore.Query.Associations.Navigations;
using Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

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
        typeof(BulkUpdatesTestBase<>),
        typeof(FiltersInheritanceBulkUpdatesTestBase<>),
        typeof(InheritanceBulkUpdatesTestBase<>),
        typeof(NonSharedModelBulkUpdatesTestBase),
        typeof(NorthwindBulkUpdatesTestBase<>),
        typeof(JsonQueryTestBase<>),
        typeof(AdHocJsonQueryTestBase),
        typeof(TypeTestBase<,>),

        // Relationships tests - not implemented for InMemory
        typeof(AssociationsCollectionTestBase<>),
        typeof(AssociationsMiscellaneousTestBase<>),
        typeof(AssociationsProjectionTestBase<>),
        typeof(AssociationsSetOperationsTestBase<>),
        typeof(AssociationsStructuralEqualityTestBase<>),
        typeof(AssociationsBulkUpdateTestBase<>),
        typeof(ComplexPropertiesCollectionTestBase<>),
        typeof(ComplexPropertiesMiscellaneousTestBase<>),
        typeof(ComplexPropertiesProjectionTestBase<>),
        typeof(ComplexPropertiesSetOperationsTestBase<>),
        typeof(ComplexPropertiesStructuralEqualityTestBase<>),
        typeof(ComplexPropertiesBulkUpdateTestBase<>),
        typeof(NavigationsCollectionTestBase<>),
        typeof(NavigationsIncludeTestBase<>),
        typeof(NavigationsMiscellaneousTestBase<>),
        typeof(NavigationsProjectionTestBase<>),
        typeof(NavigationsSetOperationsTestBase<>),
        typeof(NavigationsStructuralEqualityTestBase<>),
        typeof(OwnedNavigationsCollectionTestBase<>),
        typeof(OwnedNavigationsMiscellaneousTestBase<>),
        typeof(OwnedNavigationsProjectionTestBase<>),
        typeof(OwnedNavigationsSetOperationsTestBase<>),
        typeof(OwnedNavigationsStructuralEqualityTestBase<>)
    };

    protected override Assembly TargetAssembly { get; } = typeof(InMemoryComplianceTest).Assembly;
}
