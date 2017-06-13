// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class ModelCleanupConventionTest
    {
        [Fact]
        public void Unreachable_entity_types_are_removed()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();
            var modelBuilder = principalEntityBuilder.ModelBuilder;
            var dependentEntityBuilder = modelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.Convention);
            var baseEntityBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Convention);
            principalEntityBuilder.HasBaseType(baseEntityBuilder.Metadata, ConfigurationSource.Convention);
            dependentEntityBuilder.HasBaseType(baseEntityBuilder.Metadata, ConfigurationSource.Convention);
            dependentEntityBuilder.Relationship(
                principalEntityBuilder, nameof(OneToOneDependent.OneToOnePrincipal), null, ConfigurationSource.Convention);

            new ModelCleanupConvention().Apply(modelBuilder);

            Assert.Equal(nameof(OneToOnePrincipal), modelBuilder.Metadata.GetEntityTypes().Single().DisplayName());
        }

        [Fact]
        public void Reachable_entity_types_are_not_removed()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();
            var modelBuilder = principalEntityBuilder.ModelBuilder;
            var dependentEntityBuilder = modelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.Convention);
            dependentEntityBuilder.Relationship(
                principalEntityBuilder, null, nameof(OneToOnePrincipal.OneToOneDependent), ConfigurationSource.Convention);

            new ModelCleanupConvention().Apply(modelBuilder);

            Assert.Equal(2, modelBuilder.Metadata.GetEntityTypes().Count());
        }

        [Fact]
        public void Navigationless_foreign_keys_are_removed()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();
            var modelBuilder = principalEntityBuilder.ModelBuilder;
            var dependentEntityBuilder = modelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.DataAnnotation);
            var baseEntityBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.DataAnnotation);
            principalEntityBuilder.HasBaseType(baseEntityBuilder.Metadata, ConfigurationSource.Convention);
            dependentEntityBuilder.HasBaseType(baseEntityBuilder.Metadata, ConfigurationSource.Convention);

            dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Convention);
            dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Convention);
            principalEntityBuilder.Relationship(dependentEntityBuilder, ConfigurationSource.Convention);
            principalEntityBuilder.Relationship(dependentEntityBuilder, ConfigurationSource.Convention);
            baseEntityBuilder.Relationship(baseEntityBuilder, ConfigurationSource.Convention);
            baseEntityBuilder.Relationship(baseEntityBuilder, ConfigurationSource.Convention);

            new ModelCleanupConvention().Apply(modelBuilder);

            Assert.True(modelBuilder.Metadata.GetEntityTypes().All(e => !e.GetDeclaredForeignKeys().Any()));
        }

        private static InternalEntityTypeBuilder CreateInternalEntityBuilder<T>()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityBuilder = modelBuilder.Entity(typeof(T), ConfigurationSource.DataAnnotation);

            return entityBuilder;
        }

        private class Base
        {
            public int Id { get; set; }
        }

        private class OneToOnePrincipal : Base
        {
            public OneToOneDependent OneToOneDependent { get; set; }
        }

        private class OneToOneDependent : Base
        {
            public OneToOnePrincipal OneToOnePrincipal { get; set; }
        }
    }
}
