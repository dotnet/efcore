// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class BaseTypeDiscoveryConventionTest
    {
        [Fact]
        public void Discovers_parent_type()
        {
            var entityBuilderA = CreateInternalEntityTypeBuilder<A>();
            var entityBuilderB = entityBuilderA.ModelBuilder.Entity(typeof(B), ConfigurationSource.Explicit);
            var entityBuilderC = entityBuilderA.ModelBuilder.Entity(typeof(C), ConfigurationSource.Explicit);
            Assert.Null(entityBuilderC.Metadata.BaseType);

            new BaseTypeDiscoveryConvention().Apply(entityBuilderC);

            Assert.Same(entityBuilderB.Metadata, entityBuilderC.Metadata.BaseType);
        }

        [Fact]
        public void Discovers_grandparent_type()
        {
            var entityBuilderA = CreateInternalEntityTypeBuilder<A>();
            var entityBuilderC = entityBuilderA.ModelBuilder.Entity(typeof(C), ConfigurationSource.Explicit);
            Assert.Null(entityBuilderC.Metadata.BaseType);

            new BaseTypeDiscoveryConvention().Apply(entityBuilderC);

            Assert.Same(entityBuilderA.Metadata, entityBuilderC.Metadata.BaseType);
        }

        [Fact]
        public void Discovers_parent_type_if_base_type_set()
        {
            var entityBuilderA = CreateInternalEntityTypeBuilder<A>();
            var entityBuilderB = entityBuilderA.ModelBuilder.Entity(typeof(B), ConfigurationSource.Explicit);
            var entityBuilderC = entityBuilderA.ModelBuilder.Entity(typeof(C), ConfigurationSource.Explicit);
            entityBuilderC.HasBaseType(entityBuilderA.Metadata, ConfigurationSource.Convention);

            new BaseTypeDiscoveryConvention().Apply(entityBuilderC);

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
