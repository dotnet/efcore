// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class BuiltInDataTypesSqliteFixture : BuiltInDataTypesFixtureBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqliteTestStore _testStore;

        public BuiltInDataTypesSqliteFixture()
        {
            _testStore = SqliteTestStore.CreateScratch();

            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite(_testStore.Connection);

            _options = optionsBuilder.Options;

            using (var context = new DbContext(_serviceProvider, _options))
            {
                context.Database.EnsureCreated();
            }
        }

        public override DbContext CreateContext()
        {
            var context = new DbContext(_serviceProvider, _options);
            context.Database.GetRelationalConnection().UseTransaction(_testStore.Transaction);
            return context;
        }

        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MappedDataTypes>(b =>
                {
                    b.Property(e => e.Integer).ColumnType("Integer");
                    b.Property(e => e.Real).ColumnType("Real");
                    b.Property(e => e.Text).ColumnType("Text").Required();
                    b.Property(e => e.Blob).ColumnType("Blob").Required();
                    b.Property(e => e.SomeString).ColumnType("SomeString").Required();
                    b.Property(e => e.Int).ColumnType("Int");
                });

            modelBuilder.Entity<MappedNullableDataTypes>(b =>
                {
                    b.Property(e => e.Integer).ColumnType("Integer");
                    b.Property(e => e.Real).ColumnType("Real");
                    b.Property(e => e.Text).ColumnType("Text");
                    b.Property(e => e.Blob).ColumnType("Blob");
                    b.Property(e => e.SomeString).ColumnType("SomeString");
                    b.Property(e => e.Int).ColumnType("Int");
                });

            modelBuilder.Entity<MappedSizedDataTypes>(b =>
            {
                b.Property(e => e.Nvarchar).ColumnType("nvarchar(3)");
                b.Property(e => e.Binary).ColumnType("varbinary(3)");
            });

            modelBuilder.Entity<MappedScaledDataTypes>(b =>
            {
                b.Property(e => e.Float).ColumnType("real(3)");
                b.Property(e => e.Datetimeoffset).ColumnType("datetimeoffset(3)");
                b.Property(e => e.Datetime2).ColumnType("datetime2(3)");
                b.Property(e => e.Decimal).ColumnType("decimal(3)");
            });

            modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>(b =>
            {
                b.Property(e => e.Decimal).ColumnType("decimal(5, 2)");
            });
        }

        public override void Dispose() => _testStore.Dispose();

        public override bool SupportsBinaryKeys => true;
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
}
