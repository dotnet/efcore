// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.ValueGeneration.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ValueGeneration
{
    public class ValueGeneratorExtensionsTest
    {
        [Fact]
        public void Sentinel_value_can_be_skipped_by_value_generator()
        {
            var property = GetEntityType().GetProperty("Id");

            var valueGenerator = new TemporaryNumberValueGeneratorFactory().Create(property);

            Assert.Equal(
                new[] { -2, -3, -4 },
                new[]
                {
                    (int)valueGenerator.NextSkippingSentinel(property),
                    (int)valueGenerator.NextSkippingSentinel(property),
                    (int)valueGenerator.NextSkippingSentinel(property)
                });
        }

        [Fact]
        public void Sentinel_value_on_nullable_property_can_be_skipped_by_value_generator()
        {
            var property = GetEntityType().GetProperty("NullableInt");

            var valueGenerator = new TemporaryNumberValueGeneratorFactory().Create(property);

            Assert.Equal(
                new[] { -1, -3, -4 },
                new[]
                {
                    (int)valueGenerator.NextSkippingSentinel(property),
                    (int)valueGenerator.NextSkippingSentinel(property),
                    (int)valueGenerator.NextSkippingSentinel(property)
                });
        }

        private static IEntityType GetEntityType()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<AnEntity>()
                .Property(e => e.Id)
                .Metadata
                .SentinelValue = -1;

            builder.Entity<AnEntity>()
                .Property(e => e.NullableInt)
                .Metadata
                .SentinelValue = -2;

            return builder.Model.GetEntityType(typeof(AnEntity));
        }

        private class AnEntity
        {
            public int Id { get; set; }
            public int? NullableInt { get; set; }
        }
    }
}
