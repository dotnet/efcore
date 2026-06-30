// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class CosmosNoTrackingWithIdentityResolutionProjectionValidationTest(
    CosmosNoTrackingWithIdentityResolutionProjectionValidationTest.Fixture fixture)
    : IClassFixture<CosmosNoTrackingWithIdentityResolutionProjectionValidationTest.Fixture>
{
    private Fixture TestFixture { get; } = fixture;

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
    public void Owned_reference_with_owner_key_succeeds()
    {
        using var context = CreateContext();

        Translate(
            context,
            context.Owners
                .Select(owner => new { owner.Id, owner.OwnedReference })
                .AsNoTrackingWithIdentityResolution());
    }

    [ConditionalFact]
    public void Owned_collection_with_owner_key_succeeds()
    {
        using var context = CreateContext();

        Translate(
            context,
            context.Owners
                .Select(owner => new { owner.Id, owner.OwnedCollection })
                .AsNoTrackingWithIdentityResolution());
    }

    [ConditionalFact]
    public void Complex_reference_without_owner_key_succeeds()
    {
        using var context = CreateContext();

        Translate(
            context,
            context.Owners
                .Select(owner => owner.ComplexReference)
                .AsNoTrackingWithIdentityResolution());
    }

    [ConditionalFact]
    public void Complex_collection_without_owner_key_succeeds()
    {
        using var context = CreateContext();

        Translate(
            context,
            context.Owners
                .Select(owner => owner.ComplexCollection)
                .AsNoTrackingWithIdentityResolution());
    }

    [ConditionalFact]
    public void Scalar_projection_without_owner_key_succeeds()
    {
        using var context = CreateContext();

        Translate(
            context,
            context.Owners
                .Select(owner => owner.Name)
                .AsNoTrackingWithIdentityResolution());
    }

    [ConditionalFact]
    public void Root_entity_projection_succeeds()
    {
        using var context = CreateContext();

        Translate(
            context,
            context.Owners
                .AsNoTrackingWithIdentityResolution());
    }

    private ValidationContext CreateContext()
        => TestFixture.CreateContext();

    private static void AssertMissingOwnerKeyMessage(InvalidOperationException exception)
        => Assert.Equal(
            CosmosStrings.NoTrackingIdentityResolutionOwnedEntityProjectionMissingOwnerKey(nameof(Owner.Id), nameof(Owner)),
            exception.Message);

    private static Expression Translate<T>(ValidationContext context, IQueryable<T> query)
    {
        var queryCompilationContext = context.GetService<IQueryCompilationContextFactory>().Create(async: false);
        var preprocessedQuery = context.GetService<IQueryTranslationPreprocessorFactory>()
            .Create(queryCompilationContext)
            .Process(query.Expression);

        var translatedQuery = context.GetService<IQueryableMethodTranslatingExpressionVisitorFactory>()
            .Create(queryCompilationContext)
            .Translate(preprocessedQuery);

        return context.GetService<IQueryTranslationPostprocessorFactory>()
            .Create(queryCompilationContext)
            .Process(translatedQuery);
    }

    public class Fixture : SharedStoreFixtureBase<ValidationContext>
    {
        protected override string StoreName
            => nameof(CosmosNoTrackingWithIdentityResolutionProjectionValidationTest);

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
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
