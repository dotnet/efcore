// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore;

public class SqlServerTypeAliasTest(SqlServerFixture fixture) : IClassFixture<SqlServerFixture>
{
    private const string DatabaseName = "SqlServerTypeAliasTest";

    protected SqlServerFixture Fixture { get; } = fixture;

    [ConditionalFact]
    public async Task Can_create_database_with_alias_columns()
    {
        using var testDatabase = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        var options = Fixture.CreateOptions(testDatabase);

        using (var context = new TypeAliasContext(options))
        {
            context.Database.ExecuteSqlRaw(
                """
CREATE TYPE datetimeAlias FROM datetime2(6);
CREATE TYPE datetimeoffsetAlias FROM datetimeoffset(6);
CREATE TYPE timeAlias FROM time(6);
CREATE TYPE decimalAlias FROM decimal(10, 6);
CREATE TYPE doubleAlias FROM float(26);
CREATE TYPE floatAlias FROM real;
CREATE TYPE binaryAlias FROM varbinary(50);
CREATE TYPE stringAlias FROM nvarchar(50);
""");

            var model = context.Model;

            var aliasEntityType = model.FindEntityType(typeof(TypeAliasEntity));
            Assert.Equal("datetimeAlias", GetColumnType(aliasEntityType!, nameof(TypeAliasEntity.DateTimeAlias)));
            Assert.Equal("datetimeoffsetAlias", GetColumnType(aliasEntityType!, nameof(TypeAliasEntity.DateTimeOffsetAlias)));
            Assert.Equal("timeAlias", GetColumnType(aliasEntityType!, nameof(TypeAliasEntity.TimeAlias)));
            Assert.Equal("decimalAlias", GetColumnType(aliasEntityType!, nameof(TypeAliasEntity.DecimalAlias)));
            Assert.Equal("doubleAlias", GetColumnType(aliasEntityType!, nameof(TypeAliasEntity.DoubleAlias)));
            Assert.Equal("floatAlias", GetColumnType(aliasEntityType!, nameof(TypeAliasEntity.FloatAlias)));
            Assert.Equal("binaryAlias", GetColumnType(aliasEntityType!, nameof(TypeAliasEntity.BinaryAlias)));
            Assert.Equal("stringAlias", GetColumnType(aliasEntityType!, nameof(TypeAliasEntity.StringAlias)));

            var facetedAliasEntityType = model.FindEntityType(typeof(TypeAliasEntityWithFacets));
            Assert.Equal("datetimeAlias", GetColumnType(facetedAliasEntityType!, nameof(TypeAliasEntityWithFacets.DateTimeAlias)));
            Assert.Equal(
                "datetimeoffsetAlias", GetColumnType(facetedAliasEntityType!, nameof(TypeAliasEntityWithFacets.DateTimeOffsetAlias)));
            Assert.Equal("timeAlias", GetColumnType(facetedAliasEntityType!, nameof(TypeAliasEntityWithFacets.TimeAlias)));
            Assert.Equal("decimalAlias", GetColumnType(facetedAliasEntityType!, nameof(TypeAliasEntityWithFacets.DecimalAlias)));
            Assert.Equal("doubleAlias", GetColumnType(facetedAliasEntityType!, nameof(TypeAliasEntityWithFacets.DoubleAlias)));
            Assert.Equal("floatAlias", GetColumnType(facetedAliasEntityType!, nameof(TypeAliasEntityWithFacets.FloatAlias)));
            Assert.Equal("binaryAlias", GetColumnType(facetedAliasEntityType!, nameof(TypeAliasEntityWithFacets.BinaryAlias)));
            Assert.Equal("stringAlias", GetColumnType(facetedAliasEntityType!, nameof(TypeAliasEntityWithFacets.StringAlias)));

            context.Database.EnsureCreatedResiliently();

            context.AddRange(
                new TypeAliasEntity
                {
                    DateTimeAlias = new DateTime(),
                    DateTimeOffsetAlias = new DateTimeOffset(),
                    TimeAlias = new TimeOnly(),
                    DecimalAlias = 3.14159m,
                    DoubleAlias = 3.14159,
                    FloatAlias = 3.14159f,
                    BinaryAlias = [0, 1, 2, 3],
                    StringAlias = "Rodrigo y Gabriela"
                },
                new TypeAliasEntityWithFacets
                {
                    DateTimeAlias = new DateTime(),
                    DateTimeOffsetAlias = new DateTimeOffset(),
                    TimeAlias = new TimeOnly(),
                    DecimalAlias = 3.14159m,
                    DoubleAlias = 3.14159,
                    FloatAlias = 3.14159f,
                    BinaryAlias = [0, 1, 2, 3],
                    StringAlias = "Mettavolution"
                });

            context.SaveChanges();
        }

        using (var context = new TypeAliasContext(options))
        {
            var entity = context.Set<TypeAliasEntity>().OrderByDescending(e => e.Id).First();

            Assert.Equal(new DateTime(), entity.DateTimeAlias);
            Assert.Equal(new DateTimeOffset(), entity.DateTimeOffsetAlias);
            Assert.Equal(new TimeOnly(), entity.TimeAlias);
            Assert.Equal(3.14m, entity.DecimalAlias);
            Assert.Equal(3.14159, entity.DoubleAlias);
            Assert.Equal(3.14159f, entity.FloatAlias);
            Assert.Equal(new byte[] { 0, 1, 2, 3 }, entity.BinaryAlias);
            Assert.Equal("Rodrigo y Gabriela", entity.StringAlias);

            var entityWithFacets = context.Set<TypeAliasEntityWithFacets>().OrderByDescending(e => e.Id).First();

            Assert.Equal(new DateTime(), entityWithFacets.DateTimeAlias);
            Assert.Equal(new DateTimeOffset(), entityWithFacets.DateTimeOffsetAlias);
            Assert.Equal(new TimeOnly(), entity.TimeAlias);
            Assert.Equal(3.14159m, entityWithFacets.DecimalAlias);
            Assert.Equal(3.14159, entityWithFacets.DoubleAlias);
            Assert.Equal(3.14159f, entityWithFacets.FloatAlias);
            Assert.Equal(new byte[] { 0, 1, 2, 3 }, entityWithFacets.BinaryAlias);
            Assert.Equal("Mettavolution", entityWithFacets.StringAlias);
        }

        string GetColumnType(IEntityType entityType, string propertyName)
            => entityType.FindProperty(propertyName)!.GetColumnType(new StoreObjectIdentifier());
    }

