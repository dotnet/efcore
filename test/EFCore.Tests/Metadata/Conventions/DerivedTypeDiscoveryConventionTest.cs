// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class DerivedTypeDiscoveryConventionTest
    {
        [ConditionalFact]
        public void Discovers_child_types()
        {
            var entityBuilderA = CreateInternalEntityTypeBuilder<A>();
            var entityBuilderB = entityBuilderA.ModelBuilder.Entity(typeof(B), ConfigurationSource.Explicit);
            var entityBuilderC = entityBuilderA.ModelBuilder.Entity(typeof(C), ConfigurationSource.Explicit);
            Assert.Null(entityBuilderB.Metadata.BaseType);
            Assert.Null(entityBuilderC.Metadata.BaseType);

            RunConvention(entityBuilderA);

            Assert.Same(entityBuilderA.Metadata, entityBuilderB.Metadata.BaseType);
            Assert.Same(entityBuilderA.Metadata, entityBuilderC.Metadata.BaseType);

            RunConvention(entityBuilderB);

            Assert.Same(entityBuilderA.Metadata, entityBuilderB.Metadata.BaseType);
            Assert.Same(entityBuilderB.Metadata, entityBuilderC.Metadata.BaseType);
        }

        [ConditionalFact]
        public void Discovers_child_type_when_grandchild_type_exists()
        {
            var entityBuilderA = CreateInternalEntityTypeBuilder<A>();
            var entityBuilderB = entityBuilderA.ModelBuilder.Entity(typeof(B), ConfigurationSource.Explicit);
            var entityBuilderC = entityBuilderA.ModelBuilder.Entity(typeof(C), ConfigurationSource.Explicit);
            entityBuilderC.HasBaseType(entityBuilderB.Metadata, ConfigurationSource.DataAnnotation);

            RunConvention(entityBuilderA);

            Assert.Same(entityBuilderA.Metadata, entityBuilderB.Metadata.BaseType);
            Assert.Same(entityBuilderB.Metadata, entityBuilderC.Metadata.BaseType);
        }

        [ConditionalFact]
        public void Discovers_child_type_if_base_type_set()
        {
            var entityBuilderA = CreateInternalEntityTypeBuilder<A>();
            var entityBuilderB = entityBuilderA.ModelBuilder.Entity(typeof(B), ConfigurationSource.Explicit);
            entityBuilderB.HasBaseType(entityBuilderA.Metadata, ConfigurationSource.DataAnnotation);
            var entityBuilderC = entityBuilderA.ModelBuilder.Entity(typeof(C), ConfigurationSource.Explicit);
            entityBuilderC.HasBaseType(entityBuilderA.Metadata, ConfigurationSource.Convention);

            RunConvention(entityBuilderB);

            Assert.Same(entityBuilderB.Metadata, entityBuilderC.Metadata.BaseType);
        }

        private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var context = new ConventionContext<IConventionEntityTypeBuilder>(entityTypeBuilder.Metadata.Model.ConventionDispatcher);

            new DerivedTypeDiscoveryConvention(CreateDependencies())
                .ProcessEntityTypeAdded(entityTypeBuilder, context);
        }

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

        private class A
        {
        }

        private class B : A
        {
        }

        private class C : B
        {
        }

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var modelBuilder = new InternalModelBuilder(new Model());

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }
    }
}
