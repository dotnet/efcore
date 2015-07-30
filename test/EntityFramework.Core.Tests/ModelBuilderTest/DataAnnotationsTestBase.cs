// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Tests
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class DataAnnotationsTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void NotMappedAttribute_removes_ambiguity_in_conventional_relationship_building()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Book>();

                Assert.Contains("Details", model.GetEntityType(typeof(Book)).Navigations.Select(nav => nav.Name));
                Assert.Contains("AnotherBook", model.GetEntityType(typeof(BookDetails)).Navigations.Select(nav => nav.Name));
                Assert.DoesNotContain("Book", model.GetEntityType(typeof(BookDetails)).Navigations.Select(nav => nav.Name));
            }

            [Fact]
            public virtual void InversePropertyAttribute_removes_ambiguity_in_conventional_relationalship_building()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Book>();

                Assert.Contains("Book", model.GetEntityType(typeof(BookLabel)).Navigations.Select(nav => nav.Name));
                Assert.Equal("Label", model.GetEntityType(typeof(BookLabel)).FindNavigation("Book").ForeignKey.PrincipalToDependent.Name);

                Assert.Contains("AlternateLabel", model.GetEntityType(typeof(Book)).Navigations.Select(nav => nav.Name));
                Assert.Null(model.GetEntityType(typeof(Book)).FindNavigation("AlternateLabel").ForeignKey.PrincipalToDependent);
            }
        }
    }
}
