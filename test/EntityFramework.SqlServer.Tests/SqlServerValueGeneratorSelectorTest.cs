// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerValueGeneratorSelectorTest
    {
        [Fact]
        public void Returns_sequence_generator_when_explicitly_configured()
        {
            var sequenceFactory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>(),
                sequenceFactory,
                new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>());

            Assert.Same(sequenceFactory, selector.Select(CreateSequenceProperty<long>()));
            Assert.Same(sequenceFactory, selector.Select(CreateSequenceProperty<int>()));
            Assert.Same(sequenceFactory, selector.Select(CreateSequenceProperty<short>()));
            Assert.Same(sequenceFactory, selector.Select(CreateSequenceProperty<byte>()));
        }

        private static Property CreateSequenceProperty<T>()
        {
            var property = new BasicModelBuilder()
                .Entity<Robot>()
                .Property<T>(typeof(T).Name)
                .ForSqlServer(b => b.UseSequence())
                .Metadata;

            return property;
        }

        [Fact]
        public void Returns_sequence_generator_when_explicitly_configured_on_model()
        {
            var sequenceFactory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory()));

            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>(),
                sequenceFactory,
                new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>());

            Assert.Same(sequenceFactory, selector.Select(CreateModelSequenceProperty<long>()));
            Assert.Same(sequenceFactory, selector.Select(CreateModelSequenceProperty<int>()));
            Assert.Same(sequenceFactory, selector.Select(CreateModelSequenceProperty<short>()));
            Assert.Same(sequenceFactory, selector.Select(CreateModelSequenceProperty<byte>()));
        }

        private static Property CreateModelSequenceProperty<T>()
        {
            var property = new BasicModelBuilder()
                .ForSqlServer(b => b.UseSequence())
                .Entity<Robot>()
                .Property<T>(typeof(T).Name)
                .GenerateValuesOnAdd()
                .Metadata;

            return property;
        }

        [Fact]
        public void Returns_temp_generator_for_identity_generator_when_explicitly_configured()
        {
            var tempFactory = new SimpleValueGeneratorFactory<TemporaryValueGenerator>();

            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                tempFactory,
                new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory())),
                new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>());

            Assert.Same(tempFactory, selector.Select(CreateIdentityProperty<long>()));
            Assert.Same(tempFactory, selector.Select(CreateIdentityProperty<int>()));
            Assert.Same(tempFactory, selector.Select(CreateIdentityProperty<short>()));
        }

        private static Property CreateIdentityProperty<T>()
        {
            var property = new BasicModelBuilder()
                .Entity<Robot>()
                .Property<T>(typeof(T).Name)
                .ForSqlServer(b => b.UseIdentity())
                .Metadata;

            return property;
        }

        [Fact]
        public void Returns_temp_generator_for_identity_generator_when_explicitly_configured_on_model()
        {
            var tempFactory = new SimpleValueGeneratorFactory<TemporaryValueGenerator>();

            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                tempFactory,
                new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory())),
                new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>());

            Assert.Same(tempFactory, selector.Select(CreateModelIdentityProperty<long>()));
            Assert.Same(tempFactory, selector.Select(CreateModelIdentityProperty<int>()));
            Assert.Same(tempFactory, selector.Select(CreateModelIdentityProperty<short>()));
        }

        private static Property CreateModelIdentityProperty<T>()
        {
            var property = new BasicModelBuilder()
                .ForSqlServer(b => b.UseIdentity())
                .Entity<Robot>()
                .Property<T>(typeof(T).Name)
                .GenerateValuesOnAdd()
                .Metadata;

            return property;
        }

        [Fact] // TODO: This will change when sequence becomes the default
        public void Returns_in_temp_generator_for_all_integer_types_except_byte_setup_for_value_generation()
        {
            var tempFactory = new SimpleValueGeneratorFactory<TemporaryValueGenerator>();

            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                tempFactory,
                new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory())),
                new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>());

            Assert.Same(tempFactory, selector.Select(CreateDefaultValueGenProperty<long>()));
            Assert.Same(tempFactory, selector.Select(CreateDefaultValueGenProperty<int>()));
            Assert.Same(tempFactory, selector.Select(CreateDefaultValueGenProperty<short>()));
        }

        private static Property CreateDefaultValueGenProperty<T>()
        {
            var property = new BasicModelBuilder()
                .Entity<Robot>()
                .Property<T>(typeof(T).Name)
                .GenerateValuesOnAdd()
                .Metadata;

            return property;
        }

        [Fact]
        public void Returns_sequential_GUID_generator_for_GUID_types()
        {
            var sequentialGuidFactory = new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>();

            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>(),
                new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory())),
                sequentialGuidFactory);

            Assert.Same(sequentialGuidFactory, selector.Select(CreateDefaultValueGenProperty<Guid>()));
        }

        [Fact]
        public void Returns_null_when_no_value_generation_configured()
        {
            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>(),
                new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory())),
                new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>());

            var property = new BasicModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Int32)
                .Metadata;

            Assert.Null(selector.Select(property));

            property = new BasicModelBuilder()
                .Entity<Robot>()
                .Property(e => e.Int32)
                .StoreComputed()
                .Metadata;

            Assert.Null(selector.Select(property));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>(),
                new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor(new NullLoggerFactory())),
                new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>());

            var property = new BasicModelBuilder()
                .Entity<Robot>()
                .Property(e => e.String)
                .GenerateValuesOnAdd()
                .Metadata;

            Assert.Equal(
                TestHelpers.GetCoreString("FormatNoValueGenerator", "String", typeof(Robot).FullName, "String"),
                Assert.Throws<NotSupportedException>(() => selector.Select(property)).Message);
        }

        private class Robot
        {
            public long Int64 { get; set; }
            public int Int32 { get; set; }
            public short Int16 { get; set; }
            public byte Byte { get; set; }
            public string String { get; set; }
            public Guid Guid { get; set; }
        }
    }
}
