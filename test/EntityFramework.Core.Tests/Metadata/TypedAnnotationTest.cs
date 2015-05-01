// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class TypedAnnotationTest
    {
        [Fact]
        public void Can_covert_to_and_from_string()
        {
            var annotation = new TypedAnnotation("Forty Two");

            Assert.Equal("Forty Two", annotation.Value);
            Assert.Equal("Forty Two", new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_int()
        {
            var annotation = new TypedAnnotation(42);

            Assert.Equal(42, annotation.Value);
            Assert.Equal(42, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_long()
        {
            var annotation = new TypedAnnotation(42L);

            Assert.Equal(42L, annotation.Value);
            Assert.Equal(42L, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_short()
        {
            var annotation = new TypedAnnotation((short)42);

            Assert.Equal((short)42, annotation.Value);
            Assert.Equal((short)42, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_byte()
        {
            var annotation = new TypedAnnotation((byte)42);

            Assert.Equal((byte)42, annotation.Value);
            Assert.Equal((byte)42, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_decimal()
        {
            var annotation = new TypedAnnotation((decimal)42);

            Assert.Equal((decimal)42, annotation.Value);
            Assert.Equal((decimal)42, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_float()
        {
            var annotation = new TypedAnnotation((float)42);

            Assert.Equal((float)42, annotation.Value);
            Assert.Equal((float)42, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_double()
        {
            var annotation = new TypedAnnotation((double)42);

            Assert.Equal((double)42, annotation.Value);
            Assert.Equal((double)42, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_bool()
        {
            var annotation = new TypedAnnotation(true);

            Assert.Equal(true, annotation.Value);
            Assert.Equal(true, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_DateTime()
        {
            var annotation = new TypedAnnotation(new DateTime(1973, 9, 3, 0, 10, 1, 333));

            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 1, 333), annotation.Value);
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 1, 333), new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_char()
        {
            var annotation = new TypedAnnotation(' ');

            Assert.Equal(' ', annotation.Value);
            Assert.Equal(' ', new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_uint()
        {
            var annotation = new TypedAnnotation((uint)42);

            Assert.Equal((uint)42, annotation.Value);
            Assert.Equal((uint)42, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_ulong()
        {
            var annotation = new TypedAnnotation((ulong)42);

            Assert.Equal((ulong)42, annotation.Value);
            Assert.Equal((ulong)42, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_ushort()
        {
            var annotation = new TypedAnnotation((ushort)42);

            Assert.Equal((ushort)42, annotation.Value);
            Assert.Equal((ushort)42, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_sbyte()
        {
            var annotation = new TypedAnnotation((sbyte)42);

            Assert.Equal((sbyte)42, annotation.Value);
            Assert.Equal((sbyte)42, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_Guid()
        {
            var guid = new Guid("{6569CBEA-1E24-4D3E-A60E-6B663B4831F8}");
            var annotation = new TypedAnnotation(guid);

            Assert.Equal(guid, annotation.Value);
            Assert.Equal(guid, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_DateTimeOffset()
        {
            var dateTimeOffset = new DateTimeOffset(new DateTime(1973, 9, 3, 0, 10, 1, 333), new TimeSpan(-8, 0, 0));
            var annotation = new TypedAnnotation(dateTimeOffset);

            Assert.Equal(dateTimeOffset, annotation.Value);
            Assert.Equal(dateTimeOffset, new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_TimeSpan()
        {
            var annotation = new TypedAnnotation(new TimeSpan(-8, 1, 3));

            Assert.Equal(new TimeSpan(-8, 1, 3), annotation.Value);
            Assert.Equal(new TimeSpan(-8, 1, 3), new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Can_covert_to_and_from_byte_array()
        {
            var annotation = new TypedAnnotation(new Byte[] { 69, 70, 32, 82, 79, 67, 75, 83 });

            Assert.Equal(new Byte[] { 69, 70, 32, 82, 79, 67, 75, 83 }, annotation.Value);
            Assert.Equal(
                new Byte[] { 69, 70, 32, 82, 79, 67, 75, 83 },
                new TypedAnnotation(annotation.TypeString, annotation.ValueString).Value);
        }

        [Fact]
        public void Throws_for_unsupported_types()
        {
            Assert.Equal(
                Strings.UnsupportedAnnotationType("Random"),
                Assert.Throws<NotSupportedException>(() => new TypedAnnotation(new Random())).Message);

            Assert.Equal(
                Strings.UnsupportedAnnotationType("Random"),
                Assert.Throws<NotSupportedException>(() => new TypedAnnotation(typeof(Random).FullName, "Rand!")).Message);
        }
    }
}
