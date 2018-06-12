// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SequenceTest
    {
        [Fact]
        public void Can_be_created_with_default_values()
        {
            var sequence = new Model().Relational().GetOrAddSequence("Foo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_be_created_with_specified_values()
        {
            var sequence = new Model().Relational().GetOrAddSequence("Foo", "Smoo");
            sequence.StartValue = 1729;
            sequence.IncrementBy = 11;
            sequence.MinValue = 2001;
            sequence.MaxValue = 2010;
            sequence.ClrType = typeof(int);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);
        }

        [Fact]
        public void Can_only_be_created_for_byte_short_int_and_long_decimal()
        {
            var sequence = new Model().Relational().GetOrAddSequence("Foo");
            sequence.ClrType = typeof(byte);
            Assert.Same(typeof(byte), sequence.ClrType);
            sequence.ClrType = typeof(short);
            Assert.Same(typeof(short), sequence.ClrType);
            sequence.ClrType = typeof(int);
            Assert.Same(typeof(int), sequence.ClrType);
            sequence.ClrType = typeof(long);
            Assert.Same(typeof(long), sequence.ClrType);
            sequence.ClrType = typeof(decimal);
            Assert.Same(typeof(decimal), sequence.ClrType);

            Assert.Equal(
                RelationalStrings.BadSequenceType,
                Assert.Throws<ArgumentException>(
                    () => sequence.ClrType = typeof(bool)).Message);
        }

        [Fact]
        public void Can_get_model()
        {
            var model = new Model();

            var sequence = model.Relational().GetOrAddSequence("Foo");

            Assert.Same(model, sequence.Model);
        }

        [Fact]
        public void Can_get_model_default_schema_if_sequence_schema_not_specified()
        {
            var model = new Model();

            var sequence = model.Relational().GetOrAddSequence("Foo");

            Assert.Null(sequence.Schema);

            model.Relational().DefaultSchema = "db0";

            Assert.Equal("db0", sequence.Schema);
        }

        [Fact]
        public void Can_get_sequence_schema_if_specified_explicitly()
        {
            var model = new Model();

            model.Relational().DefaultSchema = "db0";
            var sequence = model.Relational().GetOrAddSequence("Foo", "db1");

            Assert.Equal("db1", sequence.Schema);
        }

        [Fact]
        public void Can_serialize_and_deserialize()
        {
            var model = new Model();
            var sequence = model.Relational().GetOrAddSequence("Foo", "Smoo");
            sequence.StartValue = 1729;
            sequence.IncrementBy = 11;
            sequence.MinValue = 2001;
            sequence.MaxValue = 2010;
            sequence.ClrType = typeof(int);

            model.Relational().GetOrAddSequence("Foo", "Smoo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);
        }

        [Fact]
        public void Can_serialize_and_deserialize_with_defaults()
        {
            var model = new Model();
            model.Relational().GetOrAddSequence("Foo");

            var sequence = model.Relational().GetOrAddSequence("Foo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_serialize_and_deserialize_with_funky_names()
        {
            var model = new Model();
            var sequence = model.Relational().GetOrAddSequence("'Foo'", "''S'''m'oo'''");
            sequence.StartValue = 1729;
            sequence.IncrementBy = 11;
            sequence.ClrType = typeof(int);

            sequence = model.Relational().GetOrAddSequence("'Foo'", "''S'''m'oo'''");

            Assert.Equal("'Foo'", sequence.Name);
            Assert.Equal("''S'''m'oo'''", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);
        }

        [Fact]
        public void Throws_on_bad_serialized_form()
        {
            var model = new Model();
            var sequence = model.Relational().GetOrAddSequence("Foo", "Smoo");
            sequence.StartValue = 1729;
            sequence.IncrementBy = 11;
            sequence.MinValue = 2001;
            sequence.MaxValue = 2010;
            sequence.ClrType = typeof(int);

            var annotationName = RelationalAnnotationNames.SequencePrefix + "Smoo.Foo";

            model[annotationName] = ((string)model[annotationName]).Replace("1", "Z");

            Assert.Equal(
                RelationalStrings.BadSequenceString,
                Assert.Throws<ArgumentException>(
                    () => model.Relational().GetOrAddSequence("Foo", "Smoo").ClrType).Message);
        }
    }
}
