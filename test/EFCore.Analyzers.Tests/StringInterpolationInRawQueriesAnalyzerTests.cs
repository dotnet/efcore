// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis.Testing;

namespace Microsoft.EntityFrameworkCore;

using Verify = CSharpCodeFixVerifier<StringsUsageInRawQueriesDiagnosticAnalyzer, InterpolatedStringUsageInRawQueriesCodeFixProvider>;

//Issue #37106
internal class StringInterpolationInRawQueriesAnalyzerTests
{
    public static readonly TheoryData<string> DoNotReportData =
    [
        "db.Users.FromSqlRaw(",
        "db.Database.ExecuteSqlRaw(",
        "db.Database.ExecuteSqlRawAsync(",
        "db.Database.SqlQueryRaw<int>(",
        "RelationalQueryableExtensions.FromSqlRaw(db.Users, ",
    ];

    public static readonly TheoryData<string> ShouldReportData =
    [
        "db.Users.{|#0:FromSqlRaw|}(",
        "db.Database.{|#0:ExecuteSqlRaw|}(",
        "db.Database.{|#0:ExecuteSqlRawAsync|}(",
        "db.Database.{|#0:SqlQueryRaw|}<int>(",
        "RelationalQueryableExtensions.{|#0:FromSqlRaw|}(db.Users, ",
    ];

    public static readonly TheoryData<string, string> ShouldReportDataWithFix = new()
    {
        { "db.Users.{|#0:FromSqlRaw|}(", "db.Users.FromSql(" },
        { "db.Database.{|#0:ExecuteSqlRaw|}(", "db.Database.ExecuteSql(" },
        { "db.Database.{|#0:ExecuteSqlRawAsync|}(", "db.Database.ExecuteSqlAsync(" },
        { "db.Database.{|#0:SqlQueryRaw|}<int>(", "db.Database.SqlQuery<int>(" },
        { "RelationalQueryableExtensions.{|#0:FromSqlRaw|}(db.Users, ", "RelationalQueryableExtensions.FromSql(db.Users, " },
    };

    public static readonly TheoryData<string> CorrectCallsData =
    [
        "db.Users.FromSql(",
        "db.Users.FromSqlInterpolated(",
        "db.Database.ExecuteSql(",
        "db.Database.ExecuteSqlInterpolated(",
        "db.Database.ExecuteSqlAsync(",
        "db.Database.ExecuteSqlInterpolatedAsync(",
        "db.Database.SqlQuery<int>(",
    ];

    private const string MyDbContext =
        """
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

class MyDbContext : DbContext
{
    public DbSet<User> Users { get; init; }
}
""";

    [Theory]
    [MemberData(nameof(CorrectCallsData))]
    public Task Correct_interpolation_do_not_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        {{call}}$"FooBar WHERE Id = {id}");
    }
}
""");

    [Theory]
    [MemberData(nameof(DoNotReportData))]
    public Task Constant_string_do_not_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        {{call}}"FooBar WHERE Id = 1");
    }
}
""");

    [Theory]
    [MemberData(nameof(DoNotReportData))]
    public Task Constant_string_with_parameters_do_not_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        {{call}}"FooBar WHERE Id = {0}", id);
    }
}
""");

    [Theory]
    [MemberData(nameof(ShouldReportDataWithFix))]
    public Task Interpolated_string_should_report(string call, string fixedCall)
        => Verify.VerifyCodeFixAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        {{call}}$"FooBar WHERE Id = {id}");
    }
}
""",
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        {{fixedCall}}$"FooBar WHERE Id = {id}");
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.InterpolatedStringUsageInRawQueries).WithLocation(0));

    [Theory]
    [MemberData(nameof(ShouldReportData))]
    public async Task Interpolated_string_with_other_parameters_should_report(string call)
    {
        var source = $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        {{call}}$"FooBar WHERE Id = {id}", id);
    }
}
""";
        await Verify.VerifyCodeFixAsync(source, source,
            DiagnosticResult.CompilerWarning(EFDiagnostics.InterpolatedStringUsageInRawQueries).WithLocation(0));
    }

    [Theory]
    [MemberData(nameof(DoNotReportData))]
    public Task Constant_interpolated_string_do_not_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        const string id = "1";
        {{call}}$"FooBar WHERE Id = {id}");
    }
}
""");

    [Theory]
    [MemberData(nameof(DoNotReportData))]
    public Task Pseudo_constant_interpolated_string_do_not_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        {{call}}$"FooBar WHERE Id = {1}");
    }
}
""");

    [Fact]
    public Task Fix_all()
        => Verify.VerifyCodeFixAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.{|#0:FromSqlRaw|}($"FooBar WHERE Id = {id}");
        db.Users.{|#1:FromSqlRaw|}($"FooBar WHERE Id = {id}");
    }
}
""",
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.FromSql($"FooBar WHERE Id = {id}");
        db.Users.FromSql($"FooBar WHERE Id = {id}");
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.InterpolatedStringUsageInRawQueries).WithLocation(0),
            DiagnosticResult.CompilerWarning(EFDiagnostics.InterpolatedStringUsageInRawQueries).WithLocation(1));
}
