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
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisValueGeneratorSelectorTest
    {
        private class AnEntity
        {
            public int Id { get; set; }
            public long Long { get; set; }
            public short Short { get; set; }
            public byte Byte { get; set; }
            public int? NullableInt { get; set; }
            public long? NullableLong { get; set; }
            public short? NullableShort { get; set; }
            public byte? NullableByte { get; set; }
            public uint UInt { get; set; }
            public ulong ULong { get; set; }
            public ushort UShort { get; set; }
            public sbyte SByte { get; set; }
            public uint? NullableUInt { get; set; }
            public ulong? NullableULong { get; set; }
            public ushort? NullableUShort { get; set; }
            public sbyte? NullableSByte { get; set; }
            public string String { get; set; }
            public Guid Guid { get; set; }
            public byte[] Binary { get; set; }
            public float Float { get; set; }
        }

        private static Model BuildModel(bool generateValues = true)
        {
            var model = TestHelpers.BuildModelFor<AnEntity>();
            var entityType = model.GetEntityType(typeof(AnEntity));

            foreach (var property in entityType.Properties)
            {
                property.GenerateValueOnAdd = generateValues;
            }

            return model;
        }

        [Fact]
        public void Select_returns_RedisValueGeneratorFactory_for_all_integer_types_with_ValueGeneration_set_to_OnAdd()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(AnEntity));            

            var selector = TestHelpers.CreateContextServices(model).GetRequiredService<RedisValueGeneratorSelector>();

            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("Id")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("Long")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("Short")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("NullableInt")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("NullableLong")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("NullableShort")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("NullableByte")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("UInt")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("ULong")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("UShort")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("SByte")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("NullableUInt")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("NullableULong")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("NullableUShort")));
            Assert.IsType<RedisValueGeneratorFactory>(selector.Select(entityType.GetProperty("NullableSByte")));
            Assert.IsType<SimpleValueGeneratorFactory<TemporaryStringValueGenerator>>(selector.Select(entityType.GetProperty("String")));
            Assert.IsType<SimpleValueGeneratorFactory<GuidValueGenerator>>(selector.Select(entityType.GetProperty("Guid")));
            Assert.IsType<SimpleValueGeneratorFactory<TemporaryBinaryValueGenerator>>(selector.Select(entityType.GetProperty("Binary")));
        }

        private static string GetString(string stringName, params object[] parameters)
        {
            var strings = typeof(DbContext).GetTypeInfo().Assembly.GetType(typeof(DbContext).Namespace + ".Strings");
            return (string)strings.GetTypeInfo().GetDeclaredMethods(stringName).Single().Invoke(null, parameters);
        }
    }
}
