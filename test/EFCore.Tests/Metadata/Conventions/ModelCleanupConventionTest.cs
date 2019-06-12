// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class ModelCleanupConventionTest
    {
        [ConditionalFact]
        public void Unreachable_entity_types_are_removed()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();
            var modelBuilder = principalEntityBuilder.ModelBuilder;
            var dependentEntityBuilder = modelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.Convention);
            var baseEntityBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Convention);
            principalEntityBuilder.HasBaseType(baseEntityBuilder.Metadata, ConfigurationSource.Convention);
            dependentEntityBuilder.HasBaseType(baseEntityBuilder.Metadata, ConfigurationSource.Convention);
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, nameof(OneToOneDependent.OneToOnePrincipal), null, ConfigurationSource.Convention);

            RunConvention(modelBuilder);

            Assert.Equal(nameof(OneToOnePrincipal), modelBuilder.Metadata.GetEntityTypes().Single().DisplayName());
        }

        [ConditionalFact]
        public void Reachable_entity_types_are_not_removed()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();
            var modelBuilder = principalEntityBuilder.ModelBuilder;
            var dependentEntityBuilder = modelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.Convention);
            dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, null, nameof(OneToOnePrincipal.OneToOneDependent), ConfigurationSource.Convention);

            RunConvention(modelBuilder);

            Assert.Equal(2, modelBuilder.Metadata.GetEntityTypes().Count());
        }

        [ConditionalFact]
        public void Navigationless_foreign_keys_are_removed()
        {
            var principalEntityBuilder = CreateInternalEntityBuilder<OneToOnePrincipal>();
            var modelBuilder = principalEntityBuilder.ModelBuilder;
            var dependentEntityBuilder = modelBuilder.Entity(typeof(OneToOneDependent), ConfigurationSource.DataAnnotation);
            var baseEntityBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.DataAnnotation);
            principalEntityBuilder.HasBaseType(baseEntityBuilder.Metadata, ConfigurationSource.Convention);
            dependentEntityBuilder.HasBaseType(baseEntityBuilder.Metadata, ConfigurationSource.Convention);

            dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention);
            dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention);
            principalEntityBuilder.HasRelationship(dependentEntityBuilder.Metadata, ConfigurationSource.Convention);
            principalEntityBuilder.HasRelationship(dependentEntityBuilder.Metadata, ConfigurationSource.Convention);
            baseEntityBuilder.HasRelationship(baseEntityBuilder.Metadata, ConfigurationSource.Convention);
            baseEntityBuilder.HasRelationship(baseEntityBuilder.Metadata, ConfigurationSource.Convention);

            RunConvention(modelBuilder);

            Assert.True(modelBuilder.Metadata.GetEntityTypes().All(e => !e.GetDeclaredForeignKeys().Any()));
        }

        private void RunConvention(InternalModelBuilder modelBuilder)
        {
            var context = new ConventionContext<IConventionModelBuilder>(modelBuilder.Metadata.ConventionDispatcher);

            new ModelCleanupConvention(CreateDependencies())
                .ProcessModelFinalized(modelBuilder, context);
        }

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

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
