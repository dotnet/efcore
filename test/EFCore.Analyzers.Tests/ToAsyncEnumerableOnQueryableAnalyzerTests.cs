// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis.Testing;

namespace Microsoft.EntityFrameworkCore;

using Verify = CSharpCodeFixVerifier<ToAsyncEnumerableOnQueryableDiagnosticAnalyzer, ToAsyncEnumerableOnQueryableCodeFixProvider>;

public class ToAsyncEnumerableOnQueryableAnalyzerTests
{
    private const string MyDbContext =
        """
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

class User
{
    public int Id { get; set; }
}

class MyDbContext : DbContext
{
    public DbSet<User> Users { get; init; }
}
""";

    [Fact]
    public Task DbSet_ToAsyncEnumerable_should_report()
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        _ = db.Users.{|#0:ToAsyncEnumerable|}();
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.ToAsyncEnumerableOnQueryable).WithLocation(0));

    [Fact]
    public Task IQueryable_local_ToAsyncEnumerable_should_report()
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        IQueryable<User> q = db.Users.Where(u => u.Id > 0);
        _ = q.{|#0:ToAsyncEnumerable|}();
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.ToAsyncEnumerableOnQueryable).WithLocation(0));

    [Fact]
    public Task Chained_after_Where_should_report()
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        _ = db.Users.Where(u => u.Id > 0).Select(u => u.Id).{|#0:ToAsyncEnumerable|}();
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.ToAsyncEnumerableOnQueryable).WithLocation(0));

    [Fact]
    public Task IEnumerable_local_should_not_report()
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M()
    {
        IEnumerable<User> users = new List<User>();
        _ = users.ToAsyncEnumerable();
    }
}
""");

    [Fact]
    public Task Explicit_cast_to_IEnumerable_should_not_report()
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        // Explicit cast to IEnumerable<T> opts out — caller knows they're forcing sync iteration.
        _ = ((IEnumerable<User>)db.Users).ToAsyncEnumerable();
    }
}
""");

    [Fact]
    public Task AsAsyncEnumerable_should_not_report()
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        _ = db.Users.AsAsyncEnumerable();
    }
}
""");

    [Fact]
    public Task List_ToAsyncEnumerable_should_not_report()
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M()
    {
        var list = new List<User>();
        _ = list.ToAsyncEnumerable();
    }
}
""");

    [Fact]
    public Task Array_ToAsyncEnumerable_should_not_report()
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M()
    {
        var arr = new User[0];
        _ = arr.ToAsyncEnumerable();
    }
}
""");

    [Fact]
    public Task Static_call_form_should_report()
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        _ = AsyncEnumerable.{|#0:ToAsyncEnumerable|}<User>(db.Users);
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.ToAsyncEnumerableOnQueryable).WithLocation(0));

    // -- Code fix verification --

    [Fact]
    public Task CodeFix_DbSet_replaces_ToAsyncEnumerable_with_AsAsyncEnumerable()
        => Verify.VerifyCodeFixAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        _ = db.Users.{|#0:ToAsyncEnumerable|}();
    }
}
""",
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        _ = db.Users.AsAsyncEnumerable();
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.ToAsyncEnumerableOnQueryable).WithLocation(0));

    [Fact]
    public Task CodeFix_chained_query_replaces_method_name_only()
        => Verify.VerifyCodeFixAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        _ = db.Users.Where(u => u.Id > 0).Select(u => u.Id).{|#0:ToAsyncEnumerable|}();
    }
}
""",
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        _ = db.Users.Where(u => u.Id > 0).Select(u => u.Id).AsAsyncEnumerable();
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.ToAsyncEnumerableOnQueryable).WithLocation(0));

    [Fact]
    public async Task CodeFix_static_call_form_is_not_offered()
    {
        // The static-call form `AsyncEnumerable.ToAsyncEnumerable<T>(q)` would require a structural
        // rewrite (different containing class). The analyzer warns but no automated fix is offered;
        // VerifyCodeFixAsync with identical source/fixedSource asserts the fix does NOT change the code.
        var source = $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        _ = AsyncEnumerable.{|#0:ToAsyncEnumerable|}<User>(db.Users);
    }
}
""";
        await Verify.VerifyCodeFixAsync(
            source,
            source,
            DiagnosticResult.CompilerWarning(EFDiagnostics.ToAsyncEnumerableOnQueryable).WithLocation(0));
    }
}
