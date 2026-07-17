// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

[ConditionalClass(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]
public class VectorSearchTranslationsCosmosByteTest(
    VectorSearchTranslationsCosmosTest<byte>.VectorSearchFixture fixture,
    ITestOutputHelper testOutputHelper)
    : VectorSearchTranslationsCosmosTest<byte>(fixture, testOutputHelper);

[ConditionalClass(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]
public class VectorSearchTranslationsCosmosSByteTest(
    VectorSearchTranslationsCosmosTest<sbyte>.VectorSearchFixture fixture,
    ITestOutputHelper testOutputHelper)
    : VectorSearchTranslationsCosmosTest<sbyte>(fixture, testOutputHelper);

[ConditionalClass(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]
public class VectorSearchTranslationsCosmosFloatTest(
    VectorSearchTranslationsCosmosTest<float>.VectorSearchFixture fixture,
    ITestOutputHelper testOutputHelper)
    : VectorSearchTranslationsCosmosTest<float>(fixture, testOutputHelper)
{
    [Fact]
    public virtual async Task OrderBy_VectorDistance_memory_with_shorter_input()
    {
        await using var context = CreateContext();
        var inputVector = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f }.AsMemory();

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(e.Vector, inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            """
@p='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["Vector"], @p)
""");
    }

    [Fact]
    public virtual async Task VectorDistance_with_data_type_and_distance_function_with_shorter_input()
    {
        await using var context = CreateContext();
        var inputVector = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f };

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(e => EF.Functions.VectorDistance(
                e.VectorArray, inputVector, useBruteForce: false,
                new VectorDistanceOptions { DataType = "float32", DistanceFunction = DistanceFunction.DotProduct }))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            """
@p='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["VectorArray"], @p, false, { 'distanceFunction': 'dotproduct', 'dataType': 'float32' })
""");
    }
}

[ConditionalClass(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]
public class VectorSearchTranslationsCosmosMixedTest
    : IClassFixture<VectorSearchTranslationsCosmosMixedTest.MixedVectorSearchFixture>
{
    public VectorSearchTranslationsCosmosMixedTest(MixedVectorSearchFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected MixedVectorSearchFixture Fixture { get; }

    [Fact]
    public virtual async Task RRF_with_byte_and_float_Vector_distance_functions_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector1 = new byte[] { 2, 1, 4, 6, 5, 2, 5, 7, 3, 1 };
        var inputVector2 = new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f };

        var booksFromStore = await context
            .Set<MixedBook>()
            .OrderBy(e => EF.Functions.Rrf(
                EF.Functions.VectorDistance(e.BytesArray, inputVector1),
                EF.Functions.VectorDistance(e.SinglesArray, inputVector2)))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);

        AssertSql(
            """
@p='[2,1,4,6,5,2,5,7,3,1]'
@p3='[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78]'

SELECT VALUE c
FROM root c
ORDER BY RANK RRF(VectorDistance(c["BytesArray"], @p), VectorDistance(c["SinglesArray"], @p3))
""");
    }

    private class MixedBook
    {
        public Guid Id { get; set; }
        public string Publisher { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public byte[] BytesArray { get; set; } = null!;
        public float[] SinglesArray { get; set; } = null!;
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class MixedVectorSearchFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "VectorSearchMixedTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            => modelBuilder.Entity<MixedBook>(b =>
            {
                b.Property(e => e.Id).ValueGeneratedOnAdd();
                b.HasKey(e => e.Id);
                b.HasPartitionKey(e => e.Publisher);

                b.HasIndex(e => e.BytesArray).IsVectorIndex(VectorIndexType.Flat);
                b.HasIndex(e => e.SinglesArray).IsVectorIndex(VectorIndexType.Flat);

                b.Property(e => e.BytesArray).IsVectorProperty(DistanceFunction.Cosine, 10);
                b.Property(e => e.SinglesArray).IsVectorProperty(DistanceFunction.Cosine, 10);
            });

        protected override Task SeedAsync(PoolableDbContext context)
        {
            var book1 = CreateBook(
                publisher: "Manning",
                author: "Jon P Smith",
                title: "Entity Framework Core in Action");

            var book2 = CreateBook(
                publisher: "O'Reilly",
                author: "Julie Lerman",
                title: "Programming Entity Framework: DbContext");

            var book3 = CreateBook(
                publisher: "O'Reilly",
                author: "Julie Lerman",
                title: "Programming Entity Framework");

            context.AddRange(book1, book2, book3);

            return context.SaveChangesAsync();
        }

        private static MixedBook CreateBook(string publisher, string author, string title)
            => new()
            {
                Publisher = publisher,
                Author = author,
                Title = title,
                BytesArray = [2, 1, 4, 3, 5, 2, 5, 7, 3, 1],
                SinglesArray = [0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f]
            };

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }
}

