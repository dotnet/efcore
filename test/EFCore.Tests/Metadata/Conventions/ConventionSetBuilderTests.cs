// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.InMemory.Metadata.Conventions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class ConventionSetBuilderTests
{
    [ConditionalFact]
    public virtual IReadOnlyModel Can_build_a_model_with_default_conventions_without_DI()
    {
        var modelBuilder = new ModelBuilder(GetConventionSet());
        modelBuilder.Entity<Product>();

        var model = modelBuilder.Model;
        Assert.NotNull(model.GetEntityTypes().Single());

        return model;
    }

    [ConditionalFact]
    public virtual IReadOnlyModel Can_build_a_model_with_default_conventions_without_DI_new()
    {
        var modelBuilder = GetModelBuilder();
        modelBuilder.Entity<Product>();

        var model = modelBuilder.Model;
        Assert.NotNull(model.GetEntityTypes().Single());

        return model;
    }

    [ConditionalFact]
    public virtual void Can_add_remove_and_replace_conventions()
    {
        var conventionSet = GetConventionSet();

        Assert.DoesNotContain(conventionSet.ModelFinalizingConventions, c => c is TestConvention);
        
        conventionSet.Add(new TestConvention());

        Assert.Contains(conventionSet.ModelFinalizingConventions, c => c is TestConvention);
        Assert.DoesNotContain(conventionSet.ModelInitializedConventions, c => c is DerivedTestConvention);

        conventionSet.Replace<TestConvention>(new DerivedTestConvention());
        
        Assert.Contains(conventionSet.ModelFinalizingConventions, c => c is DerivedTestConvention);
        Assert.Contains(conventionSet.ModelInitializedConventions, c => c is DerivedTestConvention);

        conventionSet.Remove(typeof(TestConvention));
        
        Assert.DoesNotContain(conventionSet.ModelFinalizingConventions, c => c is TestConvention);
    }

    protected class TestConvention : IModelFinalizingConvention
    {
        public void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
        }
    }
    
    protected class DerivedTestConvention : TestConvention, IModelInitializedConvention
    {
        public void ProcessModelInitialized(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
        }
    }

    protected virtual ConventionSet GetConventionSet()
        => InMemoryConventionSetBuilder.Build();

    protected virtual ModelBuilder GetModelBuilder()
        => InMemoryConventionSetBuilder.CreateModelBuilder();

    [Table("ProductTable")]
    protected class Product
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
    }
}
