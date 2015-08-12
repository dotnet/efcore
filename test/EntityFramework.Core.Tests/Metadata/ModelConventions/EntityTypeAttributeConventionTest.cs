// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Conventions
{
    public class EntityTypeAttributeConventionTest
    {
        #region NotMappedAttribute

        [Fact]
        public void NotMappedAttribute_overrides_configuration_from_convention_source()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());

            var entityBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Convention);

            new NotMappedEntityTypeAttributeConvention().Apply(entityBuilder);

            Assert.Equal(0, modelBuilder.Metadata.EntityTypes.Count);
        }

        [Fact]
        public void NotMappedAttribute_does_not_override_configuration_from_explicit_source()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());

            var entityBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Explicit);

            new NotMappedEntityTypeAttributeConvention().Apply(entityBuilder);

            Assert.Equal(1, modelBuilder.Metadata.EntityTypes.Count);
        }

        [Fact]
        public void NotMappedAttribute_ignores_entityTypes_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            modelBuilder.Entity<B>();

            Assert.Equal(1, modelBuilder.Model.EntityTypes.Count);
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
