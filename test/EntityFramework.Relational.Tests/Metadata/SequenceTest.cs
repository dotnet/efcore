// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Tests
{
    public class SequenceTest
    {
        [Fact]
        public void Can_be_created_with_default_values()
        {
            var sequence = new Sequence("Foo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.Type);
        }

        [Fact]
        public void Can_be_created_with_specified_values()
        {
            var sequence = new Sequence("Foo", "Smoo", 1729, 11, 2001, 2010, typeof(int));

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_only_be_created_for_byte_short_int_and_long()
        {
            Assert.Same(typeof(byte), new Sequence("Foo", null, 11, 1729, null, null, typeof(byte)).Type);
            Assert.Same(typeof(short), new Sequence("Foo", null, 11, 1729, null, null, typeof(short)).Type);
            Assert.Same(typeof(int), new Sequence("Foo", null, 11, 1729, null, null, typeof(int)).Type);
            Assert.Same(typeof(long), new Sequence("Foo", null, 11, 1729, null, null, typeof(long)).Type);

            Assert.Equal(
                Strings.BadSequenceType,
                Assert.Throws<ArgumentException>(() => new Sequence("Foo", null, 11, 1729, null, null, typeof(decimal))).Message);
        }

        [Fact]
        public void Can_set_model()
        {
            var sequence = new Sequence("Foo");

            var model = new Model();
            sequence.Model = model;
            Assert.Same(model, sequence.Model);
        }

        [Fact]
        public void Can_serialize_and_deserialize()
        {
            var sequence = Sequence.Deserialize(new Sequence("Foo", "Smoo", 1729, 11, 2001, 2010, typeof(int)).Serialize());

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_serialize_and_deserialize_with_defaults()
        {
            var serialize = new Sequence("Foo").Serialize();
            var sequence = Sequence.Deserialize(serialize);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.Type);
        }

        [Fact]
        public void Can_serialize_and_deserialize_with_funky_names()
        {
            var sequence = Sequence.Deserialize(new Sequence("'Foo'", "''S'''m'oo'''", 1729, 11, null, null, typeof(int)).Serialize());

            Assert.Equal("'Foo'", sequence.Name);
            Assert.Equal("''S'''m'oo'''", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Throws_on_bad_serialized_form()
        {
            var badString = new Sequence("Foo", "Smoo", 1729, 11, 2001, 2010, typeof(int)).Serialize().Replace("1", "Z");

            Assert.Equal(
                Strings.BadSequenceString,
                Assert.Throws<ArgumentException>(() => Sequence.Deserialize(badString)).Message);
        }
    }
}
