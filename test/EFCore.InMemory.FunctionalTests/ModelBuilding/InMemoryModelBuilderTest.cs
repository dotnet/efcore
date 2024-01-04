// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class InMemoryModelBuilderTest : ModelBuilderTest
{
    public abstract class InMemoryNonRelationship(InMemoryModelBuilderFixture fixture) : NonRelationshipTestBase(fixture), IClassFixture<InMemoryModelBuilderFixture>;

    public abstract class InMemoryComplexType(InMemoryModelBuilderFixture fixture) : ComplexTypeTestBase(fixture), IClassFixture<InMemoryModelBuilderFixture>;

    public abstract class InMemoryInheritance(InMemoryModelBuilderFixture fixture) : InheritanceTestBase(fixture), IClassFixture<InMemoryModelBuilderFixture>;

    public abstract class InMemoryOneToMany(InMemoryModelBuilderFixture fixture) : OneToManyTestBase(fixture), IClassFixture<InMemoryModelBuilderFixture>;

    public abstract class InMemoryManyToMany(InMemoryModelBuilderFixture fixture) : ManyToManyTestBase(fixture), IClassFixture<InMemoryModelBuilderFixture>;

    public abstract class InMemoryManyToOne(InMemoryModelBuilderFixture fixture) : ManyToOneTestBase(fixture), IClassFixture<InMemoryModelBuilderFixture>;

    public abstract class InMemoryOneToOne(InMemoryModelBuilderFixture fixture) : OneToOneTestBase(fixture), IClassFixture<InMemoryModelBuilderFixture>;

    public abstract class InMemoryOwnedTypes(InMemoryModelBuilderFixture fixture) : OwnedTypesTestBase(fixture), IClassFixture<InMemoryModelBuilderFixture>;

    public class InMemoryModelBuilderFixture : ModelBuilderFixtureBase
    {
        public override TestHelpers TestHelpers => InMemoryTestHelpers.Instance;
    }
}
