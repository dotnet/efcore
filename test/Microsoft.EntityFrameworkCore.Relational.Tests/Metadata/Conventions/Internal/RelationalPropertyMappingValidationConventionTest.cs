// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Tests.Metadata.Conventions.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Metadata.Conventions.Internal
{
    public class RelationalPropertyMappingValidationConventionTest : PropertyMappingValidationConventionTest
    {
        [Fact]
        public void Throws_when_added_property_is_not_mapped_to_store()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Property", typeof(long), ConfigurationSource.Convention);

            Assert.Equal(CoreStrings.PropertyNotMapped("Property", typeof(long).DisplayName(fullName: false), typeof(NonPrimitiveAsPropertyEntity).DisplayName(fullName: false)),
                Assert.Throws<InvalidOperationException>(() => CreateConvention().Apply(modelBuilder)).Message);
        }

        [Fact]
        public void Throws_when_added_property_is_not_mapped_to_store_even_if_configured_to_use_column_type()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Property", typeof(long), ConfigurationSource.Convention).Relational(ConfigurationSource.Convention).ColumnType("some_int_mapping");

            Assert.Equal(CoreStrings.PropertyNotMapped("Property", typeof(long).DisplayName(fullName: false), typeof(NonPrimitiveAsPropertyEntity).DisplayName(fullName: false)),
                Assert.Throws<InvalidOperationException>(() => CreateConvention().Apply(modelBuilder)).Message);
        }

        protected override PropertyMappingValidationConvention CreateConvention()
            => new RelationalPropertyMappingValidationConvention(new TestRelationalTypeMapper());
    }
}
