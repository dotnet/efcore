// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Internal
{
    public class BoxedValueReaderSourceTest
    {
        [Fact]
        public void Returns_generic_value_reader_for_given_property_type()
            => Assert.IsType<GenericBoxedValueReader<int>>(new BoxedValueReaderSource().GetReader(GetEntityType().GetProperty("Id")));

        [Fact]
        public void Always_returns_non_nullable_value_reader_for_given_property_type()
            => Assert.IsType<GenericBoxedValueReader<int>>(new BoxedValueReaderSource().GetReader(GetEntityType().GetProperty("NullableInt")));

        [Fact]
        public void Returns_same_value_reader_for_same_property_type()
        {
            var entityType = GetEntityType();
            var property1 = entityType.GetProperty("Id");
            var property2 = entityType.GetProperty("NullableInt");

            var source = new BoxedValueReaderSource();
            Assert.Same(source.GetReader(property1), source.GetReader(property2));
        }

        private static IEntityType GetEntityType()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<ScissorSister>();

            return builder.Model.GetEntityType(typeof(ScissorSister));
        }

        private class ScissorSister
        {
            public int Id { get; set; }
            public int? NullableInt { get; set; }
        }
    }
}
