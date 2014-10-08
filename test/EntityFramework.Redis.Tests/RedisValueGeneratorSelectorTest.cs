// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisValueGeneratorSelectorTest
    {
        [Fact]
        public void Select_returns_RedisValueGeneratorFactory_for_all_integer_types_with_ValueGeneration_set_to_OnAdd()
        {
            var dbConfigurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(dbConfigurationMock.Object);
            var guidValueGenerator = new SimpleValueGeneratorFactory<GuidValueGenerator>();
            var redisValueGeneratorFactory = new RedisValueGeneratorFactory(redisDatabaseMock.Object);

            var selector = new RedisValueGeneratorSelector(guidValueGenerator, redisValueGeneratorFactory);

            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(long), ValueGeneration.OnAdd)));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(int), ValueGeneration.OnAdd)));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(short), ValueGeneration.OnAdd)));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(byte), ValueGeneration.OnAdd)));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(ulong), ValueGeneration.OnAdd)));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(uint), ValueGeneration.OnAdd)));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(ushort), ValueGeneration.OnAdd)));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(sbyte), ValueGeneration.OnAdd)));
        }

        [Fact]
        public void Select_returns_GuidValueGenerator_for_Guid_type_with_ValueGeneration_set_to_OnAdd()
        {
            var dbConfigurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(dbConfigurationMock.Object);
            var guidValueGenerator = new SimpleValueGeneratorFactory<GuidValueGenerator>();
            var redisValueGeneratorFactory = new RedisValueGeneratorFactory(redisDatabaseMock.Object);

            var selector = new RedisValueGeneratorSelector(guidValueGenerator, redisValueGeneratorFactory);

            Assert.Same(guidValueGenerator, selector.Select(CreateProperty(typeof(Guid), ValueGeneration.OnAdd)));
        }

        [Fact]
        public void Select_returns_null_for_all_types_with_ValueGeneration_set_to_None()
        {
            var dbConfigurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(dbConfigurationMock.Object);
            var guidValueGenerator = new SimpleValueGeneratorFactory<GuidValueGenerator>();
            var redisValueGeneratorFactory = new RedisValueGeneratorFactory(redisDatabaseMock.Object);

            var selector = new RedisValueGeneratorSelector(guidValueGenerator, redisValueGeneratorFactory);

            Assert.Null(selector.Select(CreateProperty(typeof(long), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(int), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(short), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(byte), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(ulong), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(uint), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(ushort), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(sbyte), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(string), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(float), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(double), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(Guid), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(DateTime), ValueGeneration.None)));
            Assert.Null(selector.Select(CreateProperty(typeof(DateTimeOffset), ValueGeneration.None)));
        }

        [Fact]
        public void Select_throws_for_unsupported_combinations()
        {
            var dbConfigurationMock = new Mock<DbContextConfiguration>();
            var redisDatabaseMock = new Mock<RedisDatabase>(dbConfigurationMock.Object);
            var guidValueGenerator = new SimpleValueGeneratorFactory<GuidValueGenerator>();
            var redisValueGeneratorFactory = new RedisValueGeneratorFactory(redisDatabaseMock.Object);

            var selector = new RedisValueGeneratorSelector(guidValueGenerator, redisValueGeneratorFactory);

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.Name).Returns("AnEntity");

            Assert.Equal(
                GetString("FormatNoValueGenerator", "MyProperty", "MyType", "String"),
                Assert.Throws<NotSupportedException>(() => selector.Select(CreateProperty(typeof(string), ValueGeneration.OnAdd))).Message);
            Assert.Equal(
                GetString("FormatNoValueGenerator", "MyProperty", "MyType", "Single"),
                Assert.Throws<NotSupportedException>(() => selector.Select(CreateProperty(typeof(float), ValueGeneration.OnAdd))).Message);
            Assert.Equal(
                GetString("FormatNoValueGenerator", "MyProperty", "MyType", "Double"),
                Assert.Throws<NotSupportedException>(() => selector.Select(CreateProperty(typeof(double), ValueGeneration.OnAdd))).Message);
            Assert.Equal(
                GetString("FormatNoValueGenerator", "MyProperty", "MyType", "DateTime"),
                Assert.Throws<NotSupportedException>(() => selector.Select(CreateProperty(typeof(DateTime), ValueGeneration.OnAdd))).Message);
            Assert.Equal(
                GetString("FormatNoValueGenerator", "MyProperty", "MyType", "DateTimeOffset"),
                Assert.Throws<NotSupportedException>(() => selector.Select(CreateProperty(typeof(DateTimeOffset), ValueGeneration.OnAdd))).Message);
        }

        private static Property CreateProperty(Type propertyType, ValueGeneration valueGeneration)
        {
            var entityType = new Model().AddEntityType("MyType");
            var property = entityType.GetOrAddProperty("MyProperty", propertyType, shadowProperty: true);
            property.ValueGeneration = valueGeneration;

            return property;
        }

        private static string GetString(string stringName, params object[] parameters)
        {
            var strings = typeof(DbContext).GetTypeInfo().Assembly.GetType(typeof(DbContext).Namespace + ".Strings");
            return (string)strings.GetTypeInfo().GetDeclaredMethods(stringName).Single().Invoke(null, parameters);
        }
    }
}
