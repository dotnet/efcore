// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocNavigationsQuerySqlServerTest : AdHocNavigationsQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    #region 10447

    [ConditionalFact]
    public virtual async Task Nested_include_queries_do_not_populate_navigation_twice()
    {
        var contextFactory = await InitializeAsync<Context10447>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var query = context.Blogs.Include(b => b.Posts);

        foreach (var blog in query)
        {
            query.ToList();
        }

        Assert.Collection(
            query,
            b => Assert.Equal(3, b.Posts.Count),
            b => Assert.Equal(2, b.Posts.Count),
            b => Assert.Single(b.Posts));

        AssertSql(
            """
SELECT [b].[Id], [p].[Id], [p].[BlogId]
FROM [Blogs] AS [b]
LEFT JOIN [Post] AS [p] ON [b].[Id] = [p].[BlogId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b].[Id], [p].[Id], [p].[BlogId]
FROM [Blogs] AS [b]
LEFT JOIN [Post] AS [p] ON [b].[Id] = [p].[BlogId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b].[Id], [p].[Id], [p].[BlogId]
FROM [Blogs] AS [b]
LEFT JOIN [Post] AS [p] ON [b].[Id] = [p].[BlogId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b].[Id], [p].[Id], [p].[BlogId]
FROM [Blogs] AS [b]
LEFT JOIN [Post] AS [p] ON [b].[Id] = [p].[BlogId]
ORDER BY [b].[Id]
""",
            //
            """
SELECT [b].[Id], [p].[Id], [p].[BlogId]
FROM [Blogs] AS [b]
LEFT JOIN [Post] AS [p] ON [b].[Id] = [p].[BlogId]
ORDER BY [b].[Id]
""");
    }

    protected class Context10447(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public Task SeedAsync()
        {
            AddRange(
                new Blog
                {
                    Posts =
                    [
                        new(),
                        new(),
                        new()
                    ]
                },
                new Blog { Posts = [new(), new()] },
                new Blog { Posts = [new()] });

            return SaveChangesAsync();
        }

        public class Blog
        {
            public int Id { get; set; }
            public List<Post> Posts { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }

            public Blog Blog { get; set; }
        }
    }

    #endregion

    public override async Task ThenInclude_with_interface_navigations()
    {
        await base.ThenInclude_with_interface_navigations();

        AssertSql(
            """
SELECT [p].[Id], [s].[Id], [s].[ParentBackNavigationId], [s].[SelfReferenceBackNavigationId], [s].[Id0], [s].[ParentBackNavigationId0], [s].[SelfReferenceBackNavigationId0]
FROM [Parents] AS [p]
LEFT JOIN (
    SELECT [c].[Id], [c].[ParentBackNavigationId], [c].[SelfReferenceBackNavigationId], [c0].[Id] AS [Id0], [c0].[ParentBackNavigationId] AS [ParentBackNavigationId0], [c0].[SelfReferenceBackNavigationId] AS [SelfReferenceBackNavigationId0]
    FROM [Children] AS [c]
    LEFT JOIN [Children] AS [c0] ON [c].[Id] = [c0].[SelfReferenceBackNavigationId]
) AS [s] ON [p].[Id] = [s].[ParentBackNavigationId]
ORDER BY [p].[Id], [s].[Id]
""",
            //
            """
SELECT [c0].[Id], [c0].[ParentBackNavigationId], [c0].[SelfReferenceBackNavigationId], [p].[Id]
FROM [Children] AS [c]
LEFT JOIN [Children] AS [c0] ON [c].[SelfReferenceBackNavigationId] = [c0].[Id]
LEFT JOIN [Parents] AS [p] ON [c0].[ParentBackNavigationId] = [p].[Id]
""",
            //
            """
SELECT [c0].[Id], [c0].[ParentBackNavigationId], [c0].[SelfReferenceBackNavigationId], [p].[Id]
FROM [Children] AS [c]
LEFT JOIN [Children] AS [c0] ON [c].[SelfReferenceBackNavigationId] = [c0].[Id]
LEFT JOIN [Parents] AS [p] ON [c0].[ParentBackNavigationId] = [p].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[ParentBackNavigationId], [c].[SelfReferenceBackNavigationId], [c0].[Id], [c0].[ParentBackNavigationId], [c0].[SelfReferenceBackNavigationId], [p].[Id]
FROM [Children] AS [c]
LEFT JOIN [Children] AS [c0] ON [c].[SelfReferenceBackNavigationId] = [c0].[Id]
LEFT JOIN [Parents] AS [p] ON [c0].[ParentBackNavigationId] = [p].[Id]
""");
    }

    public override async Task Customer_collections_materialize_properly()
    {
        await base.Customer_collections_materialize_properly();

        AssertSql(
            """
SELECT [c].[Id], [o].[Id], [o].[CustomerId1], [o].[CustomerId2], [o].[CustomerId3], [o].[CustomerId4], [o].[Name]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[Id] = [o].[CustomerId1]
ORDER BY [c].[Id]
""",
            //
            """
SELECT [c].[Id], [o].[Id], [o].[CustomerId1], [o].[CustomerId2], [o].[CustomerId3], [o].[CustomerId4], [o].[Name]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[Id] = [o].[CustomerId2]
ORDER BY [c].[Id]
""",
            //
            """
SELECT [c].[Id], [o].[Id], [o].[CustomerId1], [o].[CustomerId2], [o].[CustomerId3], [o].[CustomerId4], [o].[Name]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[Id] = [o].[CustomerId3]
ORDER BY [c].[Id]
""",
            //
            """
SELECT [c].[Id], [o].[Id], [o].[CustomerId1], [o].[CustomerId2], [o].[CustomerId3], [o].[CustomerId4], [o].[Name]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[Id] = [o].[CustomerId4]
ORDER BY [c].[Id]
""");
    }

    public override async Task Reference_include_on_derived_type_with_sibling_works()
    {
        await base.Reference_include_on_derived_type_with_sibling_works();

        AssertSql(
            """
SELECT [p].[Id], [p].[Discriminator], [p].[LeaveStart], [p].[LeaveTypeId], [p0].[Id]
FROM [Proposals] AS [p]
LEFT JOIN [ProposalLeaveType] AS [p0] ON [p].[LeaveTypeId] = [p0].[Id]
WHERE [p].[Discriminator] = N'ProposalLeave'
""");
    }

    public override async Task Include_collection_optional_reference_collection()
    {
        await base.Include_collection_optional_reference_collection();

        AssertSql(
            """
SELECT [p].[Id], [p].[Discriminator], [p].[FamilyId], [p].[Name], [p].[TeacherId], [s].[Id], [s].[Discriminator], [s].[FamilyId], [s].[Name], [s].[TeacherId], [s].[Grade], [s].[Id0], [s].[LastName], [s].[Id1], [s].[Discriminator0], [s].[FamilyId0], [s].[Name0], [s].[TeacherId0], [s].[Grade0]
FROM [People] AS [p]
LEFT JOIN (
    SELECT [p0].[Id], [p0].[Discriminator], [p0].[FamilyId], [p0].[Name], [p0].[TeacherId], [p0].[Grade], [f].[Id] AS [Id0], [f].[LastName], [p1].[Id] AS [Id1], [p1].[Discriminator] AS [Discriminator0], [p1].[FamilyId] AS [FamilyId0], [p1].[Name] AS [Name0], [p1].[TeacherId] AS [TeacherId0], [p1].[Grade] AS [Grade0]
    FROM [People] AS [p0]
    LEFT JOIN [Families] AS [f] ON [p0].[FamilyId] = [f].[Id]
    LEFT JOIN [People] AS [p1] ON [f].[Id] = [p1].[FamilyId]
    WHERE [p0].[Discriminator] = N'PersonKid9038'
) AS [s] ON [p].[Id] = [s].[TeacherId]
WHERE [p].[Discriminator] = N'PersonTeacher9038'
ORDER BY [p].[Id], [s].[Id], [s].[Id0]
""",
            //
            """
SELECT [p].[Id], [p].[Discriminator], [p].[FamilyId], [p].[Name], [p].[TeacherId], [f].[Id], [f].[LastName], [p0].[Id], [p0].[Discriminator], [p0].[FamilyId], [p0].[Name], [p0].[TeacherId], [p0].[Grade], [p2].[Id], [p2].[Discriminator], [p2].[FamilyId], [p2].[Name], [p2].[TeacherId], [p2].[Grade]
FROM [People] AS [p]
LEFT JOIN [Families] AS [f] ON [p].[FamilyId] = [f].[Id]
LEFT JOIN [People] AS [p0] ON [f].[Id] = [p0].[FamilyId]
LEFT JOIN (
    SELECT [p1].[Id], [p1].[Discriminator], [p1].[FamilyId], [p1].[Name], [p1].[TeacherId], [p1].[Grade]
    FROM [People] AS [p1]
    WHERE [p1].[Discriminator] = N'PersonKid9038'
) AS [p2] ON [p].[Id] = [p2].[TeacherId]
WHERE [p].[Discriminator] = N'PersonTeacher9038'
ORDER BY [p].[Id], [f].[Id], [p0].[Id]
""");
    }

    public override async Task Include_with_order_by_on_interface_key()
    {
        await base.Include_with_order_by_on_interface_key();

        AssertSql(
            """
SELECT [p].[Id], [p].[Name], [c].[Id], [c].[Name], [c].[Parent10635Id], [c].[ParentId]
FROM [Parents] AS [p]
LEFT JOIN [Children] AS [c] ON [p].[Id] = [c].[Parent10635Id]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [p].[Id], [c].[Id], [c].[Name], [c].[Parent10635Id], [c].[ParentId]
FROM [Parents] AS [p]
LEFT JOIN [Children] AS [c] ON [p].[Id] = [c].[Parent10635Id]
ORDER BY [p].[Id]
""");
    }

    public override async Task Collection_without_setter_materialized_correctly()
    {
        await base.Collection_without_setter_materialized_correctly();

        AssertSql(
            """
SELECT [b].[Id], [p].[Id], [p].[BlogId1], [p].[BlogId2], [p].[BlogId3], [p].[Name], [p0].[Id], [p0].[BlogId1], [p0].[BlogId2], [p0].[BlogId3], [p0].[Name], [p1].[Id], [p1].[BlogId1], [p1].[BlogId2], [p1].[BlogId3], [p1].[Name]
FROM [Blogs] AS [b]
LEFT JOIN [Posts] AS [p] ON [b].[Id] = [p].[BlogId1]
LEFT JOIN [Posts] AS [p0] ON [b].[Id] = [p0].[BlogId2]
LEFT JOIN [Posts] AS [p1] ON [b].[Id] = [p1].[BlogId3]
ORDER BY [b].[Id], [p].[Id], [p0].[Id]
""",
            //
            """
SELECT (
    SELECT TOP(1) (
        SELECT COUNT(*)
        FROM [Comments] AS [c]
        WHERE [p].[Id] = [c].[PostId])
    FROM [Posts] AS [p]
    WHERE [b].[Id] = [p].[BlogId1]
    ORDER BY [p].[Id]) AS [Collection1], (
    SELECT TOP(1) (
        SELECT COUNT(*)
        FROM [Comments] AS [c0]
        WHERE [p0].[Id] = [c0].[PostId])
    FROM [Posts] AS [p0]
    WHERE [b].[Id] = [p0].[BlogId2]
    ORDER BY [p0].[Id]) AS [Collection2], (
    SELECT TOP(1) (
        SELECT COUNT(*)
        FROM [Comments] AS [c1]
        WHERE [p1].[Id] = [c1].[PostId])
    FROM [Posts] AS [p1]
    WHERE [b].[Id] = [p1].[BlogId3]
    ORDER BY [p1].[Id]) AS [Collection3]
FROM [Blogs] AS [b]
""");
    }

    public override async Task Include_collection_works_when_defined_on_intermediate_type()
    {
        await base.Include_collection_works_when_defined_on_intermediate_type();

        AssertSql(
            """
SELECT [s].[Id], [s].[Discriminator], [s0].[Id], [s0].[SchoolId]
FROM [Schools] AS [s]
LEFT JOIN [Students] AS [s0] ON [s].[Id] = [s0].[SchoolId]
ORDER BY [s].[Id]
""",
            //
            """
SELECT [s].[Id], [s0].[Id], [s0].[SchoolId]
FROM [Schools] AS [s]
LEFT JOIN [Students] AS [s0] ON [s].[Id] = [s0].[SchoolId]
ORDER BY [s].[Id]
""");
    }

    public override async Task Let_multiple_references_with_reference_to_outer()
    {
        await base.Let_multiple_references_with_reference_to_outer();

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [c].[Id]
    FROM [CompetitionSeasons] AS [c]
    WHERE [c].[StartDate] <= [a].[DateTime] AND [a].[DateTime] < [c].[EndDate]), [a].[Id], [a0].[Id], [s].[Id], [s].[ActivityTypeId], [s].[CompetitionSeasonId], [s].[Points], [s].[Id0]
