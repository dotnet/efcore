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
        var contextFactory = await InitializeNonSharedTest<Context33826>(seed: context => context.SeedAsync());
        Context33826.ConcurrentContextFactory = contextFactory.CreateDbContext;
        try
        {
            Context33826.InsertConcurrentEntity = true;
            using var context = contextFactory.CreateDbContext();
            var query = context.Blogs.OrderBy(blog => blog.Id).ThenBy(blog => blog.SecondId)
                .Include(blog => blog.Posts).AsSplitQuery();
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
        public static Func<Context33826>? ConcurrentContextFactory { get; set; }
        public static bool InsertConcurrentEntity { get; set; }

        public DbSet<Blog33826> Blogs => Set<Blog33826>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog33826>(builder =>
            {
                builder.HasKey(blog => new { blog.Id, blog.SecondId });
                builder.Property(blog => blog.Id).ValueGeneratedNever();
                builder.Property(blog => blog.SecondId).ValueGeneratedNever();
                builder.HasMany(blog => blog.Posts).WithOne()
                    .HasForeignKey(post => new { post.BlogId, post.BlogSecondId });
            });
            modelBuilder.Entity<Post33826>(builder =>
            {
                builder.HasKey(post => new { post.Id, post.SecondId });
                builder.Property(post => post.Id).ValueGeneratedNever();
                builder.Property(post => post.SecondId).ValueGeneratedNever();
            });
        }

        public Task SeedAsync()
        {
            Blogs.AddRange(
                new Blog33826(10, 2, [new Post33826(1, 2, "A"), new Post33826(2, 2, "B")]),
                new Blog33826(20, 1, [new Post33826(3, 1, "C"), new Post33826(4, 1, "D")]));
            return SaveChangesAsync();
        }
    }

    protected class Blog33826
    {
        public Blog33826(int id, int secondId, IEnumerable<Post33826> posts)
        {
            Id = id;
            SecondId = secondId;
            Posts = posts.ToList();
        }

        private Blog33826(int id, int secondId)
        {
            Id = id;
            SecondId = secondId;
            if (id == 20 && secondId == 1 && Context33826.InsertConcurrentEntity)
            {
                Context33826.InsertConcurrentEntity = false;
                using var context = Context33826.ConcurrentContextFactory!();
                context.Blogs.Add(new Blog33826(15, 3, [new Post33826(5, 3, "Concurrent")]));
                context.SaveChanges();
            }
        }

        public int Id { get; private init; }
        public int SecondId { get; private init; }
        public List<Post33826> Posts { get; private init; } = [];
    }

    protected class Post33826(int id, int secondId, string name)
    {
        public int Id { get; private init; } = id;
        public int SecondId { get; private init; } = secondId;
        public string Name { get; private init; } = name;
        public int BlogId { get; private set; }
        public int BlogSecondId { get; private set; }
    }
}