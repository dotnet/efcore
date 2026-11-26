// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class SqlServerIsJsonTranslationTest
{
    [Fact]
    public void IsJson_is_translated_at_compatibility_level_130()
    {
        using var context = new IsJsonContext(compatibilityLevel: 130);

        var sql = context.Entities.Where(e => EF.Functions.IsJson(e.Value)).ToQueryString();

        Assert.Contains("ISJSON([e].[Value]) = 1", sql);
    }

    [Fact]
    public void IsJson_is_not_translated_below_compatibility_level_130()
    {
        using var context = new IsJsonContext(compatibilityLevel: 120);

        var query = context.Entities.Where(e => EF.Functions.IsJson(e.Value));

        var exception = Assert.Throws<InvalidOperationException>(() => query.ToQueryString());
        Assert.Contains(nameof(RelationalDbFunctionsExtensions.IsJson), exception.Message);
    }

    private sealed class IsJsonContext(int compatibilityLevel) : DbContext
    {
        public DbSet<IsJsonEntity> Entities
            => Set<IsJsonEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(
                "Server=localhost;Database=IsJsonTranslationTest;Trusted_Connection=True",
                o => o.UseCompatibilityLevel(compatibilityLevel));
    }

    private sealed class IsJsonEntity
    {
        public int Id { get; set; }

        public string? Value { get; set; }
    }
}
