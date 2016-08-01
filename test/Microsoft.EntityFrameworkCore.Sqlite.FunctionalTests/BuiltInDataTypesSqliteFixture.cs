// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class BuiltInDataTypesSqliteFixture : BuiltInDataTypesFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly SqliteTestStore _testStore;

        public BuiltInDataTypesSqliteFixture()
        {
            _testStore = SqliteTestStore.CreateScratch();

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .UseSqlite(_testStore.Connection)
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using (var context = new DbContext(_options))
            {
                context.Database.EnsureClean();
            }
        }

        public override DbContext CreateContext()
        {
            var context = new DbContext(_options);
            context.Database.UseTransaction(_testStore.Transaction);
            return context;
        }

        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MappedDataTypes>(b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Integer).HasColumnType("Integer");
                    b.Property(e => e.Real).HasColumnType("Real");
                    b.Property(e => e.Text).HasColumnType("Text").IsRequired();
                    b.Property(e => e.Blob).HasColumnType("Blob").IsRequired();
                    b.Property(e => e.SomeString).HasColumnType("SomeString").IsRequired();
                    b.Property(e => e.Int).HasColumnType("Int");
                });

            modelBuilder.Entity<MappedNullableDataTypes>(b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Integer).HasColumnType("Integer");
                    b.Property(e => e.Real).HasColumnType("Real");
                    b.Property(e => e.Text).HasColumnType("Text");
                    b.Property(e => e.Blob).HasColumnType("Blob");
                    b.Property(e => e.SomeString).HasColumnType("SomeString");
                    b.Property(e => e.Int).HasColumnType("Int");
                });

            modelBuilder.Entity<MappedSizedDataTypes>(b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Nvarchar).HasColumnType("nvarchar(3)");
                    b.Property(e => e.Binary).HasColumnType("varbinary(3)");
                });

            modelBuilder.Entity<MappedScaledDataTypes>(b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Float).HasColumnType("real(3)");
                    b.Property(e => e.Datetimeoffset).HasColumnType("datetimeoffset(3)");
                    b.Property(e => e.Datetime2).HasColumnType("datetime2(3)");
                    b.Property(e => e.Decimal).HasColumnType("decimal(3)");
                });

            modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>(b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Decimal).HasColumnType("decimal(5, 2)");
                });

            modelBuilder.Entity<MappedDataTypesWithIdentity>(b =>
                {
                    b.Property(e => e.Integer).HasColumnType("Integer");
                    b.Property(e => e.Real).HasColumnType("Real");
                    b.Property(e => e.Text).HasColumnType("Text").IsRequired();
                    b.Property(e => e.Blob).HasColumnType("Blob").IsRequired();
                    b.Property(e => e.SomeString).HasColumnType("SomeString").IsRequired();
                    b.Property(e => e.Int).HasColumnType("Int");
                });

            modelBuilder.Entity<MappedNullableDataTypesWithIdentity>(b =>
                {
                    b.Property(e => e.Integer).HasColumnType("Integer");
                    b.Property(e => e.Real).HasColumnType("Real");
                    b.Property(e => e.Text).HasColumnType("Text");
                    b.Property(e => e.Blob).HasColumnType("Blob");
                    b.Property(e => e.SomeString).HasColumnType("SomeString");
                    b.Property(e => e.Int).HasColumnType("Int");
                });

            modelBuilder.Entity<MappedSizedDataTypesWithIdentity>(b =>
                {
                    b.Property(e => e.Nvarchar).HasColumnType("nvarchar(3)");
                    b.Property(e => e.Binary).HasColumnType("varbinary(3)");
                });

            modelBuilder.Entity<MappedScaledDataTypesWithIdentity>(b =>
                {
                    b.Property(e => e.Float).HasColumnType("real(3)");
                    b.Property(e => e.Datetimeoffset).HasColumnType("datetimeoffset(3)");
                    b.Property(e => e.Datetime2).HasColumnType("datetime2(3)");
                    b.Property(e => e.Decimal).HasColumnType("decimal(3)");
                });

            modelBuilder.Entity<MappedPrecisionAndScaledDataTypesWithIdentity>(b => { b.Property(e => e.Decimal).HasColumnType("decimal(5, 2)"); });
        }

        public override void Dispose() => _testStore.Dispose();

        public override bool SupportsBinaryKeys => true;

        public override DateTime DefaultDateTime => new DateTime();
    }

    public class MappedDataTypes
    {
        public int Id { get; set; }
        public long Integer { get; set; }
        public double Real { get; set; }
        public string Text { get; set; }
        public byte[] Blob { get; set; }
        public string SomeString { get; set; }
        public int Int { get; set; }
    }

    public class MappedSizedDataTypes
    {
        public int Id { get; set; }
        public string Nvarchar { get; set; }
        public byte[] Binary { get; set; }
    }

    public class MappedScaledDataTypes
    {
        public int Id { get; set; }
        public float Float { get; set; }
        public DateTimeOffset Datetimeoffset { get; set; }
        public DateTime Datetime2 { get; set; }
        public decimal Decimal { get; set; }
    }

    public class MappedPrecisionAndScaledDataTypes
    {
        public int Id { get; set; }
        public decimal Decimal { get; set; }
    }

    public class MappedNullableDataTypes
    {
        public int Id { get; set; }
        public long? Integer { get; set; }
        public double? Real { get; set; }
        public string Text { get; set; }
        public byte[] Blob { get; set; }
        public string SomeString { get; set; }
        public int? Int { get; set; }
    }

    public class MappedDataTypesWithIdentity
    {
        public int Id { get; set; }
        public int AltId { get; set; }
        public long Integer { get; set; }
        public double Real { get; set; }
        public string Text { get; set; }
        public byte[] Blob { get; set; }
        public string SomeString { get; set; }
        public int Int { get; set; }
    }

    public class MappedSizedDataTypesWithIdentity
    {
        public int Id { get; set; }
        public int AltId { get; set; }
        public string Nvarchar { get; set; }
        public byte[] Binary { get; set; }
    }

    public class MappedScaledDataTypesWithIdentity
    {
        public int Id { get; set; }
        public int AltId { get; set; }
        public float Float { get; set; }
        public DateTimeOffset Datetimeoffset { get; set; }
        public DateTime Datetime2 { get; set; }
        public decimal Decimal { get; set; }
    }

    public class MappedPrecisionAndScaledDataTypesWithIdentity
    {
        public int Id { get; set; }
        public int AltId { get; set; }
        public decimal Decimal { get; set; }
    }

    public class MappedNullableDataTypesWithIdentity
    {
        public int Id { get; set; }
        public int AltId { get; set; }
        public long? Integer { get; set; }
        public double? Real { get; set; }
        public string Text { get; set; }
        public byte[] Blob { get; set; }
        public string SomeString { get; set; }
        public int? Int { get; set; }
    }
}
