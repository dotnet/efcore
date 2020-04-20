// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ModelBuilderSqlServerTest : ModelBuilderOtherTest
    {
        protected override DbContextOptions Configure()
            => new DbContextOptionsBuilder()
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkSqlServer()
                        .AddSingleton<IModelCacheKeyFactory, TestModelCacheKeyFactory>()
                        .BuildServiceProvider())
                .UseSqlServer("Database = None")
                .Options;

        protected override void RunThroughDifferPipeline(DbContext context)
            => context.GetService<IMigrationsModelDiffer>().GetDifferences(null, context.Model.GetRelationalModel());

        [Fact]
        public void Class_with_int_key_identity_and_string_conversion_is_not_supported()
        {
            using var context = new CustomModelBuildingContext(Configure(), b =>
            {
                b.Entity<EntityWithIntKey>(
                    e =>
                    {
                        e.Property(x => x.Id)
                            .UseIdentityColumn()
                            .HasConversion(EntityWithIntKey.Converter);
                    });
            });

            Assert.Equal(
                SqlServerStrings.IdentityBadType(nameof(EntityWithIntKey.Id), nameof(EntityWithIntKey), "int"),
                Assert.Throws<ArgumentException>(() => context.Model).Message);
        }

        [Fact]
        public void Class_with_int_key_high_low_and_string_conversion_is_not_supported()
        {
            using var context = new CustomModelBuildingContext(Configure(), b =>
            {
                b.Entity<EntityWithIntKey>(
                    e =>
                    {
                        e.Property(x => x.Id)
                            .UseHiLo()
                            .HasConversion(EntityWithIntKey.Converter);
                    });
            });

            Assert.Equal(
                SqlServerStrings.SequenceBadType(
                    nameof(EntityWithIntKey.Id), nameof(EntityWithIntKey), "int"),
                Assert.Throws<ArgumentException>(() => context.Model).Message);
        }

        [Fact]
        public void Class_with_guid_key_identity_and_string_conversion_is_not_supported()
        {
            using var context = new CustomModelBuildingContext(Configure(), b =>
            {
                b.Entity<EntityWithGuidKey>(
                    e =>
                    {
                        e.HasKey(x => x.Id);
                        e.Property(x => x.Id)
                            .UseIdentityColumn()
                            .HasConversion(EntityWithGuidKey.Converter);
                    });
            });

            Assert.Equal(
                SqlServerStrings.IdentityBadType(
                    nameof(EntityWithIntKey.Id), nameof(EntityWithGuidKey), "Guid"),
                Assert.Throws<ArgumentException>(() => context.Model).Message);
        }

        [Fact]
        public void Class_with_guid_key_high_low_and_string_conversion_is_not_supported()
        {
            using var context = new CustomModelBuildingContext(Configure(), b =>
            {
                b.Entity<EntityWithGuidKey>(
                    e =>
                    {
                        e.HasKey(x => x.Id);
                        e.Property(x => x.Id)
                            .UseHiLo()
                            .HasConversion(EntityWithGuidKey.Converter);
                    });
            });

            Assert.Equal(
                SqlServerStrings.SequenceBadType(
                    nameof(EntityWithIntKey.Id), nameof(EntityWithGuidKey), "Guid"),
                Assert.Throws<ArgumentException>(() => context.Model).Message);
        }

        [Fact]
        public void Class_with_string_key_identity_and_int_conversion_is_not_supported()
        {
            using var context = new CustomModelBuildingContext(Configure(), b =>
            {
                b.Entity<EntityWithStringKey>(
                    e =>
                    {
                        e.HasKey(x => x.Id);
                        e.Property(x => x.Id)
                            .UseIdentityColumn()
                            .HasConversion(EntityWithStringKey.Converter);
                    });
            });

            Assert.Equal(
                SqlServerStrings.IdentityBadType(
                    nameof(EntityWithIntKey.Id), nameof(EntityWithStringKey), "string"),
                Assert.Throws<ArgumentException>(() => context.Model).Message);
        }

        [Fact]
        public void Class_with_string_key_high_low_and_int_conversion_is_not_supported()
        {
            using var context = new CustomModelBuildingContext(Configure(), b =>
            {
                b.Entity<EntityWithStringKey>(
                    e =>
                    {
                        e.HasKey(x => x.Id);
                        e.Property(x => x.Id)
                            .UseHiLo()
                            .HasConversion(EntityWithStringKey.Converter);
                    });
            });

            Assert.Equal(
                SqlServerStrings.SequenceBadType(
                    nameof(EntityWithIntKey.Id), nameof(EntityWithStringKey), "string"),
                Assert.Throws<ArgumentException>(() => context.Model).Message);
        }

        protected class EntityWithIntKey
        {
            public int Id { get; set; }

            public static ValueConverter Converter = new ValueConverter<int, string>(i => i.ToString(), s => int.Parse(s));
        }

        protected class EntityWithStringKey
        {
            public string Id { get; set; }

            public static ValueConverter Converter = new ValueConverter<string, int>(s => int.Parse(s), i => i.ToString());
        }

        protected class EntityWithGuidKey
        {
            public Guid Id { get; set; }

            public static ValueConverter Converter = new ValueConverter<Guid, string>(i => i.ToString(), s => Guid.Parse(s));
        }
    }
}
