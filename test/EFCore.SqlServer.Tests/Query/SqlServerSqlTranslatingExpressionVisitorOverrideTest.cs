// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

// The queryable and projection translators pass the query a translation goes into on to the SQL translator; they must still do so
// through its public virtual entry points, so that a provider overriding them sees every translation (#27130).
public class SqlServerSqlTranslatingExpressionVisitorOverrideTest
{
    [Fact]
    public void Overrides_are_invoked_for_predicates_orderings_aggregate_selectors_and_projections()
    {
        using var context = new OverrideContext();
        var translator = context.StartRecording();

        var sql = context.Entities
            .Where(e => e.Value > 1)
            .OrderBy(e => e.Name)
            .Select(e => new { e.Name, Total = context.Entities.Sum(o => o.Value + e.Value) })
            .ToQueryString();

        Assert.Contains("[e].[Value] > 1", sql);
        Assert.Contains(translator.Translated, e => e.ToString().Contains("Value > 1"));
        Assert.Contains(translator.Translated, e => e.ToString().EndsWith(".Name"));
        Assert.Contains(translator.Translated, e => e.ToString().Contains("].Value + ["));
        Assert.NotEmpty(translator.TranslatedProjections);
    }

    [Fact]
    public void Grouping_aggregate_translated_through_overrides_is_still_folded_into_the_query_owning_the_grouping()
    {
        using var context = new OverrideContext();
        var translator = context.StartRecording();

        var sql = context.Entities
            .GroupBy(e => e.Name)
            .Where(g => g.Count() > 1)
            .Select(g => new { g.Key, Total = g.Sum(e => e.Value) })
            .ToQueryString();

        Assert.Contains("HAVING COUNT(*) > 1", sql);
        Assert.Contains("ISNULL(SUM([e].[Value]), 0) AS [Total]", sql);
        Assert.Contains(translator.Translated, e => e.ToString().Contains("Count()"));
        Assert.Contains(translator.TranslatedProjections, e => e.ToString().Contains("Sum("));
    }

    private sealed class OverrideContext : DbContext
    {
        public DbSet<OverrideEntity> Entities
            => Set<OverrideEntity>();

        // The factory records what every translator it creates is asked to translate. It's shared by every context using the same
        // internal service provider, so recording starts from empty.
        public RecordingSqlTranslatingExpressionVisitorFactory StartRecording()
        {
            var factory = (RecordingSqlTranslatingExpressionVisitorFactory)this.GetService<IRelationalSqlTranslatingExpressionVisitorFactory>();
            factory.Translated.Clear();
            factory.TranslatedProjections.Clear();

            return factory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer("Server=localhost;Database=TranslatorOverrideTest;Trusted_Connection=True")
                .ReplaceService<IRelationalSqlTranslatingExpressionVisitorFactory, RecordingSqlTranslatingExpressionVisitorFactory>();
    }

    private sealed class OverrideEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    private sealed class RecordingSqlTranslatingExpressionVisitorFactory(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
        ISqlServerSingletonOptions sqlServerSingletonOptions)
        : IRelationalSqlTranslatingExpressionVisitorFactory
    {
        public List<Expression> Translated { get; } = [];
        public List<Expression> TranslatedProjections { get; } = [];

        public RelationalSqlTranslatingExpressionVisitor Create(
            QueryCompilationContext queryCompilationContext,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
            => new RecordingSqlTranslatingExpressionVisitor(
                this,
                dependencies,
                (SqlServerQueryCompilationContext)queryCompilationContext,
                queryableMethodTranslatingExpressionVisitor,
                sqlServerSingletonOptions);
    }

    private sealed class RecordingSqlTranslatingExpressionVisitor(
        RecordingSqlTranslatingExpressionVisitorFactory factory,
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
        SqlServerQueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
        ISqlServerSingletonOptions sqlServerSingletonOptions)
        : SqlServerSqlTranslatingExpressionVisitor(
            dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor, sqlServerSingletonOptions)
    {
        public override SqlExpression? Translate(Expression expression, bool applyDefaultTypeMapping = true)
        {
            factory.Translated.Add(expression);

            return base.Translate(expression, applyDefaultTypeMapping);
        }

        public override Expression? TranslateProjection(Expression expression, bool applyDefaultTypeMapping = true)
        {
            factory.TranslatedProjections.Add(expression);

            return base.TranslateProjection(expression, applyDefaultTypeMapping);
        }
    }
}
