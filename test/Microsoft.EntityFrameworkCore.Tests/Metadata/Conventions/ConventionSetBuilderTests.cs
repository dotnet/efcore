// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Conventions
{
    public class ConventionSetBuilderTests
    {
        [Fact]
        public virtual IModel Can_build_a_model_with_default_conventions_without_DI()
        {
            var modelBuilder = new ModelBuilder(GetConventionSet());
            modelBuilder.Entity<Product>();

            var model = modelBuilder.Model;
            Assert.Equal(2, model.GetEntityTypes().Single().GetProperties().Count());
            return model;
        }

        protected virtual ConventionSet GetConventionSet() => new CoreConventionSetBuilder().CreateConventionSet();

        [Table("ProductTable")]
        protected class Product
        {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
        }
    }
}
