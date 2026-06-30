// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class CosmosNoTrackingWithIdentityResolutionProjectionValidationTest
    : IClassFixture<CosmosNoTrackingWithIdentityResolutionProjectionValidationTest.Fixture>
{
    public CosmosNoTrackingWithIdentityResolutionProjectionValidationTest(Fixture fixture, ITestOutputHelper testOutputHelper)
    {
        TestFixture = fixture;
        TestFixture.TestSqlLoggerFactory.Clear();
        TestFixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    private Fixture TestFixture { get; }

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
    public async Task Owned_reference_with_owner_key_succeeds()
    {
        using var context = CreateContext();

        var result = Assert.Single(await context.Owners
            .Select(owner => new { owner.Id, owner.OwnedReference })
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync());

        Assert.Equal(Fixture.OwnerId, result.Id);
        AssertOwnedReference(result.OwnedReference);

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
        using var context = CreateContext();

        var result = Assert.Single(await context.Owners
            .Select(owner => new { owner.Id, owner.OwnedCollection })
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync());

        Assert.Equal(Fixture.OwnerId, result.Id);
        AssertOwnedCollection(result.OwnedCollection);

        AssertSql(
            """
SELECT c["id"], c["OwnedCollection"]
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Owned_reference_with_owner_key_after_owned_reference_projects_owner_key_first()
    {
        using var context = CreateContext();

        var result = Assert.Single(await context.Owners
            .Select(owner => new { owner.OwnedReference, owner.Id })
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync());

        Assert.Equal(Fixture.OwnerId, result.Id);
        AssertOwnedReference(result.OwnedReference);

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
        using var context = CreateContext();

        var result = Assert.Single(await context.Owners
            .Select(owner => new { owner.OwnedCollection, owner.Id })
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync());

        Assert.Equal(Fixture.OwnerId, result.Id);
        AssertOwnedCollection(result.OwnedCollection);

        AssertSql(
            """
SELECT c["id"], c["OwnedCollection"]
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Complex_reference_without_owner_key_succeeds()
    {
        using var context = CreateContext();

        var result = Assert.Single(await context.Owners
            .Select(owner => owner.ComplexReference)
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync());

        Assert.Equal("ComplexReference", result.Name);

        AssertSql(
            """
SELECT VALUE c["ComplexReference"]
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Complex_collection_without_owner_key_succeeds()
    {
        using var context = CreateContext();

        var result = Assert.Single(await context.Owners
            .Select(owner => owner.ComplexCollection)
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync());

        Assert.Collection(
            result,
            element => Assert.Equal("ComplexCollection", element.Name));

        AssertSql(
            """
SELECT VALUE c["ComplexCollection"]
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Scalar_projection_without_owner_key_succeeds()
    {
        using var context = CreateContext();

        var result = Assert.Single(await context.Owners
            .Select(owner => owner.Name)
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync());

        Assert.Equal("Owner", result);

        AssertSql(
            """
SELECT VALUE c["Name"]
FROM root c
""");
    }

    [ConditionalFact]
    public async Task Root_entity_projection_succeeds()
    {
        using var context = CreateContext();

        var result = Assert.Single(await context.Owners
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync());

        AssertOwner(result);

        AssertSql(
            """
SELECT VALUE c
FROM root c
""");
    }

    private ValidationContext CreateContext()
        => TestFixture.CreateContext();

    private static void AssertMissingOwnerKeyMessage(InvalidOperationException exception)
        => Assert.Equal(
            CosmosStrings.NoTrackingIdentityResolutionOwnedEntityProjectionMissingOwnerKey(nameof(Owner.Id), nameof(Owner)),
            exception.Message);

    private void AssertSql(params string[] expected)
        => TestFixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private static void AssertOwner(Owner owner)
    {
        Assert.Equal(Fixture.OwnerId, owner.Id);
        Assert.Equal("Owner", owner.Name);
        AssertOwnedReference(owner.OwnedReference);
        AssertOwnedCollection(owner.OwnedCollection);
        Assert.Equal("ComplexReference", owner.ComplexReference.Name);
        Assert.Collection(
            owner.ComplexCollection,
            element => Assert.Equal("ComplexCollection", element.Name));
    }

    private static void AssertOwnedReference(OwnedReference ownedReference)
    {
        Assert.Equal(Fixture.OwnedReferenceId, ownedReference.Id);
        Assert.Equal("OwnedReference", ownedReference.Name);
    }

    private static void AssertOwnedCollection(IEnumerable<OwnedCollectionElement> ownedCollection)
        => Assert.Collection(
            ownedCollection,
            element =>
            {
                Assert.Equal(Fixture.OwnedCollectionElementId, element.Id);
                Assert.Equal("OwnedCollection", element.Name);
            });

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

    public class Fixture : SharedStoreFixtureBase<ValidationContext>
    {
        public static readonly Guid OwnerId = new("11111111-1111-1111-1111-111111111111");
        public static readonly Guid OwnedReferenceId = new("22222222-2222-2222-2222-222222222222");
        public static readonly Guid OwnedCollectionElementId = new("33333333-3333-3333-3333-333333333333");

        protected override string StoreName
            => nameof(CosmosNoTrackingWithIdentityResolutionProjectionValidationTest);

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(ValidationContext context)
        {
            context.Add(
                new Owner
                {
                    Id = OwnerId,
                    Name = "Owner",
                    OwnedReference = new OwnedReference
                    {
                        Id = OwnedReferenceId,
                        Name = "OwnedReference"
                    },
                    OwnedCollection =
                    [
                        new OwnedCollectionElement
                        {
                            Id = OwnedCollectionElementId,
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
                });

            return context.SaveChangesAsync();
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

    public class ValidationContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Owner> Owners
            => Set<Owner>();

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
        }
    }
}