    private class TypeAliasContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TypeAliasEntity>();
            modelBuilder.Entity<TypeAliasEntityWithFacets>(
                b =>
                {
                    b.Property(e => e.DateTimeAlias).HasPrecision(6);
                    b.Property(e => e.DateTimeOffsetAlias).HasPrecision(6);
                    b.Property(e => e.TimeAlias).HasPrecision(6);
                    b.Property(e => e.DecimalAlias).HasPrecision(10, 6);
                    b.Property(e => e.DoubleAlias).HasPrecision(26);
                    b.Property(e => e.BinaryAlias).HasMaxLength(50);
                    b.Property(e => e.StringAlias).HasMaxLength(50);
                });
        }
    }

    private class TypeAliasEntity
    {
        public int Id { get; set; }

        [Column(TypeName = "datetimeAlias")]
        public DateTime DateTimeAlias { get; set; }

        [Column(TypeName = "datetimeoffsetAlias")]
        public DateTimeOffset DateTimeOffsetAlias { get; set; }

        [Column(TypeName = "timeAlias")]
        public TimeOnly TimeAlias { get; set; }

        [Column(TypeName = "decimalAlias")]
        public decimal DecimalAlias { get; set; }

        [Column(TypeName = "doubleAlias")]
        public double DoubleAlias { get; set; }

        [Column(TypeName = "floatAlias")]
        public float FloatAlias { get; set; }

        [Column(TypeName = "binaryAlias")]
        public byte[]? BinaryAlias { get; set; }

        [Column(TypeName = "stringAlias")]
        public string? StringAlias { get; set; }
    }

    private class TypeAliasEntityWithFacets
    {
        public int Id { get; set; }

        [Column(TypeName = "datetimeAlias")]
        public DateTime DateTimeAlias { get; set; }

        [Column(TypeName = "datetimeoffsetAlias")]
        public DateTimeOffset DateTimeOffsetAlias { get; set; }

        [Column(TypeName = "timeAlias")]
        public TimeOnly TimeAlias { get; set; }

        [Column(TypeName = "decimalAlias")]
        public decimal DecimalAlias { get; set; }

        [Column(TypeName = "doubleAlias")]
        public double DoubleAlias { get; set; }

        [Column(TypeName = "floatAlias")]
        public float FloatAlias { get; set; }

        [Column(TypeName = "binaryAlias")]
        public byte[]? BinaryAlias { get; set; }

        [Column(TypeName = "stringAlias")]
        public string? StringAlias { get; set; }
    }
}