FROM [Activities] AS [a]
INNER JOIN [ActivityType] AS [a0] ON [a].[ActivityTypeId] = [a0].[Id]
OUTER APPLY (
    SELECT [a1].[Id], [a1].[ActivityTypeId], [a1].[CompetitionSeasonId], [a1].[Points], [c0].[Id] AS [Id0]
    FROM [ActivityTypePoints] AS [a1]
    INNER JOIN [CompetitionSeasons] AS [c0] ON [a1].[CompetitionSeasonId] = [c0].[Id]
    WHERE [a0].[Id] = [a1].[ActivityTypeId] AND [c0].[Id] = (
        SELECT TOP(1) [c1].[Id]
        FROM [CompetitionSeasons] AS [c1]
        WHERE [c1].[StartDate] <= [a].[DateTime] AND [a].[DateTime] < [c1].[EndDate])
) AS [s]
ORDER BY [a].[Id], [a0].[Id], [s].[Id]
""",
            //
            """
SELECT [a].[Id], [a].[ActivityTypeId], [a].[DateTime], [a].[Points], (
    SELECT TOP(1) [c].[Id]
    FROM [CompetitionSeasons] AS [c]
    WHERE [c].[StartDate] <= [a].[DateTime] AND [a].[DateTime] < [c].[EndDate]) AS [CompetitionSeasonId], COALESCE([a].[Points], (
    SELECT TOP(1) [a1].[Points]
    FROM [ActivityTypePoints] AS [a1]
    INNER JOIN [CompetitionSeasons] AS [c0] ON [a1].[CompetitionSeasonId] = [c0].[Id]
    WHERE [a0].[Id] = [a1].[ActivityTypeId] AND [c0].[Id] = (
        SELECT TOP(1) [c1].[Id]
        FROM [CompetitionSeasons] AS [c1]
        WHERE [c1].[StartDate] <= [a].[DateTime] AND [a].[DateTime] < [c1].[EndDate])), 0) AS [Points]
