// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.SqlAzure;

#nullable disable

[SqlServerCondition(SqlServerCondition.IsAzureSql)]
public class SqlAzureDatabaseCreationTest
{
    protected string StoreName { get; } = "SqlAzureDatabaseCreationTest";

    [ConditionalFact]
    public async Task Creates_database_in_elastic_pool()
    {
        await using var testDatabase = SqlServerTestStore.Create(StoreName + "Elastic");
        await using var context = new ElasticPoolContext(testDatabase);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await AssertOptionsAsync(context.Database.GetDbConnection(), 1L << 35, "GeneralPurpose", "ElasticPool");
    }

    private class ElasticPoolContext(SqlServerTestStore testStore) : DbContext
    {
        private readonly string _connectionString = testStore.ConnectionString;

        public DbSet<FastUn> FastUns { get; set; }
        public DbSet<BigUn> BigUns { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(_connectionString, b => b.ApplyConfiguration());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.HasPerformanceLevelSql($"ELASTIC_POOL ( name = {TestEnvironment.ElasticPoolName} )");
    }

    [ConditionalFact]
    public async Task Creates_basic_database()
    {
        await using var testDatabase = SqlServerTestStore.Create(StoreName + "Basic");
        await using var context = new BasicContext(testDatabase);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await AssertOptionsAsync(context.Database.GetDbConnection(), 1L << 35, "GeneralPurpose", "GP_Gen5_2");
    }

    private class BasicContext(SqlServerTestStore testStore) : DbContext
    {
        private readonly string _connectionString = testStore.ConnectionString;

        public DbSet<FastUn> FastUns { get; set; }
        public DbSet<BigUn> BigUns { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(_connectionString, b => b.ApplyConfiguration());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDatabaseMaxSize("32 GB");
            modelBuilder.HasServiceTier("'GeneralPurpose'");
            modelBuilder.HasPerformanceLevel("GP_Gen5_2");
        }
    }

    [ConditionalFact]
    public async Task Creates_business_critical_database()
    {
        await using var testDatabase = SqlServerTestStore.Create(StoreName + "BusinessCritical");
        await using var context = new BusinessCriticalContext(testDatabase);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await AssertOptionsAsync(context.Database.GetDbConnection(), 1L << 33, "BusinessCritical", "BC_Gen5_2");
    }

    private class BusinessCriticalContext(SqlServerTestStore testStore) : DbContext
    {
        private readonly string _connectionString = testStore.ConnectionString;

        public DbSet<FastUn> FastUns { get; set; }
        public DbSet<BigUn> BigUns { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(_connectionString, b => b.ApplyConfiguration());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDatabaseMaxSize("8 GB");
            modelBuilder.HasServiceTier("BusinessCritical");
            modelBuilder.HasPerformanceLevel("BC_Gen5_2");
        }
    }

    private async Task AssertOptionsAsync(DbConnection connection, long? maxSize, string serviceTier, string performanceLevel)
    {
        var storeName = new SqlConnectionStringBuilder(connection.ConnectionString).InitialCatalog;
        await Task.Delay(TimeSpan.FromMinutes(5));

        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT DATABASEPROPERTYEX('{storeName}', 'EDITION'),
       DATABASEPROPERTYEX('{storeName}', 'ServiceObjective'),
       DATABASEPROPERTYEX('{storeName}', 'MaxSizeInBytes');";
        command.CommandTimeout = 300;

        using var reader = await command.ExecuteReaderAsync();

        await reader.ReadAsync();

        Assert.Equal(serviceTier, await reader.IsDBNullAsync(0) ? null : await reader.GetFieldValueAsync<string>(0));
        Assert.Equal(performanceLevel, await reader.IsDBNullAsync(1) ? null : await reader.GetFieldValueAsync<string>(1));
        Assert.Equal(maxSize, await reader.IsDBNullAsync(2) ? null : await reader.GetFieldValueAsync<long>(2));
    }

    private class BigUn
    {
        public int Id { get; set; }
        public ICollection<FastUn> FastUns { get; set; }
    }

    private class FastUn
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BigUn BigUn { get; set; }
    }
}
