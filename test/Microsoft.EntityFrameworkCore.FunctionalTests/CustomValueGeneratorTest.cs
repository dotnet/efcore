// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public class CustomValueGeneratorTest
    {
        private static readonly IServiceProvider _serviceProvider
            = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddScoped<InMemoryValueGeneratorSelector, CustomInMemoryValueGeneratorSelector>()
                .BuildServiceProvider();

        [Fact]
        public void Can_use_custom_value_generators()
        {
            using (var context = new CustomValueGeneratorContext())
            {
                var entities = new List<SomeEntity>();
                for (var i = 0; i < CustomGuidValueGenerator.SpecialGuids.Length; i++)
                {
                    entities.Add(context.Add(new SomeEntity()).Entity);
                }

                Assert.Equal(entities.Select(e => e.Id), entities.OrderBy(e => ToCounter(e.Id)).Select(e => e.Id));

                Assert.Equal(CustomGuidValueGenerator.SpecialGuids, entities.Select(e => e.SpecialId));
            }
        }

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

        private class CustomValueGeneratorContext : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseInMemoryDatabase();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<SomeEntity>()
                    .Property(e => e.SpecialId)
                    .HasAnnotation("SpecialGuid", true)
                    .Metadata.RequiresValueGenerator = true;
            }
        }

        private class SomeEntity
        {
            public Guid Id { get; set; }
            public Guid SpecialId { get; set; }
        }

        private class CustomInMemoryValueGeneratorSelector : InMemoryValueGeneratorSelector
        {
            public CustomInMemoryValueGeneratorSelector(IValueGeneratorCache cache)
                : base(cache)
            {
            }

            public override ValueGenerator Create(IProperty property, IEntityType entityType)
            {
                if (property["SpecialGuid"] != null)
                {
                    return new CustomGuidValueGenerator();
                }

                if (property.ClrType == typeof(Guid))
                {
                    return new SequentialGuidValueGenerator();
                }

                return base.Create(property, entityType);
            }
        }

        private class CustomGuidValueGenerator : ValueGenerator<Guid>
        {
            public static Guid[] SpecialGuids { get; } =
                {
                    Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                    Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()
                };

            private int _counter = -1;

            public override Guid Next()
                => SpecialGuids[Interlocked.Increment(ref _counter)];

            public override bool GeneratesTemporaryValues => false;
        }
    }
}
