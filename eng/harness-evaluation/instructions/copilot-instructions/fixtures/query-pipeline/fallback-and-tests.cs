static Expression CreateChildCountAccessor(
    SelectExpression parentSelectExpression,
    List<Expression> clientProjectionList,
    SelectExpression childCountSelectExpression,
    ISqlExpressionFactory sqlExpressionFactory,
    IAggregateMethodCallTranslatorProvider aggregateMethodCallTranslatorProvider,
    QueryCompilationContext queryCompilationContext)
{
    try
    {
        if (childCountSelectExpression.Limit != null
            || childCountSelectExpression.Offset != null
            || childCountSelectExpression.IsDistinct
            || childCountSelectExpression.GroupBy.Count > 0)
        {
            childCountSelectExpression.PushdownIntoSubqueryInternal();
        }

        childCountSelectExpression.ClearOrdering();
        var countTranslation = aggregateMethodCallTranslatorProvider.Translate(
            queryCompilationContext.Model,
            QueryableMethods.CountWithoutPredicate,
            new EnumerableExpression(sqlExpressionFactory.Fragment("*")),
            arguments: [],
            queryCompilationContext.Logger)
            ?? throw new UnreachableException("The Count aggregate method must always be translatable.");
        childCountSelectExpression.ReplaceProjection(
            new List<Expression> { sqlExpressionFactory.ApplyDefaultTypeMapping(countTranslation) });
        childCountSelectExpression.ApplyProjection();
    }
    catch
    {
        return Constant(-1);
    }

    var countIndex = parentSelectExpression.AddToProjection(
        new ScalarSubqueryExpression(childCountSelectExpression));
    return new ProjectionBindingExpression(parentSelectExpression, countIndex, typeof(int));
}

public abstract class AdHocQuerySplittingQueryTestBase
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Split_include_collection_not_dropped_when_unrelated_entity_inserted_concurrently(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context33826>();
        Context33826.ConcurrentContextFactory = contextFactory.CreateDbContext;
        try
        {
            Context33826.InsertConcurrentEntity = true;
            using var context = contextFactory.CreateDbContext();
            var query = context.Blogs.Include(b => b.Posts).AsSplitQuery();
            var blogs = async ? await query.ToListAsync() : query.ToList();
            Assert.Equal(2, blogs.Count);
        }
        finally
        {
            Context33826.InsertConcurrentEntity = false;
            Context33826.ConcurrentContextFactory = null;
        }
    }

    protected class Context33826(DbContextOptions options) : DbContext(options)
    {
        public static Func<Context33826> ConcurrentContextFactory { get; set; }
        public static bool InsertConcurrentEntity { get; set; }
        public static bool DeleteOtherParentsChildren { get; set; }

        private Blog33826(int id, int secondId)
        {
            if (id == 20 && secondId == 1 && Context33826.InsertConcurrentEntity)
            {
                Context33826.InsertConcurrentEntity = false;
                using var context = Context33826.ConcurrentContextFactory();
                context.Blogs.Add(new Blog33826(15, 3, []));
                context.SaveChanges();
            }
        }
    }
}