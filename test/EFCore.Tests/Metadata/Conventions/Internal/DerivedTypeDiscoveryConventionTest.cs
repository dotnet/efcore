// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class DerivedTypeDiscoveryConventionTest
    {
        [Fact]
        public void Discovers_child_types()
        {
            var entityBuilderA = CreateInternalEntityTypeBuilder<A>();
            var entityBuilderB = entityBuilderA.ModelBuilder.Entity(typeof(B), ConfigurationSource.Explicit);
            var entityBuilderC = entityBuilderA.ModelBuilder.Entity(typeof(C), ConfigurationSource.Explicit);
            Assert.Null(entityBuilderB.Metadata.BaseType);
            Assert.Null(entityBuilderC.Metadata.BaseType);

            new DerivedTypeDiscoveryConvention().Apply(entityBuilderA);

            Assert.Same(entityBuilderA.Metadata, entityBuilderB.Metadata.BaseType);
            Assert.Same(entityBuilderA.Metadata, entityBuilderC.Metadata.BaseType);

            new DerivedTypeDiscoveryConvention().Apply(entityBuilderB);

            Assert.Same(entityBuilderA.Metadata, entityBuilderB.Metadata.BaseType);
            Assert.Same(entityBuilderB.Metadata, entityBuilderC.Metadata.BaseType);
        }

        [Fact]
        public void Discovers_child_type_when_grandchild_type_exists()
        {
            var entityBuilderA = CreateInternalEntityTypeBuilder<A>();
            var entityBuilderB = entityBuilderA.ModelBuilder.Entity(typeof(B), ConfigurationSource.Explicit);
            var entityBuilderC = entityBuilderA.ModelBuilder.Entity(typeof(C), ConfigurationSource.Explicit);
            entityBuilderC.HasBaseType(entityBuilderB.Metadata, ConfigurationSource.DataAnnotation);

            new DerivedTypeDiscoveryConvention().Apply(entityBuilderA);

            Assert.Same(entityBuilderA.Metadata, entityBuilderB.Metadata.BaseType);
            Assert.Same(entityBuilderB.Metadata, entityBuilderC.Metadata.BaseType);
        }

        [Fact]
        public void Discovers_child_type_if_base_type_set()
        {
            var entityBuilderA = CreateInternalEntityTypeBuilder<A>();
            var entityBuilderB = entityBuilderA.ModelBuilder.Entity(typeof(B), ConfigurationSource.Explicit);
            entityBuilderB.HasBaseType(entityBuilderA.Metadata, ConfigurationSource.DataAnnotation);
            var entityBuilderC = entityBuilderA.ModelBuilder.Entity(typeof(C), ConfigurationSource.Explicit);
            entityBuilderC.HasBaseType(entityBuilderA.Metadata, ConfigurationSource.Convention);

            new DerivedTypeDiscoveryConvention().Apply(entityBuilderB);

            Assert.Same(entityBuilderB.Metadata, entityBuilderC.Metadata.BaseType);
        }

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
