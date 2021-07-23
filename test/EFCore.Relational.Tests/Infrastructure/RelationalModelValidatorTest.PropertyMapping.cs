// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public partial class RelationalModelValidatorTest
    {
        [ConditionalFact]
        public void Throws_when_added_property_is_not_mapped_to_store()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity));
            entityTypeBuilder.Property(typeof(Tuple<long>), "LongProperty");
            entityTypeBuilder.Ignore(nameof(NonPrimitiveAsPropertyEntity.Property));

            Assert.Equal(
                CoreStrings.PropertyNotMapped(
                    typeof(NonPrimitiveAsPropertyEntity).ShortDisplayName(), "LongProperty", typeof(Tuple<long>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
        }

        [ConditionalFact]
        public void Throws_when_added_property_is_not_mapped_to_store_even_if_configured_to_use_column_type()
        {
            var modelBuilder = CreateConventionlessModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveNonNavigationAsPropertyEntity));
            entityTypeBuilder.Property(typeof(Tuple<long>), "LongProperty")
                .HasColumnType("some_int_mapping");

            Assert.Equal(
                CoreStrings.PropertyNotMapped(
                    typeof(NonPrimitiveNonNavigationAsPropertyEntity).ShortDisplayName(), "LongProperty",
                    typeof(Tuple<long>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
        }
    }
}
