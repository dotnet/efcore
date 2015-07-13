// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity;

namespace EntityFramework.Microbenchmarks.Core
{
    public class SqlServerBenchmarkResultProcessor
    {
        private readonly string _connectionString;

        public SqlServerBenchmarkResultProcessor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void SaveSummary(BenchmarkRunSummary summary)
        {
            using (var db = new BenchmarkContext(_connectionString))
            {
                db.Database.EnsureCreated();

                db.RunSummaries.Add(summary);
                db.SaveChanges();
            }
        }

        private class BenchmarkContext : DbContext
        {
            private readonly string _connectionString;

            public BenchmarkContext(string connectionString)
            {
                _connectionString = connectionString;
            }

            public DbSet<BenchmarkRunSummary> RunSummaries { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<BenchmarkRunSummary>()
                    .ToSqlServerTable("Runs");

                modelBuilder.Entity<BenchmarkRunSummary>()
                    .Property<int>("Id");

                modelBuilder.Entity<BenchmarkRunSummary>()
                    .Key("Id");
            }
        }
    }
}
