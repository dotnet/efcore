// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class DerivedTypeDiscoveryConventionTest
{
    [ConditionalFact]
    public void Discovers_child_types()
    {
        var entityBuilderC = CreateInternalEntityTypeBuilder<C>();

        RunConvention(entityBuilderC);

        Assert.Null(entityBuilderC.Metadata.BaseType);

        var entityBuilderA = entityBuilderC.ModelBuilder.Entity(typeof(A), ConfigurationSource.Explicit);

        RunConvention(entityBuilderA);

        Assert.Same(entityBuilderA.Metadata, entityBuilderC.Metadata.BaseType);

        var entityBuilderB = entityBuilderC.ModelBuilder.Entity(typeof(B), ConfigurationSource.Explicit);
        Assert.Null(entityBuilderB.Metadata.BaseType);

        RunConvention(entityBuilderB);

        Assert.Same(entityBuilderA.Metadata, entityBuilderB.Metadata.BaseType);
        Assert.Same(entityBuilderB.Metadata, entityBuilderC.Metadata.BaseType);
    }

    [ConditionalFact]
    public void Discovers_child_type_when_grandchild_type_exists()
    {
        var entityBuilderB = CreateInternalEntityTypeBuilder<B>();

        RunConvention(entityBuilderB);

        var entityBuilderC = entityBuilderB.ModelBuilder.Entity(typeof(C), ConfigurationSource.Explicit);

        Assert.Null(entityBuilderC.Metadata.BaseType);

        RunConvention(entityBuilderC);

        Assert.Same(entityBuilderB.Metadata, entityBuilderC.Metadata.BaseType);

        var entityBuilderA = entityBuilderB.ModelBuilder.Entity(typeof(A), ConfigurationSource.Explicit);

        RunConvention(entityBuilderA);

        Assert.Same(entityBuilderA.Metadata, entityBuilderB.Metadata.BaseType);
        Assert.Same(entityBuilderB.Metadata, entityBuilderC.Metadata.BaseType);
    }

    [ConditionalFact]
    public void Discovers_child_type_if_base_type_set()
    {
        var entityBuilderA = CreateInternalEntityTypeBuilder<A>();
        var entityBuilderC = entityBuilderA.ModelBuilder.Entity(typeof(C), ConfigurationSource.Explicit);

        RunConvention(entityBuilderC);

        Assert.Same(entityBuilderA.Metadata, entityBuilderC.Metadata.BaseType);

        var entityBuilderB = entityBuilderA.ModelBuilder.Entity(typeof(B), ConfigurationSource.Explicit);

        Assert.Null(entityBuilderB.Metadata.BaseType);

        RunConvention(entityBuilderB);

        Assert.Same(entityBuilderA.Metadata, entityBuilderB.Metadata.BaseType);
        Assert.Same(entityBuilderB.Metadata, entityBuilderC.Metadata.BaseType);
    }

    private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder)
    {
        var context = new ConventionContext<IConventionEntityTypeBuilder>(entityTypeBuilder.Metadata.Model.ConventionDispatcher);

        new BaseTypeDiscoveryConvention(CreateDependencies())
            .ProcessEntityTypeAdded(entityTypeBuilder, context);
    }

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    private class A;

    private class B : A;

    private class C : B;

    private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
    {
        var modelBuilder = new InternalModelBuilder(new Model());

        return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
    }
}
