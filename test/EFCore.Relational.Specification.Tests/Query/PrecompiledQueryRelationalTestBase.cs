// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Xunit.Sdk;
using static Microsoft.EntityFrameworkCore.TestUtilities.PrecompiledQueryTestHelpers;

namespace Microsoft.EntityFrameworkCore.Query;

// ReSharper disable InconsistentNaming
/// <summary>
///     General tests for precompiled queries.
///     See also <see cref="PrecompiledSqlPregenerationQueryRelationalTestBase" /> for tests specifically related to SQL pregeneration.
/// </summary>
[Collection("PrecompiledQuery")]
public class PrecompiledQueryRelationalTestBase
{
    public PrecompiledQueryRelationalTestBase(PrecompiledQueryRelationalFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        TestOutputHelper = testOutputHelper;

        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Expression types

    [ConditionalFact]
    public virtual Task BinaryExpression()
        => Test(
            """
var id = 3;
var blogs = await context.Blogs.Where(b => b.Id > id).ToListAsync();

Assert.Equal(2, blogs.Count);
var orderedBlogs = blogs.OrderBy(x => x.Id).ToList();
var blog1 = orderedBlogs[0];
var blog2 = orderedBlogs[1];

Assert.Equal(8, blog1.Id);
Assert.Equal("Blog1", blog1.Name);
Assert.Empty(blog1.Json);

Assert.Equal(9, blog2.Id);
Assert.Equal("Blog2", blog2.Name);
Assert.Equal(2, blog2.Json.Count);

Assert.Equal(1, blog2.Json[0].Number);
Assert.Equal("One", blog2.Json[0].Text);
Assert.Equal(new DateTime(2001, 1, 1), blog2.Json[0].Inner.Date);

Assert.Equal(2, blog2.Json[1].Number);
Assert.Equal("Two", blog2.Json[1].Text);
Assert.Equal(new DateTime(2002, 2, 2), blog2.Json[1].Inner.Date);
""");

    [ConditionalFact]
    public virtual Task Conditional_no_evaluatable()
        => Test(
            """
var id = 3;
var blogs = await context.Blogs.Select(b => b.Id == 2 ? "yes" : "no").ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Conditional_contains_captured_variable()
        => Test(
            """
var yes = "yes";
var blogs = await context.Blogs.Select(b => b.Id == 2 ? yes : "no").ToListAsync();
""");

    // We do not support embedding Expression builder API calls into the query; this would require CSharpToLinqTranslator to actually
    // evaluate those APIs and embed the results into the tree. It's (at least potentially) a form of dynamic query, unsupported for now.
    [ConditionalFact]
    public virtual Task Invoke_no_evaluatability_is_not_supported()
        => Test(
            """
Expression<Func<Blog, bool>> lambda = b => b.Name == "foo";
var parameter = Expression.Parameter(typeof(Blog), "b");

var blogs = await context.Blogs
    .Where(Expression.Lambda<Func<Blog, bool>>(Expression.Invoke(lambda, parameter), parameter))
    .ToListAsync();
""",
            errorAsserter: errors => Assert.IsType<ArgumentNullException>(errors.Single().Exception));

    [ConditionalFact]
    public virtual Task ListInit_no_evaluatability()
        => Test("_ = await context.Blogs.Select(b => new List<int> { b.Id, b.Id + 1 }).ToListAsync();");

    [ConditionalFact]
    public virtual Task ListInit_with_evaluatable_with_captured_variable()
        => Test(
            """
var i = 1;
_ = await context.Blogs.Select(b => new List<int> { b.Id, i }).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task ListInit_with_evaluatable_without_captured_variable()
        => Test(
            """
var i = 1;
_ = await context.Blogs.Select(b => new List<int> { b.Id, 8 }).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task ListInit_fully_evaluatable()
        => Test(
            """
var blog = await context.Blogs.Where(b => new List<int> { 7, 8 }.Contains(b.Id)).SingleAsync();
Assert.Equal("Blog1", blog.Name);
""");

    [ConditionalFact]
    public virtual Task MethodCallExpression_no_evaluatability()
        => Test("_ = await context.Blogs.Where(b => b.Name.StartsWith(b.Name)).ToListAsync();");

    [ConditionalFact]
    public virtual Task MethodCallExpression_with_evaluatable_with_captured_variable()
        => Test(
            """
var pattern = "foo";
_ = await context.Blogs.Where(b => b.Name.StartsWith(pattern)).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task MethodCallExpression_with_evaluatable_without_captured_variable()
        => Test("""_ = await context.Blogs.Where(b => b.Name.StartsWith("foo")).ToListAsync();""");

    [ConditionalFact]
    public virtual Task MethodCallExpression_fully_evaluatable()
        => Test("""_ = await context.Blogs.Where(b => "foobar".StartsWith("foo")).ToListAsync();""");

    [ConditionalFact]
    public virtual Task New_with_no_arguments()
        => Test(
            """
var i = 8;
_ = await context.Blogs.Where(b => b == new Blog()).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Where_New_with_captured_variable()
        => Test(
            """
var i = 8;
_ = await context.Blogs.Where(b => b == new Blog(i, b.Name)).ToListAsync();
""",
            errorAsserter: errors => Assert.StartsWith("Translation of", errors.Single().Exception.Message));

    [ConditionalFact]
    public virtual Task Select_New_with_captured_variable()
        => Test(
            """
var i = 8;
_ = await context.Blogs.Select(b => new Blog(i, b.Name)).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task MemberInit_no_evaluatable()
        => Test("_ = await context.Blogs.Select(b => new Blog { Id = b.Id, Name = b.Name }).ToListAsync();");

    [ConditionalFact]
    public virtual Task MemberInit_contains_captured_variable()
        => Test(
            """
var id = 8;
_ = await context.Blogs.Select(b => new Blog { Id = id, Name = b.Name }).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task MemberInit_evaluatable_as_constant()
        => Test("""_ = await context.Blogs.Select(b => new Blog { Id = 1, Name = "foo" }).ToListAsync();""");

    [ConditionalFact]
    public virtual Task MemberInit_evaluatable_as_parameter()
        => Test(
            """
var id = 8;
var foo = "foo";
_ = await context.Blogs.Select(b => new Blog { Id = id, Name = foo }).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task NewArray()
        => Test(
            """
var i = 8;
_ = await context.Blogs.Select(b => new[] { b.Id, b.Id + i }).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Unary()
        => Test("_ = await context.Blogs.Where(b => (short)b.Id == (short)8).ToListAsync();");

    #endregion Expression types

    #region Regular operators

    [ConditionalFact]
    public virtual Task OrderBy()
        => Test("_ = await context.Blogs.OrderBy(b => b.Name).ToListAsync();");

    [ConditionalFact]
    public virtual Task Skip_with_constant()
        => Test("_ = await context.Blogs.OrderBy(b => b.Name).Skip(1).ToListAsync();");

    [ConditionalFact]
    public virtual Task Skip_with_parameter()
        => Test(
            """
var toSkip = 1;
_ = await context.Blogs.OrderBy(b => b.Name).Skip(toSkip).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Take_with_constant()
        => Test("_ = await context.Blogs.OrderBy(b => b.Name).Take(1).ToListAsync();");

    [ConditionalFact]
    public virtual Task Take_with_parameter()
        => Test(
            """
var toTake = 1;
_ = await context.Blogs.OrderBy(b => b.Name).Take(toTake).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Select_changes_type()
        => Test("_ = await context.Blogs.Select(b => b.Name).ToListAsync();");

    [ConditionalFact]
    public virtual Task Select_anonymous_object()
        => Test("""_ = await context.Blogs.Select(b => new { Foo = b.Name + "Foo" }).ToListAsync();""");

    [ConditionalFact]
    public virtual Task Include_single()
        => Test("var blogs = await context.Blogs.Include(b => b.Posts).Where(b => b.Id > 8).ToListAsync();");

    [ConditionalFact]
    public virtual Task Include_split()
        => Test("var blogs = await context.Blogs.AsSplitQuery().Include(b => b.Posts).ToListAsync();");

    [ConditionalFact]
    public virtual Task Final_GroupBy()
        => Test("""var blogs = await context.Blogs.GroupBy(b => b.Name).ToListAsync();""");

    #endregion Regular operators

    #region Terminating operators

    [ConditionalFact]
    public virtual Task Terminating_AsEnumerable()
        => Test(
            """
var blogs = context.Blogs.AsEnumerable().ToList();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_AsAsyncEnumerable_on_DbSet()
        => Test(
            """
var sum = 0;
await foreach (var blog in context.Blogs.AsAsyncEnumerable())
{
    sum += blog.Id;
}
Assert.Equal(17, sum);
""");

    [ConditionalFact]
    public virtual Task Terminating_AsAsyncEnumerable_on_IQueryable()
        => Test(
            """
var sum = 0;
await foreach (var blog in context.Blogs.Where(b => b.Id > 8).AsAsyncEnumerable())
{
    sum += blog.Id;
}
Assert.Equal(9, sum);
""");

    [ConditionalFact]
    public virtual Task Foreach_sync_over_operator()
        => Test(
            """
foreach (var blog in context.Blogs.Where(b => b.Id > 8))
{
}
""");

    [ConditionalFact]
    public virtual Task Terminating_ToArray()
        => Test(
            """
var blogs = context.Blogs.ToArray();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_ToArrayAsync()
        => Test(
            """
var blogs = await context.Blogs.ToArrayAsync();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_ToDictionary()
        => Test(
            """
var blogs = context.Blogs.ToDictionary(b => b.Id, b => b.Name);
Assert.Equal(2, blogs.Count);
Assert.Equal("Blog1", blogs[8]);
Assert.Equal("Blog2", blogs[9]);
""");

    [ConditionalFact]
    public virtual Task Terminating_ToDictionaryAsync()
        => Test(
            """
var blogs = await context.Blogs.ToDictionaryAsync(b => b.Id, b => b.Name);
Assert.Equal(2, blogs.Count);
Assert.Equal("Blog1", blogs[8]);
Assert.Equal("Blog2", blogs[9]);
""");

    [ConditionalFact]
    public virtual Task ToDictionary_over_anonymous_type()
        => Test("_ = context.Blogs.Select(b => new { b.Id, b.Name }).ToDictionary(x => x.Id, x => x.Name);");

    [ConditionalFact]
    public virtual Task ToDictionaryAsync_over_anonymous_type()
        => Test("_ = await context.Blogs.Select(b => new { b.Id, b.Name }).ToDictionaryAsync(x => x.Id, x => x.Name);");

    [ConditionalFact]
    public virtual Task Terminating_ToHashSet()
        => Test(
            """
var blogs = context.Blogs.ToHashSet();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_ToHashSetAsync()
        => Test(
            """
var blogs = await context.Blogs.ToHashSetAsync();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_ToLookup()
        => Test("_ = context.Blogs.ToLookup(b => b.Name);");

    [ConditionalFact]
    public virtual Task Terminating_ToList()
        => Test(
            """
var blogs = context.Blogs.ToList();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_ToListAsync()
        => Test(
            """
var blogs = await context.Blogs.ToListAsync();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    // foreach/await foreach directly over DbSet properties doesn't isn't supported, since we can't intercept property accesses.
    [ConditionalFact]
    public virtual async Task Foreach_sync_over_DbSet_property_is_not_supported()
    {
        // TODO: Assert diagnostics about non-intercepted query
        var exception = await Assert.ThrowsAsync<FailException>(
            () => Test(
                """
foreach (var blog in context.Blogs)
{
}
"""));
        Assert.Equal(NonCompilingQueryCompiler.ErrorMessage, exception.Message);
    }

    // foreach/await foreach directly over DbSet properties doesn't isn't supported, since we can't intercept property accesses.
    [ConditionalFact]
    public virtual async Task Foreach_async_is_not_supported()
    {
        // TODO: Assert diagnostics about non-intercepted query
        var exception = await Assert.ThrowsAsync<FailException>(
            () => Test(
                """
await foreach (var blog in context.Blogs)
{
}
"""));
        Assert.Equal(NonCompilingQueryCompiler.ErrorMessage, exception.Message);
    }

    #endregion Terminating operators

    #region Reducing terminating operators

    [ConditionalFact]
    public virtual Task Terminating_All()
        => Test(
            """
Assert.True(context.Blogs.All(b => b.Id > 7));
Assert.False(context.Blogs.All(b => b.Id > 8));
""");

    [ConditionalFact]
    public virtual Task Terminating_AllAsync()
        => Test(
            """
Assert.True(await context.Blogs.AllAsync(b => b.Id > 7));
Assert.False(await context.Blogs.AllAsync(b => b.Id > 8));
""");

    [ConditionalFact]
    public virtual Task Terminating_Any()
        => Test(
            """
Assert.True(context.Blogs.Where(b => b.Id > 7).Any());
Assert.False(context.Blogs.Where(b => b.Id < 7).Any());

Assert.True(context.Blogs.Any(b => b.Id > 7));
Assert.False(context.Blogs.Any(b => b.Id < 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_AnyAsync()
        => Test(
            """
Assert.True(await context.Blogs.Where(b => b.Id > 7).AnyAsync());
Assert.False(await context.Blogs.Where(b => b.Id < 7).AnyAsync());

Assert.True(await context.Blogs.AnyAsync(b => b.Id > 7));
Assert.False(await context.Blogs.AnyAsync(b => b.Id < 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_Average()
        => Test(
            """
Assert.Equal(8.5, context.Blogs.Select(b => b.Id).Average());
Assert.Equal(8.5, context.Blogs.Average(b => b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_AverageAsync()
        => Test(
            """
Assert.Equal(8.5, await context.Blogs.Select(b => b.Id).AverageAsync());
Assert.Equal(8.5, await context.Blogs.AverageAsync(b => b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_Contains()
        => Test(
            """
Assert.True(context.Blogs.Select(b => b.Id).Contains(8));
Assert.False(context.Blogs.Select(b => b.Id).Contains(7));
""");

    [ConditionalFact]
    public virtual Task Terminating_ContainsAsync()
        => Test(
            """
Assert.True(await context.Blogs.Select(b => b.Id).ContainsAsync(8));
Assert.False(await context.Blogs.Select(b => b.Id).ContainsAsync(7));
""");

    [ConditionalFact]
    public virtual Task Terminating_Count()
        => Test(
            """
Assert.Equal(2, context.Blogs.Count());
Assert.Equal(1, context.Blogs.Count(b => b.Id > 8));
""");

    [ConditionalFact]
    public virtual Task Terminating_CountAsync()
        => Test(
            """
Assert.Equal(2, await context.Blogs.CountAsync());
Assert.Equal(1, await context.Blogs.CountAsync(b => b.Id > 8));
""");

    [ConditionalFact]
    public virtual Task Terminating_ElementAt()
        => Test(
            """
Assert.Equal("Blog2", context.Blogs.OrderBy(b => b.Id).ElementAt(1).Name);
Assert.Throws<InvalidOperationException>(() => context.Blogs.OrderBy(b => b.Id).ElementAt(3));
""");

    [ConditionalFact]
    public virtual Task Terminating_ElementAtAsync()
        => Test(
            """
Assert.Equal("Blog2", (await context.Blogs.OrderBy(b => b.Id).ElementAtAsync(1)).Name);
await Assert.ThrowsAsync<InvalidOperationException>(() => context.Blogs.OrderBy(b => b.Id).ElementAtAsync(3));
""");

    [ConditionalFact]
    public virtual Task Terminating_ElementAtOrDefault()
        => Test(
            """
Assert.Equal("Blog2", context.Blogs.OrderBy(b => b.Id).ElementAtOrDefault(1).Name);
Assert.Null(context.Blogs.OrderBy(b => b.Id).ElementAtOrDefault(3));
""");

    [ConditionalFact]
    public virtual Task Terminating_ElementAtOrDefaultAsync()
        => Test(
            """
Assert.Equal("Blog2", (await context.Blogs.OrderBy(b => b.Id).ElementAtOrDefaultAsync(1)).Name);
Assert.Null(await context.Blogs.OrderBy(b => b.Id).ElementAtOrDefaultAsync(3));
""");

    [ConditionalFact]
    public virtual Task Terminating_First()
        => Test(
            """
Assert.Equal("Blog1", context.Blogs.Where(b => b.Id == 8).First().Name);
Assert.Throws<InvalidOperationException>(() => context.Blogs.Where(b => b.Id == 7).First());

Assert.Equal("Blog1", context.Blogs.First(b => b.Id == 8).Name);
Assert.Throws<InvalidOperationException>(() => context.Blogs.First(b => b.Id == 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_FirstAsync()
        => Test(
            """
Assert.Equal("Blog1", (await context.Blogs.Where(b => b.Id == 8).FirstAsync()).Name);
await Assert.ThrowsAsync<InvalidOperationException>(() => context.Blogs.Where(b => b.Id == 7).FirstAsync());

Assert.Equal("Blog1", (await context.Blogs.FirstAsync(b => b.Id == 8)).Name);
await Assert.ThrowsAsync<InvalidOperationException>(() => context.Blogs.FirstAsync(b => b.Id == 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_FirstOrDefault()
        => Test(
            """
Assert.Equal("Blog1", context.Blogs.Where(b => b.Id == 8).FirstOrDefault().Name);
Assert.Null(context.Blogs.Where(b => b.Id == 7).FirstOrDefault());

Assert.Equal("Blog1", context.Blogs.FirstOrDefault(b => b.Id == 8).Name);
Assert.Null(context.Blogs.FirstOrDefault(b => b.Id == 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_FirstOrDefaultAsync()
        => Test(
            """
Assert.Equal("Blog1", (await context.Blogs.Where(b => b.Id == 8).FirstOrDefaultAsync()).Name);
Assert.Null(await context.Blogs.Where(b => b.Id == 7).FirstOrDefaultAsync());

Assert.Equal("Blog1", (await context.Blogs.FirstOrDefaultAsync(b => b.Id == 8)).Name);
Assert.Null(await context.Blogs.FirstOrDefaultAsync(b => b.Id == 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_GetEnumerator()
        => Test(
            """
using var enumerator = context.Blogs.Where(b => b.Id == 8).GetEnumerator();
Assert.True(enumerator.MoveNext());
Assert.Equal("Blog1", enumerator.Current.Name);
Assert.False(enumerator.MoveNext());
""");

    [ConditionalFact]
    public virtual Task Terminating_Last()
        => Test(
            """
Assert.Equal("Blog2", context.Blogs.OrderBy(b => b.Id).Last().Name);
Assert.Throws<InvalidOperationException>(() => context.Blogs.OrderBy(b => b.Id).Where(b => b.Id == 7).Last());

Assert.Equal("Blog1", context.Blogs.OrderBy(b => b.Id).Last(b => b.Id == 8).Name);
Assert.Throws<InvalidOperationException>(() => context.Blogs.OrderBy(b => b.Id).Last(b => b.Id == 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_LastAsync()
        => Test(
            """
Assert.Equal("Blog2", (await context.Blogs.OrderBy(b => b.Id).LastAsync()).Name);
await Assert.ThrowsAsync<InvalidOperationException>(() => context.Blogs.OrderBy(b => b.Id).Where(b => b.Id == 7).LastAsync());

Assert.Equal("Blog1", (await context.Blogs.OrderBy(b => b.Id).LastAsync(b => b.Id == 8)).Name);
await Assert.ThrowsAsync<InvalidOperationException>(() => context.Blogs.OrderBy(b => b.Id).LastAsync(b => b.Id == 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_LastOrDefault()
        => Test(
            """
Assert.Equal("Blog2", context.Blogs.OrderBy(b => b.Id).LastOrDefault().Name);
Assert.Null(context.Blogs.OrderBy(b => b.Id).Where(b => b.Id == 7).LastOrDefault());

Assert.Equal("Blog1", context.Blogs.OrderBy(b => b.Id).LastOrDefault(b => b.Id == 8).Name);
Assert.Null(context.Blogs.OrderBy(b => b.Id).LastOrDefault(b => b.Id == 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_LastOrDefaultAsync()
        => Test(
            """
Assert.Equal("Blog2", (await context.Blogs.OrderBy(b => b.Id).LastOrDefaultAsync()).Name);
Assert.Null(await context.Blogs.OrderBy(b => b.Id).Where(b => b.Id == 7).LastOrDefaultAsync());

Assert.Equal("Blog1", (await context.Blogs.OrderBy(b => b.Id).LastOrDefaultAsync(b => b.Id == 8)).Name);
Assert.Null(await context.Blogs.OrderBy(b => b.Id).LastOrDefaultAsync(b => b.Id == 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_LongCount()
        => Test(
            """
Assert.Equal(2, context.Blogs.LongCount());
Assert.Equal(1, context.Blogs.LongCount(b => b.Id == 8));
""");

    [ConditionalFact]
    public virtual Task Terminating_LongCountAsync()
        => Test(
            """
Assert.Equal(2, await context.Blogs.LongCountAsync());
Assert.Equal(1, await context.Blogs.LongCountAsync(b => b.Id == 8));
""");

    [ConditionalFact]
    public virtual Task Terminating_Max()
        => Test(
            """
Assert.Equal(9, context.Blogs.Select(b => b.Id).Max());
Assert.Equal(9, context.Blogs.Max(b => b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_MaxAsync()
        => Test(
            """
Assert.Equal(9, await context.Blogs.Select(b => b.Id).MaxAsync());
Assert.Equal(9, await context.Blogs.MaxAsync(b => b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_Min()
        => Test(
            """
Assert.Equal(8, context.Blogs.Select(b => b.Id).Min());
Assert.Equal(8, context.Blogs.Min(b => b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_MinAsync()
        => Test(
            """
Assert.Equal(8, await context.Blogs.Select(b => b.Id).MinAsync());
Assert.Equal(8, await context.Blogs.MinAsync(b => b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_Single()
        => Test(
            """
Assert.Equal("Blog1", context.Blogs.Where(b => b.Id == 8).Single().Name);
Assert.Throws<InvalidOperationException>(() => context.Blogs.Where(b => b.Id == 7).Single());

Assert.Equal("Blog1", context.Blogs.Single(b => b.Id == 8).Name);
Assert.Throws<InvalidOperationException>(() => context.Blogs.Single(b => b.Id == 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_SingleAsync()
        => Test(
            """
Assert.Equal("Blog1", (await context.Blogs.Where(b => b.Id == 8).SingleAsync()).Name);
await Assert.ThrowsAsync<InvalidOperationException>(() => context.Blogs.Where(b => b.Id == 7).SingleAsync());

Assert.Equal("Blog1", (await context.Blogs.SingleAsync(b => b.Id == 8)).Name);
await Assert.ThrowsAsync<InvalidOperationException>(() => context.Blogs.SingleAsync(b => b.Id == 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_SingleOrDefault()
        => Test(
            """
Assert.Equal("Blog1", context.Blogs.Where(b => b.Id == 8).SingleOrDefault().Name);
Assert.Null(context.Blogs.Where(b => b.Id == 7).SingleOrDefault());

Assert.Equal("Blog1", context.Blogs.SingleOrDefault(b => b.Id == 8).Name);
Assert.Null(context.Blogs.SingleOrDefault(b => b.Id == 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_SingleOrDefaultAsync()
        => Test(
            """
Assert.Equal("Blog1", (await context.Blogs.Where(b => b.Id == 8).SingleOrDefaultAsync()).Name);
Assert.Null(await context.Blogs.Where(b => b.Id == 7).SingleOrDefaultAsync());

Assert.Equal("Blog1", (await context.Blogs.SingleOrDefaultAsync(b => b.Id == 8)).Name);
Assert.Null(await context.Blogs.SingleOrDefaultAsync(b => b.Id == 7));
""");

    [ConditionalFact]
    public virtual Task Terminating_Sum()
        => Test(
            """
Assert.Equal(17, context.Blogs.Select(b => b.Id).Sum());
Assert.Equal(17, context.Blogs.Sum(b => b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_SumAsync()
        => Test(
            """
Assert.Equal(17, await context.Blogs.Select(b => b.Id).SumAsync());
Assert.Equal(17, await context.Blogs.SumAsync(b => b.Id));
""");

    [ConditionalFact]
    public virtual Task Terminating_ExecuteDelete()
        => Test(
            """
await context.Database.BeginTransactionAsync();

var rowsAffected = context.Blogs.Where(b => b.Id > 8).ExecuteDelete();
Assert.Equal(1, rowsAffected);
Assert.Equal(1, await context.Blogs.CountAsync());
""");

    [ConditionalFact]
    public virtual Task Terminating_ExecuteDeleteAsync()
        => Test(
            """
await context.Database.BeginTransactionAsync();

var rowsAffected = await context.Blogs.Where(b => b.Id > 8).ExecuteDeleteAsync();
Assert.Equal(1, rowsAffected);
Assert.Equal(1, await context.Blogs.CountAsync());
""");

    [ConditionalFact]
    public virtual Task Terminating_ExecuteUpdate_with_lambda()
        => Test(
            """
await context.Database.BeginTransactionAsync();

var suffix = "Suffix";
var rowsAffected = context.Blogs.Where(b => b.Id > 8).ExecuteUpdate(setters => setters.SetProperty(b => b.Name, b => b.Name + suffix));
Assert.Equal(1, rowsAffected);
Assert.Equal(1, await context.Blogs.CountAsync(b => b.Id == 9 && b.Name == "Blog2Suffix"));
""");

    [ConditionalFact]
    public virtual Task Terminating_ExecuteUpdate_without_lambda()
        => Test(
            """
await context.Database.BeginTransactionAsync();

var newValue = "NewValue";
var rowsAffected = context.Blogs.Where(b => b.Id > 8).ExecuteUpdate(setters => setters.SetProperty(b => b.Name, newValue));
Assert.Equal(1, rowsAffected);
Assert.Equal(1, await context.Blogs.CountAsync(b => b.Id == 9 && b.Name == "NewValue"));
""");

    [ConditionalFact]
    public virtual Task Terminating_ExecuteUpdateAsync_with_lambda()
        => Test(
            """
await context.Database.BeginTransactionAsync();

var suffix = "Suffix";
var rowsAffected = await context.Blogs.Where(b => b.Id > 8).ExecuteUpdateAsync(setters => setters.SetProperty(b => b.Name, b => b.Name + suffix));
Assert.Equal(1, rowsAffected);
Assert.Equal(1, await context.Blogs.CountAsync(b => b.Id == 9 && b.Name == "Blog2Suffix"));
""");

    [ConditionalFact]
    public virtual Task Terminating_ExecuteUpdateAsync_without_lambda()
        => Test(
            """
await context.Database.BeginTransactionAsync();

var newValue = "NewValue";
var rowsAffected = await context.Blogs.Where(b => b.Id > 8).ExecuteUpdateAsync(setters => setters.SetProperty(b => b.Name, newValue));
Assert.Equal(1, rowsAffected);
Assert.Equal(1, await context.Blogs.CountAsync(b => b.Id == 9 && b.Name == "NewValue"));
""");

    [ConditionalFact] // #35494
    public virtual Task Terminating_with_cancellation_token()
        => Test(
            """
CancellationTokenSource source = new CancellationTokenSource();
CancellationToken token = source.Token;
Assert.Equal("Blog1", (await context.Blogs.Where(b => b.Id == 8).FirstOrDefaultAsync(token)).Name);
Assert.Null(await context.Blogs.Where(b => b.Id == 7).FirstOrDefaultAsync(token));
""");

    #endregion Reducing terminating operators

    #region SQL expression quotability

    [ConditionalFact]
    public virtual Task Union()
        => Test(
            """
var posts = await context.Posts.Where(p => p.Id > 11)
    .Union(context.Posts.Where(p => p.Id < 21))
    .OrderBy(p => p.Id)
    .ToListAsync();

Assert.Collection(posts,
    b => Assert.Equal(11, b.Id),
    b => Assert.Equal(12, b.Id),
    b => Assert.Equal(21, b.Id),
    b => Assert.Equal(22, b.Id),
    b => Assert.Equal(23, b.Id));
""");

    [ConditionalFact(Skip = "issue 33378")]
    public virtual Task UnionOnEntitiesWithJson()
        => Test(
            """
var blogs = await context.Blogs.Where(b => b.Id > 7)
    .Union(context.Blogs.Where(b => b.Id < 10))
    .OrderBy(b => b.Id)
    .ToListAsync();

Assert.Collection(blogs,
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [ConditionalFact]
    public virtual Task Concat()
        => Test(
            """
var posts = await context.Posts.Where(p => p.Id > 11)
    .Concat(context.Posts.Where(p => p.Id < 21))
    .OrderBy(p => p.Id)
    .ToListAsync();

Assert.Collection(posts,
    b => Assert.Equal(11, b.Id),
    b => Assert.Equal(12, b.Id),
    b => Assert.Equal(12, b.Id),
    b => Assert.Equal(21, b.Id),
    b => Assert.Equal(22, b.Id),
    b => Assert.Equal(23, b.Id));
""");

    [ConditionalFact(Skip = "issue 33378")]
    public virtual Task ConcatOnEntitiesWithJson()
        => Test(
            """
var blogs = await context.Blogs.Where(b => b.Id > 7)
    .Concat(context.Blogs.Where(b => b.Id < 10))
    .OrderBy(b => b.Id)
    .ToListAsync();

Assert.Collection(blogs,
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [ConditionalFact]
    public virtual Task Intersect()
        => Test(
            """
var posts = await context.Posts.Where(b => b.Id > 11)
    .Intersect(context.Posts.Where(b => b.Id < 22))
    .OrderBy(b => b.Id)
    .ToListAsync();

Assert.Collection(posts,
    b => Assert.Equal(12, b.Id),
    b => Assert.Equal(21, b.Id));
""");

    [ConditionalFact(Skip = "issue 33378")]
    public virtual Task IntersectOnEntitiesWithJson()
        => Test(
            """
var blogs = await context.Blogs.Where(b => b.Id > 7)
    .Intersect(context.Blogs.Where(b => b.Id > 8))
    .OrderBy(b => b.Id)
    .ToListAsync();

Assert.Collection(blogs, b => Assert.Equal(9, b.Id));
""");

    [ConditionalFact]
    public virtual Task Except()
        => Test(
            """
var posts = await context.Posts.Where(b => b.Id > 11)
    .Except(context.Posts.Where(b => b.Id > 21))
    .OrderBy(b => b.Id)
    .ToListAsync();

Assert.Collection(posts,
    b => Assert.Equal(12, b.Id),
    b => Assert.Equal(21, b.Id));
""");

    [ConditionalFact(Skip = "issue 33378")]
    public virtual Task ExceptOnEntitiesWithJson()
        => Test(
            """
var blogs = await context.Blogs.Where(b => b.Id > 7)
    .Except(context.Blogs.Where(b => b.Id > 8))
    .OrderBy(b => b.Id)
    .ToListAsync();

Assert.Collection(blogs, b => Assert.Equal(8, b.Id));
""");

    [ConditionalFact]
    public virtual Task ValuesExpression()
        => Test("_ = await context.Blogs.Where(b => new[] { 7, b.Id }.Count(i => i > 8) == 2).ToListAsync();");

    // Tests e.g. OPENJSON on SQL Server
    [ConditionalFact]
    public virtual Task Contains_with_parameterized_collection()
        => Test(
            """
int[] ids = [1, 2, 3];
_ = await context.Blogs.Where(b => ids.Contains(b.Id)).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task FromSqlRaw()
        => Test(
            $""""_ = await context.Blogs.FromSqlRaw("""{NormalizeDelimitersInRawString("SELECT * FROM [Blogs] WHERE [Id] > 8")}""").OrderBy(b => b.Id).ToListAsync();"""");

    [ConditionalFact]
    public virtual Task FromSql_with_FormattableString_parameters()
        => Test(
            $""""_ = await context.Blogs.FromSql($"""{NormalizeDelimitersInRawString("SELECT * FROM [Blogs] WHERE [Id] > {8} AND [Id] < {9}")}""").OrderBy(b => b.Id).ToListAsync();"""");

    #endregion SQL expression quotability

    #region Different DbContext expressions

    [ConditionalFact]
    public virtual Task DbContext_as_local_variable()
        => Test(
            """
var context2 = context;

_ = await context2.Blogs.ToListAsync();
""");

    [ConditionalFact]
    public virtual Task DbContext_as_field()
        => FullSourceTest(
            """
public static class TestContainer
{
    private static PrecompiledQueryContext _context;

    public static async Task Test(DbContextOptions dbContextOptions)
    {
        using (_context = new PrecompiledQueryContext(dbContextOptions))
        {
            var blogs = await _context.Blogs.ToListAsync();
            Assert.Collection(
                blogs.OrderBy(b => b.Id),
                b => Assert.Equal(8, b.Id),
                b => Assert.Equal(9, b.Id));
        }
    }
}
""");

    [ConditionalFact]
    public virtual Task DbContext_as_property()
        => FullSourceTest(
            """
public static class TestContainer
{
    private static PrecompiledQueryContext Context { get; set; }

    public static async Task Test(DbContextOptions dbContextOptions)
    {
        using (Context = new PrecompiledQueryContext(dbContextOptions))
        {
            var blogs = await Context.Blogs.ToListAsync();
            Assert.Collection(
                blogs.OrderBy(b => b.Id),
                b => Assert.Equal(8, b.Id),
                b => Assert.Equal(9, b.Id));
        }
    }
}
""");

    [ConditionalFact]
    public virtual Task DbContext_as_captured_variable()
        => Test(
            """
Func<List<Blog>> foo = () => context.Blogs.ToList();
_ = foo();
""");

    [ConditionalFact]
    public virtual Task DbContext_as_method_invocation_result()
        => FullSourceTest(
            """
public static class TestContainer
{
    private static PrecompiledQueryContext _context;

    public static async Task Test(DbContextOptions dbContextOptions)
    {
        using (_context = new PrecompiledQueryContext(dbContextOptions))
        {
            var blogs = await GetContext().Blogs.ToListAsync();
            Assert.Collection(
                blogs.OrderBy(b => b.Id),
                b => Assert.Equal(8, b.Id),
                b => Assert.Equal(9, b.Id));
        }
    }

    private static PrecompiledQueryContext GetContext()
        => _context;
}
""");

    #endregion Different DbContext expressions

    #region Captured variable handling

    [ConditionalFact]
    public virtual Task Two_captured_variables_in_same_lambda()
        => Test(
            """
var yes = "yes";
var no = "no";
var blogs = await context.Blogs.Select(b => b.Id == 3 ? yes : no).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Two_captured_variables_in_different_lambdas()
        => Test(
            """
var starts = "Blog";
var ends = "2";
var blog = await context.Blogs.Where(b => b.Name.StartsWith(starts)).Where(b => b.Name.EndsWith(ends)).SingleAsync();
Assert.Equal(9, blog.Id);
""");

    [ConditionalFact]
    public virtual Task Same_captured_variable_twice_in_same_lambda()
        => Test(
            """
var foo = "X";
var blogs = await context.Blogs.Where(b => b.Name.StartsWith(foo) && b.Name.EndsWith(foo)).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Same_captured_variable_twice_in_different_lambdas()
        => Test(
            """
var foo = "X";
var blogs = await context.Blogs.Where(b => b.Name.StartsWith(foo)).Where(b => b.Name.EndsWith(foo)).ToListAsync();
""");

    [ConditionalFact]
    public virtual Task Multiple_queries_with_captured_variables()
        => Test(
            """
var id1 = 8;
var id2 = 9;
var blogs = await context.Blogs.Where(b => b.Id == id1 || b.Id == id2).ToListAsync();
var blog1 = await context.Blogs.Where(b => b.Id == id1).SingleAsync();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
Assert.Equal("Blog1", blog1.Name);
""");

    #endregion Captured variable handling

    #region Negative cases

    [ConditionalFact]
    public virtual Task Dynamic_query_does_not_get_precompiled()
        => Test(
            """
var query = context.Blogs;
var blogs = await query.ToListAsync();
""",
            errorAsserter: errors =>
            {
                var dynamicQueryError = errors.Single();
                Assert.IsType<InvalidOperationException>(dynamicQueryError.Exception);
                Assert.Equal(DesignStrings.DynamicQueryNotSupported, dynamicQueryError.Exception.Message);
                Assert.Equal("query.ToListAsync()", dynamicQueryError.SyntaxNode.NormalizeWhitespace().ToFullString());
            });

    [ConditionalFact]
    public virtual Task ToList_over_objects_does_not_get_precompiled()
        => Test(
            """
int[] numbers = [1, 2, 3];
var lessNumbers = numbers.Where(i => i > 1).ToList();
""");

    [ConditionalFact]
    public virtual async Task Query_compilation_failure()
        => await Test(
            "_ = await context.Blogs.Where(b => PrecompiledQueryRelationalTestBase.Untranslatable(b.Id) == 999).ToListAsync();",
            errorAsserter: errors
                => Assert.Contains(
                    CoreStrings.TranslationFailedWithDetails(
                        "",
                        CoreStrings.QueryUnableToTranslateMethod(
                            "Microsoft.EntityFrameworkCore.Query.PrecompiledQueryRelationalTestBase",
                            "Untranslatable"))[21..],
                    errors.Single().Exception.Message));

    public static int Untranslatable(int foo)
        => throw new InvalidOperationException();

    [ConditionalFact]
    public virtual Task EF_Constant_is_not_supported()
        => Test(
            "_ = await context.Blogs.Where(b => b.Id > EF.Constant(8)).ToListAsync();",
            errorAsserter: errors
                => Assert.Equal(CoreStrings.EFConstantNotSupportedInPrecompiledQueries, errors.Single().Exception.Message));

    [ConditionalFact]
    public virtual Task NotParameterizedAttribute_with_constant()
        => Test(
            """
var blog = await context.Blogs.Where(b => EF.Property<string>(b, "Name") == "Blog2").SingleAsync();
Assert.Equal(9, blog.Id);
""");

    [ConditionalFact]
    public virtual Task NotParameterizedAttribute_is_not_supported_with_non_constant_argument()
        => Test(
            """
var propertyName = "Name";
var blog = await context.Blogs.Where(b => EF.Property<string>(b, propertyName) == "Blog2").SingleAsync();
""",
            errorAsserter: errors
                => Assert.Equal(
                    CoreStrings.NotParameterizedAttributeWithNonConstantNotSupportedInPrecompiledQueries("propertyName", "Property"),
                    errors.Single().Exception.Message));

    [ConditionalFact]
    public virtual Task Query_syntax_is_not_supported()
        => Test(
            """
var id = 3;
var blogs = await (
    from b in context.Blogs
    where b.Id > 8
    select b).ToListAsync();
""",
            errorAsserter: errors
                => Assert.Equal(DesignStrings.QueryComprehensionSyntaxNotSupportedInPrecompiledQueries, errors.Single().Exception.Message));

    #endregion Negative cases

    [ConditionalFact]
    public virtual Task Unsafe_accessor_gets_generated_once_for_multiple_queries()
        => Test(
            """
var blogs1 = await context.Blogs.ToListAsync();
var blogs2 = await context.Blogs.ToListAsync();
""",
            interceptorCodeAsserter: code => Assert.Equal(
                2, code.Split("private static extern ref int UnsafeAccessor_Microsoft_EntityFrameworkCore_Query_Blog_Id_Set").Length));

    public class PrecompiledQueryContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Blog>().OwnsMany(
                x => x.Json,
                n =>
                {
                    n.ToJson();
                    n.OwnsOne(xx => xx.Inner);
                });
            modelBuilder.Entity<Blog>().HasMany(x => x.Posts).WithOne(x => x.Blog).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>().Property(x => x.Id).ValueGeneratedNever();
        }
    }

    protected PrecompiledQueryRelationalFixture Fixture { get; }
    protected ITestOutputHelper TestOutputHelper { get; }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected virtual Task Test(
        string sourceCode,
        Action<string>? interceptorCodeAsserter = null,
        Action<List<PrecompiledQueryCodeGenerator.QueryPrecompilationError>>? errorAsserter = null,
        [CallerMemberName] string callerName = "")
        => Fixture.PrecompiledQueryTestHelpers.Test(
            """
await using var context = new PrecompiledQueryContext(dbContextOptions);

"""
            + sourceCode,
            Fixture.ServiceProvider.GetRequiredService<DbContextOptions>(),
            typeof(PrecompiledQueryContext),
            interceptorCodeAsserter,
            errorAsserter,
            TestOutputHelper,
            AlwaysPrintGeneratedSources,
            callerName);

    protected virtual Task FullSourceTest(
        string sourceCode,
        Action<string>? interceptorCodeAsserter = null,
        Action<List<PrecompiledQueryCodeGenerator.QueryPrecompilationError>>? errorAsserter = null,
        [CallerMemberName] string callerName = "")
        => Fixture.PrecompiledQueryTestHelpers.FullSourceTest(
            sourceCode,
            Fixture.ServiceProvider.GetRequiredService<DbContextOptions>(),
            typeof(PrecompiledQueryContext),
            interceptorCodeAsserter,
            errorAsserter,
            TestOutputHelper,
            AlwaysPrintGeneratedSources,
            callerName);

    protected virtual bool AlwaysPrintGeneratedSources
        => false;

    protected string NormalizeDelimitersInRawString(string sql)
        => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

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
        public List<JsonRoot> Json { get; set; } = new();
    }

    public class JsonRoot
    {
        public int Number { get; set; }
        public string? Text { get; set; }

        public JsonBranch Inner { get; set; } = null!;
    }

    public class JsonBranch
    {
        public DateTime Date { get; set; }
    }

    public class Post
    {
        public int Id { get; set; }
        public string? Title { get; set; }

        public Blog? Blog { get; set; }
    }

    public static readonly IEnumerable<object[]> IsAsyncData = [[false], [true]];
}
