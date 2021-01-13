// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.InMemory.Metadata.Conventions;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class ConventionSetBuilderTests
    {
        [ConditionalFact]
        public virtual IModel Can_build_a_model_with_default_conventions_without_DI()
        {
            var modelBuilder = new ModelBuilder(GetConventionSet());
            modelBuilder.Entity<Product>();

            var model = modelBuilder.Model;
            Assert.NotNull(model.GetEntityTypes().Single());

            return model;
        }

        [ConditionalFact]
        public virtual IModel Can_build_a_model_with_default_conventions_without_DI_new()
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
}
