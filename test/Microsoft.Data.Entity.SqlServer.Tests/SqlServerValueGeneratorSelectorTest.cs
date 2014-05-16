// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerValueGeneratorSelectorTest
    {
        [Fact]
        public void Returns_in_temp_generator_for_all_integer_types_except_byte_setup_for_client_values()
        {
            var tempFactory = new SimpleValueGeneratorFactory<TemporaryValueGenerator>();

            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                tempFactory,
                new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor()),
                new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>());

            Assert.Same(tempFactory, selector.Select(CreateProperty(typeof(long), ValueGenerationOnAdd.Client)));
            Assert.Same(tempFactory, selector.Select(CreateProperty(typeof(int), ValueGenerationOnAdd.Client)));
            Assert.Same(tempFactory, selector.Select(CreateProperty(typeof(short), ValueGenerationOnAdd.Client)));
        }

        [Fact]
        public void Returns_sequence_generator_for_all_integer_types_setup_for_server_values()
        {
            var sequenceFactory = new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor());

            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>(),
                sequenceFactory,
                new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>());

            Assert.Same(sequenceFactory, selector.Select(CreateProperty(typeof(long), ValueGenerationOnAdd.Server)));
            Assert.Same(sequenceFactory, selector.Select(CreateProperty(typeof(int), ValueGenerationOnAdd.Server)));
            Assert.Same(sequenceFactory, selector.Select(CreateProperty(typeof(short), ValueGenerationOnAdd.Server)));
            Assert.Same(sequenceFactory, selector.Select(CreateProperty(typeof(byte), ValueGenerationOnAdd.Server)));
        }

        [Fact]
        public void Returns_sequential_GUID_generator_for_GUID_types_setup_for_client_values()
        {
            var sequentialGuidFactory = new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>();

            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>(),
                new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor()),
                sequentialGuidFactory);

            Assert.Same(sequentialGuidFactory, selector.Select(CreateProperty(typeof(Guid), ValueGenerationOnAdd.Client)));
        }

        [Fact]
        public void Returns_null_when_no_value_generation_configured()
        {
            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>(),
                new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor()),
                new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>());

            Assert.Null(selector.Select(CreateProperty(typeof(int), ValueGenerationOnAdd.None)));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var selector = new SqlServerValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>(),
                new SqlServerSequenceValueGeneratorFactory(new SqlStatementExecutor()),
                new SimpleValueGeneratorFactory<SequentialGuidValueGenerator>());

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.Name).Returns("AnEntity");

            var property = CreateProperty(typeof(double), ValueGenerationOnAdd.Client);

            Assert.Equal(
                GetString("FormatNoValueGenerator", "client", "MyType", "MyProperty", "Double"),
                Assert.Throws<NotSupportedException>(() => selector.Select(property)).Message);
        }

        private static Property CreateProperty(Type propertyType, ValueGenerationOnAdd valueGeneration)
        {
            var entityType = new EntityType("MyType");
            var property = entityType.AddProperty("MyProperty", propertyType);
            property.ValueGenerationOnAdd = valueGeneration;
            entityType.StorageName = "MyTable";

            new Model().AddEntityType(entityType);

            return property;
        }

        private static string GetString(string stringName, params object[] parameters)
        {
            var strings = typeof(DbContext).GetTypeInfo().Assembly.GetType(typeof(DbContext).Namespace + ".Strings");
            return (string)strings.GetTypeInfo().GetDeclaredMethods(stringName).Single().Invoke(null, parameters);
        }
    }
}
