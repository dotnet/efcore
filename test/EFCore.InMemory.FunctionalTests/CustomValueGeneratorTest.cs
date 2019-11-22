// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class CustomValueGeneratorTest
    {
        [ConditionalFact]
        public void Can_use_custom_value_generators()
        {
            using (var context = new CustomValueGeneratorContext())
            {
                var entities = new List<SomeEntity>();
                for (var i = 0; i < CustomGuidValueGenerator.SpecialGuids.Length; i++)
                {
                    entities.Add(
                        context.Add(
                            new SomeEntity { Name = _names[i] }).Entity);
                }

                Assert.Equal(entities.Select(e => e.Id), entities.OrderBy(e => ToCounter(e.Id)).Select(e => e.Id));

                Assert.Equal(CustomGuidValueGenerator.SpecialGuids, entities.Select(e => e.SpecialId));

                Assert.Equal(_names.Select((n, i) => n + " - " + (i + 1)), entities.Select(e => e.SpecialString));
            }
        }

        private class CustomValueGeneratorContext : DbContext
        {
            private static readonly IServiceProvider _serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddScoped<IValueGeneratorSelector, CustomInMemoryValueGeneratorSelector>()
                    .BuildServiceProvider();

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseInMemoryDatabase(nameof(CustomValueGeneratorContext));

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder
                    .Entity<SomeEntity>(
                        b =>
                        {
                            b.HasAlternateKey(
                                e => new { e.SpecialId, e.SpecialString });
                            b.Property(e => e.SpecialId)
                                .HasAnnotation("SpecialGuid", true)
                                .ValueGeneratedOnAdd();

                            b.Property(e => e.SpecialString)
                                .ValueGeneratedOnAdd();
                        });
        }

        [ConditionalFact]
        public void Can_use_custom_value_generator_from_annotated_type()
        {
            using (var context = new CustomValueGeneratorContextAnnotateType())
            {
                var entities = new List<SomeEntity>();
                for (var i = 0; i < CustomGuidValueGenerator.SpecialGuids.Length; i++)
                {
                    entities.Add(
                        context.Add(
                            new SomeEntity { Name = _names[i] }).Entity);
                }

                Assert.Equal(entities.Select(e => e.Id), entities.OrderBy(e => ToCounter(e.Id)).Select(e => e.Id));

                Assert.Equal(CustomGuidValueGenerator.SpecialGuids, entities.Select(e => e.SpecialId));

                Assert.Equal(_names.Select((n, i) => n + " - " + (i + 1)), entities.Select(e => e.SpecialString));
            }
        }

        private class CustomValueGeneratorContextAnnotateType : DbContext
        {
            private static readonly IServiceProvider _serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseInMemoryDatabase(nameof(CustomValueGeneratorContextAnnotateType));

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder
                    .Entity<SomeEntity>(
                        b =>
                        {
                            b.Property(e => e.Id).HasValueGenerator<SequentialGuidValueGenerator>();
                            b.Property(e => e.SpecialId).HasValueGenerator(typeof(CustomGuidValueGenerator));
                            b.Property(e => e.SpecialString).HasValueGenerator<SomeEntityStringValueGenerator>();
                        });
        }

        [ConditionalFact]
        public void Can_use_custom_value_generator_from_annotated_factory()
        {
            using (var context = new CustomValueGeneratorContextAnnotateFactory())
            {
                var entities = new List<SomeEntity>();
                for (var i = 0; i < CustomGuidValueGenerator.SpecialGuids.Length; i++)
                {
                    entities.Add(
                        context.Add(
                            new SomeEntity { Name = _names[i] }).Entity);
                }

                Assert.Equal(entities.Select(e => e.Id), entities.OrderBy(e => ToCounter(e.Id)).Select(e => e.Id));

                Assert.Equal(CustomGuidValueGenerator.SpecialGuids, entities.Select(e => e.SpecialId));

                Assert.Equal(_names.Select((n, i) => n + " - " + (i + 1)), entities.Select(e => e.SpecialString));
            }
        }

        private class CustomValueGeneratorContextAnnotateFactory : DbContext
        {
            private static readonly IServiceProvider _serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseInMemoryDatabase(nameof(CustomValueGeneratorContextAnnotateFactory));

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder
                    .Entity<SomeEntity>(
                        b =>
                        {
                            var factory = new CustomValueGeneratorFactory();

                            b.Property(e => e.Id).HasValueGenerator((p, e) => factory.Create(p));

                            b.Property(e => e.SpecialId)
                                .Metadata.SetValueGeneratorFactory((p, e) => factory.Create(p));

                            b.Property(e => e.SpecialId)
                                .HasAnnotation("SpecialGuid", true)
                                .ValueGeneratedOnAdd();

                            b.Property(e => e.SpecialString).HasValueGenerator((p, e) => factory.Create(p));
                        });
        }

        private class SomeEntity
        {
            public Guid Id { get; set; }
            public Guid SpecialId { get; set; }
            public string SpecialString { get; set; }
            public string Name { get; set; }
        }

        private readonly string[] _names =
        {
            "Jamie Vardy",
            "Danny Drinkwater",
            "Andy King",
            "Riyad Mahrez",
            "Kasper Schmeichel",
            "Wes Morgan",
            "Robert Huth",
            "Leonardo Ulloa"
        };

        private static long ToCounter(Guid guid)
        {
            var guidBytes = guid.ToByteArray();
            var counterBytes = new byte[8];

            counterBytes[1] = guidBytes[08];
            counterBytes[0] = guidBytes[09];
            counterBytes[7] = guidBytes[10];
            counterBytes[6] = guidBytes[11];
            counterBytes[5] = guidBytes[12];
            counterBytes[4] = guidBytes[13];
            counterBytes[3] = guidBytes[14];
            counterBytes[2] = guidBytes[15];

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

            return BitConverter.ToInt64(counterBytes, 0);
        }

        private class CustomInMemoryValueGeneratorSelector : InMemoryValueGeneratorSelector
        {
            private readonly ValueGeneratorFactory _factory = new CustomValueGeneratorFactory();

            public CustomInMemoryValueGeneratorSelector(
                ValueGeneratorSelectorDependencies dependencies,
                IInMemoryDatabase inMemoryDatabase)
                : base(dependencies, inMemoryDatabase)
            {
            }

            public override ValueGenerator Create(IProperty property, IEntityType entityType)
                => _factory.Create(property);
        }

        private class CustomGuidValueGenerator : ValueGenerator<Guid>
        {
            public static Guid[] SpecialGuids { get; } =
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            private int _counter = -1;

            public override Guid Next(EntityEntry entry)
                => SpecialGuids[Interlocked.Increment(ref _counter)];

            public override bool GeneratesTemporaryValues => false;
        }

        private class SomeEntityStringValueGenerator : ValueGenerator<string>
        {
            private int _counter;

            public override string Next(EntityEntry entry)
                => ((SomeEntity)entry.Entity).Name + " - " + Interlocked.Increment(ref _counter);

            public override bool GeneratesTemporaryValues => false;
        }

        private class CustomValueGeneratorFactory : ValueGeneratorFactory
        {
            public override ValueGenerator Create(IProperty property)
            {
                if (property.ClrType == typeof(Guid))
                {
                    return property["SpecialGuid"] != null
                        ? (ValueGenerator)new CustomGuidValueGenerator()
                        : new SequentialGuidValueGenerator();
                }

                return property.ClrType == typeof(string)
                    && property.DeclaringEntityType.ClrType == typeof(SomeEntity)
                        ? new SomeEntityStringValueGenerator()
                        : null;
            }
        }
    }
}
