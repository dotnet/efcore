namespace Microsoft.EntityFrameworkCore.Query;

public class PricingContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<PriceRow> PriceRows => Set<PriceRow>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Campaign>().HasQueryFilter(campaign => !campaign.Hidden);
        modelBuilder.Entity<PriceRow>()
            .HasOne(row => row.Campaign)
            .WithMany()
            .HasForeignKey(row => row.CampaignId)
            .IsRequired();
    }

    public async Task SeedAsync()
    {
        PriceRows.AddRange(
            new PriceRow { Id = 1, Band = 1, Campaign = new Campaign { Id = 1, Amount = 10 } },
            new PriceRow { Id = 2, Band = 2, Campaign = new Campaign { Id = 2, Amount = 20, Hidden = true } });
        await SaveChangesAsync();
    }
}

public class SampledPricingContext(DbContextOptions options) : PricingContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PriceRow>().HasQueryFilter(row => EF.Functions.Random() >= 0);
    }
}

public class PriceRow
{
    public int Id { get; set; }
    public int Band { get; set; }
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
}

public class Campaign
{
    public int Id { get; set; }
    public int Amount { get; set; }
    public bool Hidden { get; set; }
}

public static class PricingQueries
{
    public static IQueryable<object> Summary(PricingContext context)
        => from row in context.PriceRows
           let latest = context.PriceRows
               .Where(candidate => candidate.Band <= row.Band)
               .OrderByDescending(candidate => candidate.Band)
               .ThenBy(candidate => candidate.Id)
               .FirstOrDefault()
           orderby row.Band
           select new { Band = (int?)latest!.Band, Amount = (int?)latest.Campaign.Amount };

    public static IQueryable<object> StableSummary(PricingContext context)
        => from row in context.PriceRows
           let latest = context.PriceRows
               .Where(candidate => candidate.Band < row.Band)
               .OrderByDescending(candidate => candidate.Band)
               .ThenBy(candidate => candidate.Id)
               .FirstOrDefault()
           select new { row.Id, LatestId = (int?)latest!.Id, LatestBand = (int?)latest.Band };

    public static IQueryable<object> RankedSummary(PricingContext context)
        => from row in context.PriceRows
           let latest = context.PriceRows
               .Where(candidate => candidate.Band <= row.Band)
               .OrderBy(candidate => Guid.NewGuid())
               .FirstOrDefault()
           select new { row.Id, LatestId = (int?)latest!.Id, LatestBand = (int?)latest.Band };
}

public static class NestedProjection
{
    public static Expression<Func<PriceRow, IEnumerable<object>>> Build(IQueryable<PriceRow> rows)
    {
        var owner = Expression.Parameter(typeof(PriceRow), "owner");
        var child = Expression.Parameter(typeof(PriceRow), "child");
        var candidate = Expression.Parameter(typeof(PriceRow), "candidate");
        var source = rows.Expression;
        var filtered = Expression.Call(
            QueryableMethods.Where.MakeGenericMethod(typeof(PriceRow)),
            source,
            Expression.Quote(Expression.Lambda(
                Expression.LessThanOrEqual(
                    Expression.Property(candidate, nameof(PriceRow.Band)),
                    Expression.Property(child, nameof(PriceRow.Band))),
                candidate)));
        var ordered = Expression.Call(
            QueryableMethods.OrderByDescending.MakeGenericMethod(typeof(PriceRow), typeof(int)),
            filtered,
            Expression.Quote(Expression.Lambda(Expression.Property(candidate, nameof(PriceRow.Id)), candidate)));
        var latest = Expression.Call(QueryableMethods.FirstOrDefaultWithoutPredicate.MakeGenericMethod(typeof(PriceRow)), ordered);
        var pair = Expression.NewArrayInit(
            typeof(int?),
            Expression.Convert(Expression.Property(latest, nameof(PriceRow.Id)), typeof(int?)),
            Expression.Convert(Expression.Property(latest, nameof(PriceRow.Band)), typeof(int?)));
        var nestedSelector = Expression.Lambda<Func<PriceRow, object>>(Expression.Convert(pair, typeof(object)), child);
        var nested = Expression.Call(
            QueryableMethods.Select.MakeGenericMethod(typeof(PriceRow), typeof(object)),
            source,
            Expression.Quote(nestedSelector));
        return Expression.Lambda<Func<PriceRow, IEnumerable<object>>>(nested, owner);
    }
}

public static class MixedProjection
{
    public static Expression<Func<int?[]>> Build(IQueryable<PriceRow> rows)
    {
        var candidate = Expression.Parameter(typeof(PriceRow), "candidate");
        var ordered = Expression.Call(
            QueryableMethods.OrderBy.MakeGenericMethod(typeof(PriceRow), typeof(int)),
            rows.Expression,
            Expression.Quote(Expression.Lambda(Expression.Property(candidate, nameof(PriceRow.Id)), candidate)));
        var latest = Expression.Call(QueryableMethods.FirstOrDefaultWithoutPredicate.MakeGenericMethod(typeof(PriceRow)), ordered);
        var values = Expression.NewArrayInit(
            typeof(int?),
            Expression.Convert(Expression.Property(latest, nameof(PriceRow.Id)), typeof(int?)),
            Expression.Convert(Expression.Property(latest, nameof(PriceRow.Band)), typeof(int?)),
            Expression.Convert(
                Expression.Property(Expression.Property(latest, nameof(PriceRow.Campaign)), nameof(Campaign.Amount)),
                typeof(int?)));
        return Expression.Lambda<Func<int?[]>>(values);
    }
}