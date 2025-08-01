// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class ComplexPropertyTest
{
    [ConditionalFact]
    public void Can_get_TargetType()
    {
        var modelBuilder = CreateModelBuilder();
        var entityBuilder = modelBuilder.Entity<TestEntity>();
        
        var complexPropertyBuilder = entityBuilder.ComplexProperty(e => e.Address);
        var complexProperty = complexPropertyBuilder.Metadata;

        Assert.Same(complexProperty.ComplexType, ((IStructuralProperty)complexProperty).TargetType);
        Assert.Same(complexProperty.ComplexType, ((IReadOnlyStructuralProperty)complexProperty).TargetType);
        Assert.Same(complexProperty.ComplexType, ((IMutableStructuralProperty)complexProperty).TargetType);
        Assert.Same(complexProperty.ComplexType, ((IConventionStructuralProperty)complexProperty).TargetType);
    }

    private static ModelBuilder CreateModelBuilder()
        => new(new ConventionSet());

    private class TestEntity
    {
        public int Id { get; set; }
        public Address Address { get; set; } = null!;
    }

    private class Address
    {
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
    }
}
