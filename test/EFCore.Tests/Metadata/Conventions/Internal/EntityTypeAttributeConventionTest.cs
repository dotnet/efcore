﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class EntityTypeAttributeConventionTest
    {
        #region NotMappedAttribute

        [Fact]
        public void NotMappedAttribute_overrides_configuration_from_convention_source()
        {
            var modelBuilder = new InternalModelBuilder(new Model());

            var entityBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Convention);

            new NotMappedEntityTypeAttributeConvention().Apply(entityBuilder);

            Assert.Equal(0, modelBuilder.Metadata.GetEntityTypes().Count());
        }

        [Fact]
        public void NotMappedAttribute_does_not_override_configuration_from_explicit_source()
        {
            var modelBuilder = new InternalModelBuilder(new Model());

            var entityBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Explicit);

            new NotMappedEntityTypeAttributeConvention().Apply(entityBuilder);

            Assert.Equal(1, modelBuilder.Metadata.GetEntityTypes().Count());
        }

        [Fact]
        public void NotMappedAttribute_ignores_entityTypes_with_conventional_builder()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<B>();

            Assert.Equal(1, modelBuilder.Model.GetEntityTypes().Count());
        }

        #endregion

        [NotMapped]
        private class A
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private class B
        {
            public int Id { get; set; }

            public virtual A NavToA { get; set; }
        }
    }
}
