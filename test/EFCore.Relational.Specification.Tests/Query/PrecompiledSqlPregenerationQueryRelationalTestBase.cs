// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Test suite for SQL pregeneration scenarios with precompiled queries.
///     See <see cref="PrecompiledQueryRelationalTestBase" /> for general precompiled query tests not related to
///     SQL pregeneration.
/// </summary>
public class PrecompiledSqlPregenerationQueryRelationalTestBase
{
    public PrecompiledSqlPregenerationQueryRelationalTestBase(
        PrecompiledSqlPregenerationQueryRelationalFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        TestOutputHelper = testOutputHelper;

        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected virtual bool AlwaysPrintGeneratedSources
        => false;

    [ConditionalFact]
    public virtual Task No_parameters()
        => Test("""var blogs = await context.Blogs.Where(b => b.Name == "foo").ToListAsync();""");

    [ConditionalFact]
    public virtual Task Non_nullable_value_type()
        => Test(
            """
int id = 8;
var blogs = await context.Blogs.Where(b => b.Id == id).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Nullable_value_type()
        => Test(
            """
int? id = 8;
var blogs = await context.Blogs.Where(b => b.Id == id).ToListAsync();
""",
            interceptorCodeAsserter: code =>
            {
                Assert.DoesNotContain(nameof(RelationalCommandCache), code);

                AssertContains(
                    """
if (parameters["__id_0"] == null)
{
    result = relationalCommandTemplate;
}
else
{
    result = relationalCommandTemplate0;
}
""", code);
            });

    [ConditionalFact]
    public virtual Task Nullable_reference_type()
        => Test(
            """
string? name = "bar";
var blogs = await context.Blogs.Where(b => b.Name == name).ToListAsync();
""",
            interceptorCodeAsserter: code =>
            {
                Assert.DoesNotContain(nameof(RelationalCommandCache), code);

                AssertContains(
                    """
if (parameters["__name_0"] == null)
{
    result = relationalCommandTemplate;
}
else
{
    result = relationalCommandTemplate0;
}
""", code);
            });

    [ConditionalFact]
    public virtual Task Non_nullable_reference_type()
        => Test(
            """
string name = "bar";
var blogs = await context.Blogs.Where(b => b.Name == name).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Nullable_and_non_nullable_value_types()
        => Test(
            """
int? id1 = 8;
int id2 = 9;
var blogs = await context.Blogs.Where(b => b.Id == id1 || b.Id == id2).ToListAsync();
""",
            interceptorCodeAsserter: code =>
            {
                Assert.DoesNotContain(nameof(RelationalCommandCache), code);

                AssertContains(
                    """
if (parameters["__id1_0"] == null)
{
    result = relationalCommandTemplate;
}
else
{
    result = relationalCommandTemplate0;
}
""", code);
            });

    [ConditionalFact]
    public virtual Task Two_nullable_reference_types()
        => Test(
            """
string? name1 = "foo";
string? name2 = "bar";
var blogs = await context.Blogs.Where(b => b.Name == name1 || b.Name == name2).ToListAsync();
""",
            interceptorCodeAsserter: code =>
            {
                Assert.DoesNotContain(nameof(RelationalCommandCache), code);

                AssertContains(
                    """
if (parameters["__name1_0"] == null)
{
    if (parameters["__name2_1"] == null)
    {
        result = relationalCommandTemplate;
    }
    else
    {
        result = relationalCommandTemplate0;
    }
}
else
{
    if (parameters["__name2_1"] == null)
    {
        result = relationalCommandTemplate1;
    }
    else
    {
        result = relationalCommandTemplate2;
    }
}
""", code);
            });

    [ConditionalFact]
    public virtual Task Two_non_nullable_reference_types()
        => Test(
            """
string name1 = "foo";
string name2 = "bar";
var blogs = await context.Blogs.Where(b => b.Name == name1 || b.Name == name2).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Nullable_and_non_nullable_reference_types()
        => Test(
            """
string? name1 = "foo";
string name2 = "bar";
var blogs = await context.Blogs.Where(b => b.Name == name1 || b.Name == name2).ToListAsync();
""",
            interceptorCodeAsserter: code =>
            {
                Assert.DoesNotContain(nameof(RelationalCommandCache), code);

                AssertContains(
                    """
if (parameters["__name1_0"] == null)
{
    result = relationalCommandTemplate;
}
else
{
    result = relationalCommandTemplate0;
}
""", code);
            });

    [ConditionalFact]
    public virtual Task Too_many_nullable_parameters_prevent_pregeneration()
        => Test(
            """
string? name1 = "foo";
string? name2 = "bar";
string? name3 = "baz";
string? name4 = "baq";
var blogs = await context.Blogs.Where(b => b.Name == name1 || b.Name == name2 || b.Name == name3 || b.Name == name4).ToListAsync();
""",
            interceptorCodeAsserter: code => Assert.Contains(nameof(RelationalCommandCache), code));

    [ConditionalFact]
    public virtual Task Many_non_nullable_parameters_do_not_prevent_pregeneration()
        => Test(
            """
string name1 = "foo";
string name2 = "bar";
string name3 = "baz";
string name4 = "baq";
var blogs = await context.Blogs.Where(b => b.Name == name1 || b.Name == name2 || b.Name == name3 || b.Name == name4).ToListAsync();
""");

    #region Tests for the different querying enumerables

    [ConditionalFact]
    public virtual Task Include_single_query()
        => Test(
            """
var blogs = await context.Blogs
    .Include(b => b.Posts)
    .ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Include_split_query()
        => Test(
            """
var blogs = await context.Blogs
    .Include(b => b.Posts)
    .AsSplitQuery()
    .ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Final_GroupBy()
        => Test("var blogs = await context.Blogs.GroupBy(b => b.Name).ToListAsync();");

    #endregion Tests for the different querying enumerables

    public class PrecompiledQueryContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs { get; set; } = null!;
    }

    protected PrecompiledSqlPregenerationQueryRelationalFixture Fixture { get; }
    protected ITestOutputHelper TestOutputHelper { get; }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    /// <summary>
    ///     Normalizes line endings and strips all leading whitespace before checking substring containment.
    /// </summary>
    private void AssertContains(string expected, string actual)
        => Assert.Contains(
            Regex.Replace(expected.ReplaceLineEndings(), @"^\s*", "", RegexOptions.Multiline),
            Regex.Replace(actual.ReplaceLineEndings(), @"^\s*", "", RegexOptions.Multiline));

    protected virtual async Task Test(
        string sourceCode,
        Action<string>? interceptorCodeAsserter = null,
        Action<List<PrecompiledQueryCodeGenerator.QueryPrecompilationError>>? errorAsserter = null,
        [CallerMemberName] string callerName = "")
    {
        // By default, make sure there's no mention of RelationalCommandCache in the generated interceptor code.
        // That means that SQL was pregenerated.
        interceptorCodeAsserter ??= code => Assert.DoesNotContain(nameof(RelationalCommandCache), code);

        await Fixture.PrecompiledQueryTestHelpers.Test(
            """
await using var context = new Microsoft.EntityFrameworkCore.Query.PrecompiledSqlPregenerationQueryRelationalTestBase.PrecompiledQueryContext(dbContextOptions);

"""
            + sourceCode,
            Fixture.ServiceProvider.GetRequiredService<DbContextOptions>(),
            typeof(PrecompiledQueryContext),
            interceptorCodeAsserter,
            errorAsserter,
            TestOutputHelper,
            AlwaysPrintGeneratedSources,
            callerName);
    }

    public class Blog
    {
        public Blog()
        {
        }

        public Blog(int id, string name)
        {
            Id = id;
            Name = name;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string? Name { get; set; }

        public List<Post> Posts { get; set; } = new();
    }

    public class Post
    {
        public int Id { get; set; }
        public string? Title { get; set; }

        public Blog? Blog { get; set; }
    }
}
