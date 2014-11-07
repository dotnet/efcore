// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisValueGeneratorSelectorTest
    {
        [Fact]
        public void Select_returns_RedisValueGeneratorFactory_for_all_integer_types_with_ValueGeneration_set_to_OnAdd()
        {
            var guidValueGenerator = new SimpleValueGeneratorFactory<GuidValueGenerator>();
            var redisValueGeneratorFactory = new RedisValueGeneratorFactory(Mock.Of<RedisDatabase>());

            var selector = new RedisValueGeneratorSelector(guidValueGenerator, redisValueGeneratorFactory);

            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(long))));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(int))));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(short))));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(byte))));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(ulong))));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(uint))));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(ushort))));
            Assert.Same(redisValueGeneratorFactory, selector.Select(CreateProperty(typeof(sbyte))));
        }

        [Fact]
        public void Select_returns_GuidValueGenerator_for_Guid_type_with_ValueGeneration_set_to_OnAdd()
        {
            var guidValueGenerator = new SimpleValueGeneratorFactory<GuidValueGenerator>();
            var redisValueGeneratorFactory = new RedisValueGeneratorFactory(Mock.Of<RedisDatabase>());

            var selector = new RedisValueGeneratorSelector(guidValueGenerator, redisValueGeneratorFactory);

            Assert.Same(guidValueGenerator, selector.Select(CreateProperty(typeof(Guid))));
        }

        [Fact]
        public void Select_returns_null_for_all_types_with_ValueGeneration_set_to_None()
        {
            var guidValueGenerator = new SimpleValueGeneratorFactory<GuidValueGenerator>();
            var redisValueGeneratorFactory = new RedisValueGeneratorFactory(Mock.Of<RedisDatabase>());

            var selector = new RedisValueGeneratorSelector(guidValueGenerator, redisValueGeneratorFactory);

            Assert.Null(selector.Select(CreateProperty(typeof(long), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(int), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(short), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(byte), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(ulong), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(uint), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(ushort), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(sbyte), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(string), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(float), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(double), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(Guid), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(DateTime), generateValues: false)));
            Assert.Null(selector.Select(CreateProperty(typeof(DateTimeOffset), generateValues: false)));
        }

        [Fact]
        public void Select_throws_for_unsupported_combinations()
        {
            var guidValueGenerator = new SimpleValueGeneratorFactory<GuidValueGenerator>();
            var redisValueGeneratorFactory = new RedisValueGeneratorFactory(Mock.Of<RedisDatabase>());

            var selector = new RedisValueGeneratorSelector(guidValueGenerator, redisValueGeneratorFactory);

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.Name).Returns("AnEntity");

            Assert.Equal(
                GetString("FormatNoValueGenerator", "MyProperty", "MyType", "String"),
                Assert.Throws<NotSupportedException>(() => selector.Select(CreateProperty(typeof(string)))).Message);
            Assert.Equal(
                GetString("FormatNoValueGenerator", "MyProperty", "MyType", "Single"),
                Assert.Throws<NotSupportedException>(() => selector.Select(CreateProperty(typeof(float)))).Message);
            Assert.Equal(
                GetString("FormatNoValueGenerator", "MyProperty", "MyType", "Double"),
                Assert.Throws<NotSupportedException>(() => selector.Select(CreateProperty(typeof(double)))).Message);
            Assert.Equal(
                GetString("FormatNoValueGenerator", "MyProperty", "MyType", "DateTime"),
                Assert.Throws<NotSupportedException>(() => selector.Select(CreateProperty(typeof(DateTime)))).Message);
            Assert.Equal(
                GetString("FormatNoValueGenerator", "MyProperty", "MyType", "DateTimeOffset"),
                Assert.Throws<NotSupportedException>(() => selector.Select(CreateProperty(typeof(DateTimeOffset)))).Message);
        }

        private static Property CreateProperty(Type propertyType, bool generateValues = true)
        {
            var entityType = new Model().AddEntityType("MyType");
            var property = entityType.GetOrAddProperty("MyProperty", propertyType, shadowProperty: true);
            property.GenerateValueOnAdd = generateValues;

            return property;
        }

        private static string GetString(string stringName, params object[] parameters)
        {
            var strings = typeof(DbContext).GetTypeInfo().Assembly.GetType(typeof(DbContext).Namespace + ".Strings");
            return (string)strings.GetTypeInfo().GetDeclaredMethods(stringName).Single().Invoke(null, parameters);
        }
    }
}
