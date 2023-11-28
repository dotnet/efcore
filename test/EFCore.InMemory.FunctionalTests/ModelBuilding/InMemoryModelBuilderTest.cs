// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class InMemoryModelBuilderTest : ModelBuilderTest
{
    public abstract class InMemoryNonRelationship : NonRelationshipTestBase, IClassFixture<InMemoryModelBuilderFixture>
    {
        public InMemoryNonRelationship(InMemoryModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class InMemoryComplexType : ComplexTypeTestBase, IClassFixture<InMemoryModelBuilderFixture>
    {
        public InMemoryComplexType(InMemoryModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class InMemoryInheritance : InheritanceTestBase, IClassFixture<InMemoryModelBuilderFixture>
    {
        public InMemoryInheritance(InMemoryModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class InMemoryOneToMany : OneToManyTestBase, IClassFixture<InMemoryModelBuilderFixture>
    {
        public InMemoryOneToMany(InMemoryModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class InMemoryManyToMany : ManyToManyTestBase, IClassFixture<InMemoryModelBuilderFixture>
    {
        public InMemoryManyToMany(InMemoryModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class InMemoryManyToOne : ManyToOneTestBase, IClassFixture<InMemoryModelBuilderFixture>
    {
        public InMemoryManyToOne(InMemoryModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class InMemoryOneToOne : OneToOneTestBase, IClassFixture<InMemoryModelBuilderFixture>
    {
        public InMemoryOneToOne(InMemoryModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class InMemoryOwnedTypes : OwnedTypesTestBase, IClassFixture<InMemoryModelBuilderFixture>
    {
        public InMemoryOwnedTypes(InMemoryModelBuilderFixture fixture)
            : base(fixture)
        {
        }
    }

    public class InMemoryModelBuilderFixture : ModelBuilderFixtureBase
    {
        public override TestHelpers TestHelpers => InMemoryTestHelpers.Instance;
    }
}