FROM [Activities] AS [a]
INNER JOIN [ActivityType] AS [a0] ON [a].[ActivityTypeId] = [a0].[Id]
""");
    }

    public override async Task Include_collection_with_OfType_base()
    {
        await base.Include_collection_with_OfType_base();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [d].[Id], [d].[Device], [d].[EmployeeId]
FROM [Employees] AS [e]
LEFT JOIN [Devices] AS [d] ON [e].[Id] = [d].[EmployeeId]
ORDER BY [e].[Id]
""",
            //
            """
SELECT [e].[Id], [d0].[Id], [d0].[Device], [d0].[EmployeeId]
FROM [Employees] AS [e]
LEFT JOIN (
    SELECT [d].[Id], [d].[Device], [d].[EmployeeId]
    FROM [Devices] AS [d]
    WHERE [d].[Device] <> N'foo' OR [d].[Device] IS NULL
) AS [d0] ON [e].[Id] = [d0].[EmployeeId]
ORDER BY [e].[Id]
""");
    }

    public override async Task Correlated_collection_correctly_associates_entities_with_byte_array_keys()
    {
        await base.Correlated_collection_correctly_associates_entities_with_byte_array_keys();

        AssertSql(
            """
SELECT [b].[Name], [c].[Id]
FROM [Blogs] AS [b]
LEFT JOIN [Comments] AS [c] ON [b].[Name] = [c].[BlogName]
ORDER BY [b].[Name]
""");
    }

    public override async Task Can_ignore_invalid_include_path_error()
    {
        await base.Can_ignore_invalid_include_path_error();

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[SubAId]
FROM [BaseClasses] AS [b]
WHERE [b].[Discriminator] = N'ClassA'
""");
    }

    public override async Task SelectMany_and_collection_in_projection_in_FirstOrDefault()
    {
        await base.SelectMany_and_collection_in_projection_in_FirstOrDefault();

        AssertSql(
            """
@__referenceId_0='a' (Size = 4000)
@__customerId_1='1115c816-6c4c-4016-94df-d8b60a22ffa1'

SELECT [o0].[Id], [s0].[Id], [s0].[Image], [s0].[Id0], [s0].[Id00]
FROM (
    SELECT TOP(2) [o].[Id]
    FROM [Orders] AS [o]
    WHERE [o].[ExternalReferenceId] = @__referenceId_0 AND [o].[CustomerId] = @__customerId_1
) AS [o0]
OUTER APPLY (
    SELECT [i].[Id], [s].[Image], [s].[Id] AS [Id0], [s].[Id0] AS [Id00]
    FROM [IdentityDocument] AS [i]
    OUTER APPLY (
        SELECT [i1].[Image], [i0].[Id], [i1].[Id] AS [Id0]
        FROM [IdentityDocument] AS [i0]
        INNER JOIN [IdentityDocumentImage] AS [i1] ON [i0].[Id] = [i1].[IdentityDocumentId]
        WHERE [o0].[Id] = [i0].[OrderId]
    ) AS [s]
    WHERE [o0].[Id] = [i].[OrderId]
) AS [s0]
ORDER BY [o0].[Id], [s0].[Id], [s0].[Id0]
""");
    }

    public override async Task Using_explicit_interface_implementation_as_navigation_works()
    {
        await base.Using_explicit_interface_implementation_as_navigation_works();

        AssertSql(
            """
SELECT TOP(2) CASE
    WHEN EXISTS (
        SELECT 1
        FROM [CoverIllustrations] AS [c]
        WHERE [b0].[Id] = [c].[CoverId] AND [c].[State] >= 2) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, (
    SELECT TOP(1) [c0].[Uri]
    FROM [CoverIllustrations] AS [c0]
    WHERE [b0].[Id] = [c0].[CoverId] AND [c0].[State] >= 2)
FROM [Books] AS [b]
INNER JOIN [BookCovers] AS [b0] ON [b].[FrontCoverId] = [b0].[Id]
WHERE [b].[Id] = 1
""");
    }

    public override async Task Select_enumerable_navigation_backed_by_collection(bool async, bool split)
    {
        await base.Select_enumerable_navigation_backed_by_collection(async, split);

        if (split)
        {
            AssertSql(
                """
SELECT [e].[Id]
FROM [Entities] AS [e]
ORDER BY [e].[Id]
""",
                //
                """
SELECT [o].[Id], [o].[AppEntityId], [e].[Id]
FROM [Entities] AS [e]
INNER JOIN [OtherEntity] AS [o] ON [e].[Id] = [o].[AppEntityId]
ORDER BY [e].[Id]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [e].[Id], [o].[Id], [o].[AppEntityId]
FROM [Entities] AS [e]
LEFT JOIN [OtherEntity] AS [o] ON [e].[Id] = [o].[AppEntityId]
ORDER BY [e].[Id]
""");
        }
    }

    public override async Task Cycles_in_auto_include()
    {
        await base.Cycles_in_auto_include();

        AssertSql(
            """
SELECT [p].[Id], [d].[Id], [d].[PrincipalId]
FROM [PrincipalOneToOne] AS [p]
LEFT JOIN [DependentOneToOne] AS [d] ON [p].[Id] = [d].[PrincipalId]
""",
            //
            """
SELECT [d].[Id], [d].[PrincipalId], [p].[Id]
FROM [DependentOneToOne] AS [d]
INNER JOIN [PrincipalOneToOne] AS [p] ON [d].[PrincipalId] = [p].[Id]
""",
            //
            """
SELECT [p].[Id], [d].[Id], [d].[PrincipalId]
FROM [PrincipalOneToMany] AS [p]
LEFT JOIN [DependentOneToMany] AS [d] ON [p].[Id] = [d].[PrincipalId]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [d].[Id], [d].[PrincipalId], [p].[Id], [d0].[Id], [d0].[PrincipalId]
FROM [DependentOneToMany] AS [d]
INNER JOIN [PrincipalOneToMany] AS [p] ON [d].[PrincipalId] = [p].[Id]
LEFT JOIN [DependentOneToMany] AS [d0] ON [p].[Id] = [d0].[PrincipalId]
ORDER BY [d].[Id], [p].[Id]
""",
            //
            """
SELECT [p].[Id]
FROM [PrincipalManyToMany] AS [p]
""",
            //
            """
SELECT [d].[Id]
FROM [DependentManyToMany] AS [d]
""",
            //
            """
SELECT [c].[Id], [c].[CycleCId]
FROM [CycleA] AS [c]
""",
            //
            """
SELECT [c].[Id], [c].[CId], [c].[CycleAId]
FROM [CycleB] AS [c]
""",
            //
            """
SELECT [c].[Id], [c].[BId]
FROM [CycleC] AS [c]
""");
    }

    public override async Task Walking_back_include_tree_is_not_allowed_1()
    {
        await base.Walking_back_include_tree_is_not_allowed_1();

        AssertSql();
    }

    public override async Task Walking_back_include_tree_is_not_allowed_2()
    {
        await base.Walking_back_include_tree_is_not_allowed_2();

        AssertSql();
    }

    public override async Task Walking_back_include_tree_is_not_allowed_3()
    {
        await base.Walking_back_include_tree_is_not_allowed_3();

        AssertSql(
            """
SELECT [m].[Id], [m].[PrincipalId], [p].[Id], [s0].[Id], [s0].[PrincipalId], [s0].[Id0], [s0].[ManyDependentId], [s0].[PrincipalId0]
FROM [ManyDependent] AS [m]
LEFT JOIN [Principal] AS [p] ON [m].[PrincipalId] = [p].[Id]
LEFT JOIN (
    SELECT [m0].[Id], [m0].[PrincipalId], [s].[Id] AS [Id0], [s].[ManyDependentId], [s].[PrincipalId] AS [PrincipalId0]
    FROM [ManyDependent] AS [m0]
    LEFT JOIN [SingleDependent] AS [s] ON [m0].[Id] = [s].[ManyDependentId]
) AS [s0] ON [p].[Id] = [s0].[PrincipalId]
ORDER BY [m].[Id], [p].[Id], [s0].[Id]
""");
    }

    public override async Task Walking_back_include_tree_is_not_allowed_4()
    {
        await base.Walking_back_include_tree_is_not_allowed_4();

        AssertSql();
    }

    public override async Task Projection_with_multiple_includes_and_subquery_with_set_operation()
    {
        await base.Projection_with_multiple_includes_and_subquery_with_set_operation();

        AssertSql(
            """
@__id_0='1'

SELECT [s].[Id], [s].[Name], [s].[Surname], [s].[Birthday], [s].[Hometown], [s].[Bio], [s].[AvatarUrl], [s].[Id0], [s].[Id1], [p0].[Id], [p0].[ImageUrl], [p0].[Height], [p0].[Width], [u].[Id], [u].[Name], [u].[PosterUrl], [u].[Rating]
FROM (
    SELECT TOP(1) [p].[Id], [p].[Name], [p].[Surname], [p].[Birthday], [p].[Hometown], [p].[Bio], [p].[AvatarUrl], [a].[Id] AS [Id0], [d].[Id] AS [Id1]
    FROM [Persons] AS [p]
    LEFT JOIN [ActorEntity] AS [a] ON [p].[Id] = [a].[PersonId]
    LEFT JOIN [DirectorEntity] AS [d] ON [p].[Id] = [d].[PersonId]
    WHERE [p].[Id] = @__id_0
) AS [s]
LEFT JOIN [PersonImageEntity] AS [p0] ON [s].[Id] = [p0].[PersonId]
OUTER APPLY (
    SELECT [m0].[Id], [m0].[Budget], [m0].[Description], [m0].[DurationInMins], [m0].[Name], [m0].[PosterUrl], [m0].[Rating], [m0].[ReleaseDate], [m0].[Revenue]
    FROM [MovieActorEntity] AS [m]
    INNER JOIN [MovieEntity] AS [m0] ON [m].[MovieId] = [m0].[Id]
    WHERE [s].[Id0] IS NOT NULL AND [s].[Id0] = [m].[ActorId]
    UNION
    SELECT [m2].[Id], [m2].[Budget], [m2].[Description], [m2].[DurationInMins], [m2].[Name], [m2].[PosterUrl], [m2].[Rating], [m2].[ReleaseDate], [m2].[Revenue]
    FROM [MovieDirectorEntity] AS [m1]
    INNER JOIN [MovieEntity] AS [m2] ON [m1].[MovieId] = [m2].[Id]
    WHERE [s].[Id1] IS NOT NULL AND [s].[Id1] = [m1].[DirectorId]
) AS [u]
ORDER BY [s].[Id], [s].[Id0], [s].[Id1], [p0].[Id]
""");
    }

    public override async Task Count_member_over_IReadOnlyCollection_works(bool async)
    {
        await base.Count_member_over_IReadOnlyCollection_works(async);

        AssertSql(
            """
SELECT (
    SELECT COUNT(*)
    FROM [Books] AS [b]
    WHERE [a].[AuthorId] = [b].[AuthorId]) AS [BooksCount]
FROM [Authors] AS [a]
""");
    }
}
