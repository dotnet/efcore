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
