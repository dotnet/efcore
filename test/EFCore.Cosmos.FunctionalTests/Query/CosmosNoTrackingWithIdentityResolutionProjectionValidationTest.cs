// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class CosmosNoTrackingWithIdentityResolutionProjectionTest
    : QueryTestBase<CosmosNoTrackingWithIdentityResolutionProjectionTest.ProjectionFixture>
{
    public CosmosNoTrackingWithIdentityResolutionProjectionTest(ProjectionFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public void Owned_reference_without_owner_key_throws()
    {
        using var context = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => Translate(
                context,
                context.Owners
                    .Select(owner => owner.OwnedReference)
                    .AsNoTrackingWithIdentityResolution()));

        AssertMissingOwnerKeyMessage(exception);
    }

    [ConditionalFact]
    public void Owned_collection_without_owner_key_throws()
    {
        using var context = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => Translate(
                context,
                context.Owners
                    .Select(owner => owner.OwnedCollection)
                    .AsNoTrackingWithIdentityResolution()));

        AssertMissingOwnerKeyMessage(exception);
    }

    [ConditionalFact]
    public void SelectMany_owned_collection_without_owner_key_throws()
    {
        using var context = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => Translate(
                context,
                context.Owners
                    .SelectMany(owner => owner.OwnedCollection)
                    .AsNoTrackingWithIdentityResolution()));

        AssertMissingOwnerKeyMessage(exception);
    }

    [ConditionalFact]
    public void Root_entity_projection_with_owned_reference_without_owner_key_throws()
    {
        using var context = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => Translate(
                context,
                context.Owners
                    .Select(owner => new { Owner = owner, owner.OwnedReference })
                    .AsNoTrackingWithIdentityResolution()));

        AssertMissingOwnerKeyMessage(exception);
    }

    [ConditionalFact]
    public void SelectMany_owned_collection_with_owner_key_throws()
    {
        using var context = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => Translate(
                context,
                context.Owners
                    .SelectMany(e => e.OwnedCollection.Select(o => new { e.Id, o }))
                    .AsNoTrackingWithIdentityResolution()));

        Assert.Equal(CosmosStrings.ComplexProjectionInSubqueryNotSupported, exception.Message);
    }

    [ConditionalFact]
    public void SelectMany_owned_collection_throws()
    {
        using var context = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => Translate(
                context,
                context.Owners
                    .SelectMany(x => x.OwnedCollection)
                    .AsNoTrackingWithIdentityResolution()));

        AssertMissingOwnerKeyMessage(exception);
    }

    [ConditionalFact]
    public async Task Owned_reference_with_owner_key_succeeds()
    {
        await AssertQuery(
            ss => ss.Set<Owner>()
                .Select(owner => new { owner.Id, owner.OwnedReference })
                .AsNoTrackingWithIdentityResolution(),
            elementSorter: result => result.Id,
            elementAsserter: (expected, actual) =>
            {
                Assert.Equal(expected.Id, actual.Id);
                AssertOwnedReference(expected.OwnedReference, actual.OwnedReference);
            });

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["id"],
    "OwnedReference" : c["OwnedReference"]
}
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Owned_collection_with_owner_key_succeeds()
    {
        await AssertQuery(
            ss => ss.Set<Owner>()
                .Select(owner => new { owner.Id, owner.OwnedCollection })
                .AsNoTrackingWithIdentityResolution(),
            elementSorter: result => result.Id,
            elementAsserter: (expected, actual) =>
            {
                Assert.Equal(expected.Id, actual.Id);
                AssertOwnedCollection(expected.OwnedCollection, actual.OwnedCollection);
            });

        AssertSql(
            """
SELECT c["id"], c["OwnedCollection"]
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Owned_reference_with_owner_key_after_owned_reference_projects_owner_key_first()
    {
        await AssertQuery(
            ss => ss.Set<Owner>()
                .Select(owner => new { owner.OwnedReference, owner.Id })
                .AsNoTrackingWithIdentityResolution(),
            elementSorter: result => result.Id,
            elementAsserter: (expected, actual) =>
            {
                Assert.Equal(expected.Id, actual.Id);
                AssertOwnedReference(expected.OwnedReference, actual.OwnedReference);
            });

        AssertSql(
            """
SELECT VALUE
{
    "Id" : c["id"],
    "OwnedReference" : c["OwnedReference"]
}
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Owned_collection_with_owner_key_after_owned_collection_projects_owner_key_first()
    {
        await AssertQuery(
            ss => ss.Set<Owner>()
                .Select(owner => new { owner.OwnedCollection, owner.Id })
                .AsNoTrackingWithIdentityResolution(),
            elementSorter: result => result.Id,
            elementAsserter: (expected, actual) =>
            {
                Assert.Equal(expected.Id, actual.Id);
                AssertOwnedCollection(expected.OwnedCollection, actual.OwnedCollection);
            });

        AssertSql(
            """
SELECT c["id"], c["OwnedCollection"]
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Complex_reference_without_owner_key_succeeds()
    {
        await AssertQuery(
            ss => ss.Set<Owner>()
                .Select(owner => owner.ComplexReference)
                .AsNoTrackingWithIdentityResolution(),
            elementAsserter: (expected, actual) => Assert.Equal(expected.Name, actual.Name));

        AssertSql(
            """
SELECT VALUE c["ComplexReference"]
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Complex_collection_without_owner_key_succeeds()
    {
        await AssertQuery(
            ss => ss.Set<Owner>()
                .Select(owner => owner.ComplexCollection)
                .AsNoTrackingWithIdentityResolution(),
            elementAsserter: (expected, actual) => AssertComplexCollection(expected, actual));

        AssertSql(
            """
SELECT VALUE c["ComplexCollection"]
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Scalar_projection_without_owner_key_succeeds()
    {
        await AssertQuery(
            ss => ss.Set<Owner>()
                .Select(owner => owner.Name)
                .AsNoTrackingWithIdentityResolution());

        AssertSql(
            """
SELECT VALUE c["Name"]
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Root_entity_projection_succeeds()
    {
        await AssertQuery(
            ss => ss.Set<Owner>().AsNoTrackingWithIdentityResolution(),
            elementSorter: owner => owner.Id,
            elementAsserter: AssertOwner);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    [ConditionalFact]
    public async Task AssociateAsNoTrackingWithIdentityResolution()
    {
        using var context = CreateContext();

        var results = (await context.IdentityResolutionEntities.AsNoTrackingWithIdentityResolution()
            .Select(x => new { x.Id, x.RequiredAssociate })
            .ToListAsync()).OrderBy(x => x.Id).ToList();

        Assert.Equal(2, results.Count);
        Assert.NotSame(results[0].RequiredAssociate, results[1].RequiredAssociate);
    }

    [ConditionalFact]
    public async Task DoubleAssociateAsNoTrackingWithIdentityResolution()
    {
        using var context = CreateContext();

        var results = (await context.IdentityResolutionEntities.AsNoTrackingWithIdentityResolution()
            .Select(x => new { x.Id, first = x.RequiredAssociate, second = x.RequiredAssociate })
            .ToListAsync()).OrderBy(x => x.Id).ToList();

        Assert.Equal(2, results.Count);
        for (var i = 0; i < results.Count; i++)
        {
            var result = results[i];
            Assert.Same(result.first, result.second);
            if (i > 0)
            {
                var otherResult = results[i - 1];
                Assert.NotSame(otherResult.first, result.first);
            }
        }
    }

    [ConditionalFact]
    public async Task ConcatAssociateCollectionAsNoTrackingWithIdentityResolution()
    {
        using var context = CreateContext();

        var results = await context.IdentityResolutionEntities.AsNoTrackingWithIdentityResolution()
            .Select(x => new { x.Id, Associates = x.Associates.Concat(x.Associates).ToList() })
            .ToListAsync();

        Assert.Equal(2, results.Count);

        foreach (var result in results.Select(result => result.Associates))
        {
            Assert.Equal(4, result.Count);

            for (var j = 0; j < result.Count / 2; j++)
            {
                Assert.Same(result[j], result[j + 2]);

                if (j > 0)
                {
                    Assert.NotSame(result[j], result[j - 1]);
                }
            }
        }
    }

    [ConditionalFact]
    public async Task ConcatOrdinalAssociateCollectionAsNoTrackingWithIdentityResolution()
    {
        using var context = CreateContext();

        var results = await context.IdentityResolutionEntities.AsNoTrackingWithIdentityResolution()
            .Select(x => new { x.Id, OrdinalAssociates = x.OrdinalAssociates.Concat(x.OrdinalAssociates).ToList() })
            .ToListAsync();

        Assert.Equal(2, results.Count);

        foreach (var result in results.Select(result => result.OrdinalAssociates))
        {
            Assert.Equal(4, result.Count);

            for (var j = 0; j < result.Count / 2; j++)
            {
                if (j > 0)
                {
                    Assert.NotSame(result[j], result[j - 1]);
                }

                Assert.NotSame(result[j], result[j + result.Count / 2]);
            }
        }
    }

    [ConditionalFact]
    public async Task SelectManyConcatAssociateCollectionAsNoTrackingWithIdentityResolution_throws()
    {
        using var context = CreateContext();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.IdentityResolutionEntities.AsNoTrackingWithIdentityResolution()
                .SelectMany(x => x.Associates.Concat(x.Associates), (x, associate) => new { x.Id, Associate = associate })
                .ToListAsync());

        AssertMissingOwnerKeyMessage(
            exception,
            nameof(AsNoTrackingWithIdentityResolutionEntity.Id),
            nameof(AsNoTrackingWithIdentityResolutionEntity));
    }

    [ConditionalFact]
    public async Task SelectManyConcatOrdinalAssociateCollectionAsNoTrackingWithIdentityResolution()
    {
        using var context = CreateContext();

        var results = await context.IdentityResolutionEntities.AsNoTrackingWithIdentityResolution()
            .SelectMany(x => x.OrdinalAssociates.Concat(x.OrdinalAssociates), (x, associate) => new { x.Id, Associate = associate })
            .ToListAsync();

        Assert.Equal(8, results.Count);

        foreach (var resultGroup in results.GroupBy(result => result.Id))
        {
            var groupResults = resultGroup.ToList();

            for (var i = 0; i < groupResults.Count / 2; i++)
            {
                var result = groupResults[i].Associate;
                var otherResult = groupResults[i + 2].Associate;

                Assert.NotSame(result, otherResult);

                if (i > 0)
                {
                    Assert.NotSame(result, groupResults[i - 1].Associate);
                }
            }
        }
    }

    [ConditionalFact]
    public async Task DoubleAssociateCollectionAsNoTrackingWithIdentityResolution()
    {
        using var context = CreateContext();

        var results = (await context.IdentityResolutionEntities.AsNoTrackingWithIdentityResolution()
            .Select(x => new { x.Id, first = x.Associates, second = x.Associates })
            .ToListAsync()).OrderBy(x => x.Id).ToList();

        Assert.Equal(2, results.Count);
        for (var i = 0; i < results.Count; i++)
        {
            var result = results[i];

            for (var j = 0; j < result.first.Count; j++)
            {
                Assert.Same(result.first[j], result.second[j]);
            }

            if (i > 0)
            {
                var otherResult = results[i - 1];
                for (var j = 0; j < result.first.Count; j++)
                {
                    Assert.NotSame(otherResult.first[j], result.first[j]);
                }
            }
        }
    }

    [ConditionalFact]
    public async Task DoubleOrdinalAssociateCollectionAsNoTrackingWithIdentityResolution()
    {
        using var context = CreateContext();

        var results = (await context.IdentityResolutionEntities.AsNoTrackingWithIdentityResolution()
            .Select(x => new { x.Id, first = x.OrdinalAssociates, second = x.OrdinalAssociates })
            .ToListAsync()).OrderBy(x => x.Id).ToList();

        Assert.Equal(2, results.Count);
        for (var i = 0; i < results.Count; i++)
        {
            var result = results[i];

            for (var j = 0; j < result.first.Count; j++)
            {
                Assert.Same(result.first[j], result.second[j]);
            }

            if (i > 0)
            {
                var otherResult = results[i - 1];
                for (var j = 0; j < result.first.Count; j++)
                {
                    Assert.NotSame(otherResult.first[j], result.first[j]);
                }
            }
        }
    }

    private ValidationContext CreateContext()
        => Fixture.CreateContext();

    private static void AssertMissingOwnerKeyMessage(
        InvalidOperationException exception,
        string keyPropertyName = nameof(Owner.Id),
        string entityTypeName = nameof(Owner))
        => Assert.Equal(
            CosmosStrings.NoTrackingIdentityResolutionOwnedEntityProjectionMissingOwnerKey(keyPropertyName, entityTypeName),
            exception.Message);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private static void AssertOwner(Owner expected, Owner actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        AssertOwnedReference(expected.OwnedReference, actual.OwnedReference);
        AssertOwnedCollection(expected.OwnedCollection, actual.OwnedCollection);
        Assert.Equal(expected.ComplexReference.Name, actual.ComplexReference.Name);
        AssertComplexCollection(expected.ComplexCollection, actual.ComplexCollection);
    }

    private static void AssertOwnedReference(OwnedReference expected, OwnedReference actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
    }

    private static void AssertOwnedCollection(IEnumerable<OwnedCollectionElement> expected, IEnumerable<OwnedCollectionElement> actual)
        => Assert.Collection(
            actual,
            expected.ToList().Select<OwnedCollectionElement, Action<OwnedCollectionElement>>(
                expectedElement => actualElement =>
                {
                    Assert.Equal(expectedElement.Id, actualElement.Id);
                    Assert.Equal(expectedElement.Name, actualElement.Name);
                }).ToArray());

    private static void AssertComplexCollection(IEnumerable<ComplexCollectionElement> expected, IEnumerable<ComplexCollectionElement> actual)
        => Assert.Collection(
            actual,
            expected.ToList().Select<ComplexCollectionElement, Action<ComplexCollectionElement>>(
                expectedElement => actualElement => Assert.Equal(expectedElement.Name, actualElement.Name)).ToArray());

    private static ShapedQueryExpression Translate<T>(ValidationContext context, IQueryable<T> query)
    {
        var queryCompilationContext = context.GetService<IQueryCompilationContextFactory>().Create(async: false);
        var preprocessedQuery = context.GetService<IQueryTranslationPreprocessorFactory>()
            .Create(queryCompilationContext)
            .Process(query.Expression);

        var translatedQuery = context.GetService<IQueryableMethodTranslatingExpressionVisitorFactory>()
            .Create(queryCompilationContext)
            .Translate(preprocessedQuery);

        return Assert.IsType<ShapedQueryExpression>(context.GetService<IQueryTranslationPostprocessorFactory>()
            .Create(queryCompilationContext)
            .Process(translatedQuery));
    }

    public class ProjectionFixture : QueryFixtureBase<ValidationContext>
    {
        public static readonly Guid OwnerId = new("11111111-1111-1111-1111-111111111111");
        public static readonly Guid OwnedReferenceId = new("22222222-2222-2222-2222-222222222222");
        public static readonly Guid OwnedCollectionElementId = new("33333333-3333-3333-3333-333333333333");
        public static readonly Guid IdentityResolutionEntityId1 = new("44444444-4444-4444-4444-444444444444");
        public static readonly Guid IdentityResolutionEntityId2 = new("55555555-5555-5555-5555-555555555555");
        public static readonly Guid AssociateId1 = new("66666666-6666-6666-6666-666666666666");
        public static readonly Guid AssociateId2 = new("77777777-7777-7777-7777-777777777777");
        public static readonly Guid AssociateId3 = new("88888888-8888-8888-8888-888888888888");
        public static readonly Guid AssociateId4 = new("99999999-9999-9999-9999-999999999999");

        private ProjectionData? _expectedData;

        protected override string StoreName
            => nameof(CosmosNoTrackingWithIdentityResolutionProjectionTest);

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(ValidationContext context)
        {
            var data = (ProjectionData)GetExpectedData();
            context.AddRange(data.Owners);
            context.AddRange(data.IdentityResolutionEntities);

            return context.SaveChangesAsync();
        }

        public override ISetSource GetExpectedData()
            => _expectedData ??= new ProjectionData();

        public override IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
        {
            { typeof(Owner), e => ((Owner?)e)?.Id }
        }.ToDictionary(e => e.Key, e => (object)e.Value);

        public override IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object?, object?>>
        {
            {
                typeof(Owner), (e, a) =>
                {
                    Assert.Equal(e is null, a is null);

                    if (a is not null)
                    {
                        AssertOwner((Owner)e!, (Owner)a);
                    }
                }
            }
        }.ToDictionary(e => e.Key, e => (object)e.Value);
    }

    public class ProjectionData : ISetSource
    {
        public IReadOnlyList<Owner> Owners { get; } =
        [
            new()
            {
                Id = ProjectionFixture.OwnerId,
                Name = "Owner",
                OwnedReference = new OwnedReference
                {
                    Id = ProjectionFixture.OwnedReferenceId,
                    Name = "OwnedReference"
                },
                OwnedCollection =
                [
                    new OwnedCollectionElement
                    {
                        Id = ProjectionFixture.OwnedCollectionElementId,
                        Name = "OwnedCollection"
                    }
                ],
                ComplexReference = new ComplexReference
                {
                    Name = "ComplexReference"
                },
                ComplexCollection =
                [
                    new ComplexCollectionElement
                    {
                        Name = "ComplexCollection"
                    }
                ]
            }
        ];

        public IReadOnlyList<AsNoTrackingWithIdentityResolutionEntity> IdentityResolutionEntities { get; } =
        [
            new()
            {
                Id = ProjectionFixture.IdentityResolutionEntityId1,
                RequiredAssociate = new AsNoTrackingWithIdentityResolutionAssociateEntity(),
                Associates =
                [
                    new AsNoTrackingWithIdentityResolutionAssociateEntity
                    {
                        Id = ProjectionFixture.AssociateId1
                    },
                    new AsNoTrackingWithIdentityResolutionAssociateEntity
                    {
                        Id = ProjectionFixture.AssociateId2
                    }
                ],
                OrdinalAssociates = [new(), new()]
            },
            new()
            {
                Id = ProjectionFixture.IdentityResolutionEntityId2,
                RequiredAssociate = new AsNoTrackingWithIdentityResolutionAssociateEntity(),
                Associates =
                [
                    new AsNoTrackingWithIdentityResolutionAssociateEntity
                    {
                        Id = ProjectionFixture.AssociateId3
                    },
                    new AsNoTrackingWithIdentityResolutionAssociateEntity
                    {
                        Id = ProjectionFixture.AssociateId4
                    }
                ],
                OrdinalAssociates = [new(), new()]
            }
        ];

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(Owner))
            {
                return (IQueryable<TEntity>)Owners.AsQueryable();
            }

            if (typeof(TEntity) == typeof(AsNoTrackingWithIdentityResolutionEntity))
            {
                return (IQueryable<TEntity>)IdentityResolutionEntities.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }
    }

    public class Owner
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Owner";
        public OwnedReference OwnedReference { get; set; } = new();
        public List<OwnedCollectionElement> OwnedCollection { get; set; } = [];
        public ComplexReference ComplexReference { get; set; } = new();
        public List<ComplexCollectionElement> ComplexCollection { get; set; } = [];
    }

    public class OwnedReference
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "OwnedReference";
    }

    public class OwnedCollectionElement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "OwnedCollection";
    }

    public class ComplexReference
    {
        public string Name { get; set; } = "ComplexReference";
    }

    public class ComplexCollectionElement
    {
        public string Name { get; set; } = "ComplexCollection";
    }

    public class AsNoTrackingWithIdentityResolutionEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public AsNoTrackingWithIdentityResolutionAssociateEntity RequiredAssociate { get; set; } = new();

        public List<AsNoTrackingWithIdentityResolutionAssociateEntity> Associates { get; set; } = new();

        public List<AsNoTrackingWithIdentityResolutionOrdinalAssociateEntity> OrdinalAssociates { get; set; } = new();
    }

    public class AsNoTrackingWithIdentityResolutionAssociateEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Name";
    }

    public class AsNoTrackingWithIdentityResolutionOrdinalAssociateEntity
    {
        public string OtherName { get; set; } = "Name";
    }

    public class ValidationContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Owner> Owners
            => Set<Owner>();

        public DbSet<AsNoTrackingWithIdentityResolutionEntity> IdentityResolutionEntities
            => Set<AsNoTrackingWithIdentityResolutionEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Owner>(builder =>
            {
                builder.HasPartitionKey(owner => owner.Id);
                builder.OwnsOne(owner => owner.OwnedReference);
                builder.OwnsMany(owner => owner.OwnedCollection);
                builder.ComplexProperty(owner => owner.ComplexReference);
                builder.ComplexCollection(owner => owner.ComplexCollection);
            });

            modelBuilder.Entity<AsNoTrackingWithIdentityResolutionEntity>(builder =>
            {
                builder.ToContainer(nameof(AsNoTrackingWithIdentityResolutionEntity));
                builder.HasPartitionKey(entity => entity.Id);
                builder.OwnsOne(entity => entity.RequiredAssociate);
                builder.OwnsMany(entity => entity.Associates);
                builder.OwnsMany(entity => entity.OrdinalAssociates);
            });
        }
    }
}
