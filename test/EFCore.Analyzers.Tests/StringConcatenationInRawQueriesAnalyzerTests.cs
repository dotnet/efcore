// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis.Testing;

namespace Microsoft.EntityFrameworkCore;

using Verify = CSharpAnalyzerVerifier<StringsUsageInRawQueriesDiagnosticAnalyzer>;

public class StringConcatenationInRawQueriesAnalyzerTests
{
    public static readonly TheoryData<string> DoNotReportData =
    [
        "Users.FromSqlRaw",
        "Database.ExecuteSqlRaw",
        "Database.ExecuteSqlRawAsync",
        "Database.SqlQueryRaw<int>",
    ];

    public static readonly TheoryData<string> ShouldReportData =
    [
        "Users.{|#0:FromSqlRaw|}",
        "Database.{|#0:ExecuteSqlRaw|}",
        "Database.{|#0:ExecuteSqlRawAsync|}",
        "Database.{|#0:SqlQueryRaw|}<int>",
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
    [MemberData(nameof(DoNotReportData))]
    public Task Constant_string_do_not_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.{{call}}("FooBar WHERE Id = 1");
    }
}
""");

    [Theory]
    [MemberData(nameof(DoNotReportData))]
    public Task Constant_strings_do_not_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.{{call}}("FooBar"
            + " WHERE Id = 1");
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
    void M(MyDbContext db, string id)
    {
        db.{{call}}("FooBar WHERE Id = {0}", id);
    }
}
""");

    [Theory]
    [MemberData(nameof(ShouldReportData))]
    public Task Argument_should_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, string id)
    {
        db.{{call}}("FooBar WHERE Id = " + id);
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.StringConcatenationUsageInRawQueries).WithLocation(0));

    [Theory]
    [MemberData(nameof(ShouldReportData))]
    public Task Method_call_should_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.{{call}}("FooBar WHERE Id = " + GetId());
    }

    string GetId() => "1";
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.StringConcatenationUsageInRawQueries).WithLocation(0));

    [Theory]
    [MemberData(nameof(DoNotReportData))]
    public Task Constant_string_member_do_not_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    const string Id = "1";

    void M(MyDbContext db)
    {
        db.{{call}}("FooBar WHERE Id = " + Id);
    }
}
""");

    [Theory]
    [MemberData(nameof(ShouldReportData))]
    public Task Readonly_static_string_variable_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    readonly static string Id = "1";

    void M(MyDbContext db)
    {
        db.{{call}}("FooBar WHERE Id = " + Id);
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.StringConcatenationUsageInRawQueries).WithLocation(0));

    [Theory]
    [MemberData(nameof(DoNotReportData))]
    public Task Constant_string_format_do_not_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.{{call}}(string.Format("FooBar WHERE Id = {0}", "1"));
    }
}
""");

    [Theory]
    [MemberData(nameof(DoNotReportData))]
    public Task Constant_string_format_with_format_provider_do_not_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.{{call}}(string.Format(System.Globalization.CultureInfo.InvariantCulture, "FooBar WHERE Id = {0}", "1"));
    }
}
""");

    [Theory]
    [MemberData(nameof(ShouldReportData))]
    public Task String_format_with_format_provider_and_argument_should_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, string id)
    {
        db.{{call}}(string.Format(System.Globalization.CultureInfo.InvariantCulture, "FooBar WHERE Id = {0}", id));
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.StringConcatenationUsageInRawQueries).WithLocation(0));

    [Theory]
    [MemberData(nameof(ShouldReportData))]
    public Task String_format_with_argument_should_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, string id)
    {
        db.{{call}}(string.Format("FooBar WHERE Id = {0}", id));
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.StringConcatenationUsageInRawQueries).WithLocation(0));

    [Theory]
    [MemberData(nameof(ShouldReportData))]
    public Task String_format_with_method_call_should_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.{{call}}(string.Format("FooBar WHERE Id = {0}", GetId()));
    }

    string GetId() => "1";
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.StringConcatenationUsageInRawQueries).WithLocation(0));

    [Theory]
    [MemberData(nameof(DoNotReportData))]
    public Task Constant_string_concat_do_not_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.{{call}}(string.Concat("FooBar", " WHERE Id = ", "1"));
    }
}
""");

    [Theory]
    [MemberData(nameof(ShouldReportData))]
    public Task String_concat_with_argument_should_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, string id)
    {
        db.{{call}}(string.Concat("FooBar WHERE Id = ", id));
    }
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.StringConcatenationUsageInRawQueries).WithLocation(0));

    [Theory]
    [MemberData(nameof(ShouldReportData))]
    public Task String_concat_with_method_call_should_report(string call)
        => Verify.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.{{call}}(string.Concat("FooBar WHERE Id = ", GetId()));
    }

    string GetId() => "1";
}
""",
            DiagnosticResult.CompilerWarning(EFDiagnostics.StringConcatenationUsageInRawQueries).WithLocation(0));
}