public abstract class VectorSearchTranslationsCosmosTest<TVector>
    : IClassFixture<VectorSearchTranslationsCosmosTest<TVector>.VectorSearchFixture>
{
    protected VectorSearchTranslationsCosmosTest(VectorSearchFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected VectorSearchFixture Fixture { get; }

    [Fact]
    public virtual async Task OrderBy_VectorDistance_memory()
    {
        await using var context = CreateContext();
        var inputVector = VectorData.InputVector.AsMemory();

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(VectorDistance(nameof(Book.Vector), inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            $$"""
@p='{{VectorData.SqlLiteral}}'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["Vector"], @p)
""");
    }

    [Fact]
    public virtual async Task OrderBy_VectorDistance_memory_with_array()
    {
        await using var context = CreateContext();
        var inputVector = VectorData.InputVector;

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(VectorDistance(nameof(Book.Vector), inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            $$"""
@p='{{VectorData.SqlLiteral}}'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["Vector"], @p)
""");
    }

    [Fact]
    public virtual async Task OrderBy_VectorDistance_array()
    {
        await using var context = CreateContext();
        var inputVector = VectorData.InputVector;

        var booksFromStore = await context
            .Set<Book>()
            .Select(VectorDistance(
                nameof(Book.VectorArray), inputVector, useBruteForce: false,
                new VectorDistanceOptions { DistanceFunction = DistanceFunction.DotProduct }))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            $$"""
@p='{{VectorData.SqlLiteral}}'

SELECT VALUE VectorDistance(c["VectorArray"], @p, false, { 'distanceFunction': 'dotproduct' })
FROM root c
""");
    }

    [Fact]
    public virtual async Task OrderBy_VectorDistance_array_with_memory()
    {
        await using var context = CreateContext();
        var inputVector = VectorData.InputVector.AsMemory();

        var booksFromStore = await context
            .Set<Book>()
            .Select(VectorDistance(
                nameof(Book.VectorArray), inputVector, useBruteForce: false,
                new VectorDistanceOptions { DistanceFunction = DistanceFunction.DotProduct }))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            $$"""
@p='{{VectorData.SqlLiteral}}'

SELECT VALUE VectorDistance(c["VectorArray"], @p, false, { 'distanceFunction': 'dotproduct' })
FROM root c
""");
    }

    [Fact]
    public virtual async Task OrderBy_VectorDistance_array_without_options()
    {
        await using var context = CreateContext();
        var inputVector = VectorData.InputVector;

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(VectorDistance(nameof(Book.VectorArray), inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            $$"""
@p='{{VectorData.SqlLiteral}}'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["VectorArray"], @p)
""");
    }

    [Fact]
    public virtual async Task OrderBy_VectorDistance_array_without_options_with_memory()
    {
        await using var context = CreateContext();
        var inputVector = VectorData.InputVector.AsMemory();

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(VectorDistance(nameof(Book.VectorArray), inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            $$"""
@p='{{VectorData.SqlLiteral}}'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["VectorArray"], @p)
""");
    }

    #region Brute force and options

    [Fact]
    public virtual async Task VectorDistance_with_brute_force_true()
    {
        await using var context = CreateContext();
        var inputVector = VectorData.InputVector;

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(VectorDistance(nameof(Book.VectorArray), inputVector, useBruteForce: true))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            $$"""
@p='{{VectorData.SqlLiteral}}'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["VectorArray"], @p, true)
""");
    }

    [Fact]
    public virtual async Task VectorDistance_with_brute_force_and_distance_function()
    {
        await using var context = CreateContext();
        var inputVector = VectorData.InputVector;

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(VectorDistance(
                nameof(Book.VectorArray), inputVector, useBruteForce: true,
                new VectorDistanceOptions { DistanceFunction = DistanceFunction.DotProduct }))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            $$"""
@p='{{VectorData.SqlLiteral}}'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["VectorArray"], @p, true, { 'distanceFunction': 'dotproduct' })
""");
    }

    [Fact]
    public virtual async Task VectorDistance_with_distance_function_and_brute_force_null()
    {
        await using var context = CreateContext();
        var inputVector = VectorData.InputVector;

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(VectorDistance(
                nameof(Book.VectorArray), inputVector, useBruteForce: null,
                new VectorDistanceOptions { DistanceFunction = DistanceFunction.DotProduct }))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            $$"""
@p='{{VectorData.SqlLiteral}}'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["VectorArray"], @p, false, { 'distanceFunction': 'dotproduct' })
""");
    }

    [Fact]
    public virtual async Task VectorDistance_with_data_type_and_distance_function()
    {
        await using var context = CreateContext();
        var inputVector = VectorData.InputVector;

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(VectorDistance(
                nameof(Book.VectorArray), inputVector, useBruteForce: false,
                new VectorDistanceOptions { DataType = VectorData.DataType, DistanceFunction = DistanceFunction.DotProduct }))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        AssertSql(
            $$"""
@p='{{VectorData.SqlLiteral}}'

SELECT VALUE c
FROM root c
ORDER BY VectorDistance(c["VectorArray"], @p, false, { 'distanceFunction': 'dotproduct', 'dataType': '{{VectorData.DataType}}' })
""");
    }

    #endregion Brute force and options

    // issue #35898: vector indexes on collection wildcard paths are not supported
    //[ConditionalFact(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotEmulator))]
    public virtual async Task Vector_index_through_complex_collection_roundtrips()
    {
        await using var context = CreateContext();
        var books = await context.Set<Book>().ToListAsync();

        Assert.Equal(3, books.Count);
        Assert.All(books, b => Assert.Single(b.ComplexNestedCollection));

        // Verify that VectorDistance can be queried over the property inside the complex collection,
        // exercising the vector index created for "ComplexNestedCollection[].NestedVector".
        Fixture.TestSqlLoggerFactory.Clear();

        var inputVector = new ReadOnlyMemory<TVector>(VectorData.InputVector);
        var distances = await context
            .Set<Book>()
            .SelectMany(b => b.ComplexNestedCollection)
            .Select(VectorDistanceComplex(inputVector))
            .ToListAsync();

        Assert.Equal(3, distances.Count);
        Assert.All(distances, d => Assert.NotEqual(0.0, d));

        AssertSql(
            $$"""
@_inputVector='{{VectorData.SqlLiteral}}'

SELECT VALUE VectorDistance(c0["NestedVector"], @_inputVector)
FROM root c
JOIN c0 IN c["ComplexNestedCollection"]
""");
    }

    [Fact]
    public virtual async Task Select_VectorDistance()
    {
        await using var context = CreateContext();
        var inputVector = new ReadOnlyMemory<TVector>(VectorData.InputVector);

        var booksFromStore = await context
            .Set<Book>()
            .Select(VectorDistanceNested(inputVector))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);
        Assert.All(booksFromStore, s => Assert.NotEqual(0.0, s));

        AssertSql(
            $$"""
@_inputVector='{{VectorData.SqlLiteral}}'

SELECT VALUE VectorDistance(c["OwnedReference"]["NestedOwned"]["NestedVector"], @_inputVector)
FROM root c
""");
    }

    [Fact]
    public virtual async Task RRF_with_two_Vector_distance_functions_in_OrderBy()
    {
        await using var context = CreateContext();
        var inputVector1 = VectorData.InputVector;
        var inputVector2 = VectorData.AlternateInputVector;

        var booksFromStore = await context
            .Set<Book>()
            .OrderBy(Rrf(inputVector1, inputVector2))
            .ToListAsync();

        Assert.Equal(3, booksFromStore.Count);

        AssertSql(
            $$"""
@p='{{VectorData.SqlLiteral}}'
@p3='{{VectorData.AlternateSqlLiteral}}'

SELECT VALUE c
FROM root c
ORDER BY RANK RRF(VectorDistance(c["Vector"], @p), VectorDistance(c["VectorArray"], @p3))
""");
    }

    [Fact]
    public virtual async Task VectorDistance_throws_when_used_on_non_vector()
    {
        await using var context = CreateContext();
        var inputVector = Array.Empty<TVector>();

        Assert.Equal(
            CosmosStrings.VectorSearchRequiresVector,
            (await Assert.ThrowsAsync<InvalidOperationException>(async () => await context
                .Set<Book>()
                .OrderBy(VectorDistance(nameof(Book.NonVector), inputVector))
                .ToListAsync())).Message);

        Assert.Equal(
            CosmosStrings.VectorSearchRequiresVector,
            (await Assert.ThrowsAsync<InvalidOperationException>(async () => await context
                .Set<Book>()
                .OrderBy(VectorDistanceReversed(nameof(Book.NonVector), inputVector))
                .ToListAsync())).Message);
    }

    [Fact]
    public virtual async Task VectorDistance_throws_when_used_with_non_const_args()
    {
        await using var context = CreateContext();
        var inputVector = new ReadOnlyMemory<TVector>(VectorData.InputVector);

        Assert.Equal(
            CoreStrings.ArgumentNotConstant("useBruteForce", nameof(CosmosDbFunctionsExtensions.VectorDistance)),
            (await Assert.ThrowsAsync<InvalidOperationException>(async () => await context
                .Set<Book>()
                .OrderBy(VectorDistanceNestedWithNonConstantUseBruteForce(inputVector))
                .ToListAsync())).Message);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await context
            .Set<Book>()
            .OrderBy(VectorDistanceNestedWithNonConstantOptions(inputVector))
            .ToListAsync());
    }

    protected class Book
    {
        public Guid Id { get; set; }

        public string Publisher { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Author { get; set; } = null!;

        public ReadOnlyMemory<TVector> NonVector { get; set; }

        public bool IsPublished { get; set; }

        public DistanceFunction DistanceFunction { get; set; } // Not meaningful; used for exception testing.

        public ReadOnlyMemory<TVector> Vector { get; set; }

        public TVector[] VectorArray { get; set; } = null!;

        public Owned1 OwnedReference { get; set; } = null!;
        public List<Owned1> OwnedCollection { get; set; } = null!;
        public List<ComplexNested> ComplexNestedCollection { get; set; } = null!;
    }

    protected class Owned1
    {
        public int Prop { get; set; }
        public Owned2 NestedOwned { get; set; } = null!;
        public List<Owned2> NestedOwnedCollection { get; set; } = null!;
    }

    protected class Owned2
    {
        public string Prop { get; set; } = null!;
        public ReadOnlyMemory<TVector> NestedVector { get; set; }
    }

    protected class ComplexNested
    {
        public ReadOnlyMemory<TVector> NestedVector { get; set; }
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private static Expression<Func<Book, double>> VectorDistance(
        string propertyName,
        Memory<TVector> inputVector,
        bool? useBruteForce = null,
        VectorDistanceOptions? options = null)
        => VectorDistance(
            propertyName,
            VectorParameter(inputVector, convertToReadOnlyMemory: true),
            useBruteForce,
            options);

    private static Expression<Func<Book, double>> VectorDistance(
        string propertyName,
        TVector[] inputVector,
        bool? useBruteForce = null,
        VectorDistanceOptions? options = null)
        => VectorDistance(
            propertyName,
            VectorParameter(inputVector, convertToReadOnlyMemory: true),
            useBruteForce,
            options);

    private static Expression<Func<Book, double>> VectorDistance(
        string propertyName,
        ReadOnlyMemory<TVector> inputVector,
        bool? useBruteForce = null,
        VectorDistanceOptions? options = null)
        => VectorDistance(
            propertyName,
            VectorParameter(inputVector, convertToReadOnlyMemory: false),
            useBruteForce,
            options);

    private static Expression<Func<Book, double>> VectorDistance(
        string propertyName,
        Expression inputVector,
        bool? useBruteForce,
        VectorDistanceOptions? options)
    {
        var parameter = Expression.Parameter(typeof(Book), "e");
        var vector = ConvertToReadOnlyMemory(Expression.Property(parameter, propertyName));

        return Expression.Lambda<Func<Book, double>>(
            VectorDistanceCall(
                vector,
                inputVector,
                Expression.Constant(useBruteForce, typeof(bool?)),
                Expression.Constant(options, typeof(VectorDistanceOptions))),
            parameter);
    }

    private static Expression<Func<Book, double>> VectorDistanceReversed(
        string propertyName,
        TVector[] inputVector)
    {
        var parameter = Expression.Parameter(typeof(Book), "e");
        var vector = ConvertToReadOnlyMemory(Expression.Property(parameter, propertyName));

        return Expression.Lambda<Func<Book, double>>(
            VectorDistanceCall(
                VectorParameter(inputVector, convertToReadOnlyMemory: true),
                vector,
                Expression.Constant(null, typeof(bool?)),
                Expression.Constant(null, typeof(VectorDistanceOptions))),
            parameter);
    }

    private static Expression<Func<Book, double>> VectorDistanceNested(ReadOnlyMemory<TVector> inputVector)
    {
        var parameter = Expression.Parameter(typeof(Book), "e");

        return Expression.Lambda<Func<Book, double>>(
            VectorDistanceCall(
                NestedVector(parameter),
                VectorParameter(inputVector, convertToReadOnlyMemory: false),
                Expression.Constant(null, typeof(bool?)),
                Expression.Constant(null, typeof(VectorDistanceOptions))),
            parameter);
    }

    private static Expression<Func<Book, double>> VectorDistanceNestedWithNonConstantUseBruteForce(ReadOnlyMemory<TVector> inputVector)
    {
        var parameter = Expression.Parameter(typeof(Book), "e");

        return Expression.Lambda<Func<Book, double>>(
            VectorDistanceCall(
                NestedVector(parameter),
                VectorParameter(inputVector, convertToReadOnlyMemory: false),
                Expression.Convert(Expression.Property(parameter, nameof(Book.IsPublished)), typeof(bool?)),
                Expression.Constant(null, typeof(VectorDistanceOptions))),
            parameter);
    }

    private static Expression<Func<Book, double>> VectorDistanceNestedWithNonConstantOptions(ReadOnlyMemory<TVector> inputVector)
    {
        var parameter = Expression.Parameter(typeof(Book), "e");
        var options = Expression.MemberInit(
            Expression.New(typeof(VectorDistanceOptions)),
            Expression.Bind(
                DistanceFunctionProperty,
                Expression.Convert(Expression.Property(parameter, nameof(Book.DistanceFunction)), typeof(DistanceFunction?))));

        return Expression.Lambda<Func<Book, double>>(
            VectorDistanceCall(
                NestedVector(parameter),
                VectorParameter(inputVector, convertToReadOnlyMemory: false),
                Expression.Constant(false, typeof(bool?)),
                options),
            parameter);
    }

    private static Expression<Func<ComplexNested, double>> VectorDistanceComplex(ReadOnlyMemory<TVector> inputVector)
    {
        var parameter = Expression.Parameter(typeof(ComplexNested), "e");

        return Expression.Lambda<Func<ComplexNested, double>>(
            VectorDistanceCall(
                ConvertToReadOnlyMemory(Expression.Property(parameter, nameof(ComplexNested.NestedVector))),
                VectorParameter(inputVector, convertToReadOnlyMemory: false),
                Expression.Constant(null, typeof(bool?)),
                Expression.Constant(null, typeof(VectorDistanceOptions))),
            parameter);
    }

    private static Expression<Func<Book, double>> Rrf(TVector[] inputVector1, TVector[] inputVector2)
    {
        var parameter = Expression.Parameter(typeof(Book), "e");
        var score1 = VectorDistanceCall(
            ConvertToReadOnlyMemory(Expression.Property(parameter, nameof(Book.Vector))),
            VectorParameter(inputVector1, convertToReadOnlyMemory: true),
            Expression.Constant(null, typeof(bool?)),
            Expression.Constant(null, typeof(VectorDistanceOptions)));
        var score2 = VectorDistanceCall(
            ConvertToReadOnlyMemory(Expression.Property(parameter, nameof(Book.VectorArray))),
            VectorParameter(inputVector2, convertToReadOnlyMemory: true),
            Expression.Constant(null, typeof(bool?)),
            Expression.Constant(null, typeof(VectorDistanceOptions)));

        return Expression.Lambda<Func<Book, double>>(
            Expression.Call(
                RrfMethod,
                Functions,
                Expression.NewArrayInit(typeof(double), score1, score2)),
            parameter);
    }

    private static Expression VectorDistanceCall(
        Expression vector1,
        Expression vector2,
        Expression useBruteForce,
        Expression options)
        => Expression.Call(
            VectorDistanceMethod,
            Functions,
            vector1,
            vector2,
            useBruteForce,
            options);

    private static Expression NestedVector(ParameterExpression parameter)
        => ConvertToReadOnlyMemory(
            Expression.Property(
                Expression.Property(
                    Expression.Property(parameter, nameof(Book.OwnedReference)),
                    nameof(Owned1.NestedOwned)),
                nameof(Owned2.NestedVector)));

    private static Expression VectorParameter<TParameter>(TParameter inputVector, bool convertToReadOnlyMemory)
    {
        var holder = new VectorParameterHolder<TParameter>(inputVector);
        Expression vector = Expression.Field(Expression.Constant(holder), VectorParameterField<TParameter>());

        return convertToReadOnlyMemory
            ? Expression.Convert(vector, ReadOnlyMemoryType)
            : vector;
    }

    private static Expression ConvertToReadOnlyMemory(Expression vector)
        => vector.Type == ReadOnlyMemoryType
            ? vector
            : Expression.Convert(vector, ReadOnlyMemoryType);

    private static readonly Type ReadOnlyMemoryType = typeof(ReadOnlyMemory<TVector>);

    private static readonly Expression Functions = Expression.Property(null, typeof(EF).GetProperty(nameof(EF.Functions))!);

    private static readonly MethodInfo VectorDistanceMethod = typeof(CosmosDbFunctionsExtensions)
        .GetMethods()
        .Single(
            m => m.Name == nameof(CosmosDbFunctionsExtensions.VectorDistance)
                && m.GetParameters()[1].ParameterType == ReadOnlyMemoryType);

    private static readonly MethodInfo RrfMethod = typeof(CosmosDbFunctionsExtensions)
        .GetMethods()
        .Single(
            m => m.Name == nameof(CosmosDbFunctionsExtensions.Rrf)
                && m.GetParameters().Length == 2
                && m.GetParameters()[1].ParameterType == typeof(double[]));

    private static readonly PropertyInfo DistanceFunctionProperty = typeof(VectorDistanceOptions)
        .GetProperty(nameof(VectorDistanceOptions.DistanceFunction))!;

    private static readonly VectorSearchData VectorData = CreateVectorData();

    private static FieldInfo VectorParameterField<TParameter>()
        => typeof(VectorParameterHolder<TParameter>).GetField(
            "_inputVector",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static VectorSearchData CreateVectorData()
    {
        if (typeof(TVector) == typeof(float))
        {
            return new(
                (TVector[])(object)new[] { 0.33f, -0.52f, 0.45f, -0.67f, 0.89f, -0.34f, 0.86f, -0.78f, 0.86f, -0.78f },
                (TVector[])(object)new[] { 0.13f, -0.42f, 0.25f, -0.37f, 0.69f, -0.14f, 0.66f, -0.58f, 0.46f, -0.28f },
                "[0.33,-0.52,0.45,-0.67,0.89,-0.34,0.86,-0.78,0.86,-0.78]",
                "[0.13,-0.42,0.25,-0.37,0.69,-0.14,0.66,-0.58,0.46,-0.28]",
                "float32",
                "Single");
        }

        if (typeof(TVector) == typeof(byte))
        {
            return new(
                (TVector[])(object)new byte[] { 2, 1, 4, 6, 5, 2, 5, 7, 3, 1 },
                (TVector[])(object)new byte[] { 1, 3, 5, 7, 9, 2, 4, 6, 8, 10 },
                "[2,1,4,6,5,2,5,7,3,1]",
                "[1,3,5,7,9,2,4,6,8,10]",
                "uint8",
                "Byte");
        }

        if (typeof(TVector) == typeof(sbyte))
        {
            return new(
                (TVector[])(object)new sbyte[] { 2, 1, 4, 6, 5, 2, 5, 7, 3, 1 },
                (TVector[])(object)new sbyte[] { 1, 3, 5, 7, 9, 2, 4, 6, 8, 10 },
                "[2,1,4,6,5,2,5,7,3,1]",
                "[1,3,5,7,9,2,4,6,8,10]",
                "int8",
                "SByte");
        }

        throw new UnreachableException();
    }

    private sealed record VectorSearchData(
        TVector[] InputVector,
        TVector[] AlternateInputVector,
        string SqlLiteral,
        string AlternateSqlLiteral,
        string DataType,
        string StoreNameSuffix)
    {
        public int Dimensions
            => InputVector.Length;
    }

    private sealed class VectorParameterHolder<TParameter>(TParameter inputVector)
    {
        private readonly TParameter _inputVector = inputVector;
    }

    public class VectorSearchFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => $"VectorSearch{VectorData.StoreNameSuffix}Test";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            => modelBuilder.Entity<Book>(b =>
            {
                b.Property(e => e.Id).ValueGeneratedOnAdd();
                b.HasKey(e => e.Id);
                b.HasPartitionKey(e => e.Publisher);

                b.HasIndex(e => e.Vector).IsVectorIndex(VectorIndexType.Flat);
                b.HasIndex(e => e.VectorArray).IsVectorIndex(VectorIndexType.Flat);

                b.Property(e => e.Vector).IsVectorProperty(DistanceFunction.Cosine, VectorData.Dimensions);
                b.Property(e => e.VectorArray).IsVectorProperty(DistanceFunction.Cosine, VectorData.Dimensions);

                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.OwnsOne(
                            x => x.NestedOwned, bbb =>
                            {
                                bbb.HasIndex(x => x.NestedVector).IsVectorIndex(VectorIndexType.Flat);
                                bbb.Property(x => x.NestedVector).IsVectorProperty(DistanceFunction.Cosine, VectorData.Dimensions);
                            });

                        bb.OwnsMany(x => x.NestedOwnedCollection, bbb => bbb.Ignore(x => x.NestedVector));
                    });

                b.OwnsMany(
                    x => x.OwnedCollection, bb =>
                    {
                        bb.OwnsOne(x => x.NestedOwned, bbb => bbb.Ignore(x => x.NestedVector));
                        bb.OwnsMany(x => x.NestedOwnedCollection, bbb => bbb.Ignore(x => x.NestedVector));
                    });

                // issue #35898: vector indexes on collection wildcard paths are not supported
                //if (!CosmosTestEnvironment.IsEmulator)
                //{
                //    b.ComplexCollection(x => x.ComplexNestedCollection, cb => cb.Property(c => c.NestedSingles).IsVectorProperty(DistanceFunction.Cosine, 10));
                //    b.HasIndex(x => x.ComplexNestedCollection.Select(c => c.NestedSingles)).IsVectorIndex(VectorIndexType.Flat);
                //}
                //else
                //{
                b.Ignore(x => x.ComplexNestedCollection);
                //}
            });

        protected override Task SeedAsync(PoolableDbContext context)
        {
            var book1 = CreateBook(
                publisher: "Manning",
                author: "Jon P Smith",
                title: "Entity Framework Core in Action");

            var book2 = CreateBook(
                publisher: "O'Reilly",
                author: "Julie Lerman",
                title: "Programming Entity Framework: DbContext");

            var book3 = CreateBook(
                publisher: "O'Reilly",
                author: "Julie Lerman",
                title: "Programming Entity Framework");

            context.AddRange(book1, book2, book3);

            return context.SaveChangesAsync();
        }

        private static Book CreateBook(string publisher, string author, string title)
            => new()
            {
                Publisher = publisher,
                Author = author,
                Title = title,
                NonVector = new ReadOnlyMemory<TVector>(VectorData.AlternateInputVector),
                Vector = new ReadOnlyMemory<TVector>(VectorData.InputVector),
                VectorArray = VectorData.InputVector.ToArray(),
                OwnedReference = new Owned1
                {
                    Prop = 7,
                    NestedOwned = new Owned2
                    {
                        Prop = "7",
                        NestedVector = new ReadOnlyMemory<TVector>(VectorData.InputVector)
                    },
                    NestedOwnedCollection = [new() { Prop = "71" }, new() { Prop = "72" }]
                },
                OwnedCollection = [new() { Prop = 71 }, new() { Prop = 72 }],
                ComplexNestedCollection =
                [
                    new ComplexNested
                    {
                        NestedVector = new ReadOnlyMemory<TVector>(VectorData.InputVector)
                    }
                ]
            };

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }
}
