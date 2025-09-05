// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using VerifyCS =
    CSharpAnalyzerVerifier<StringConcatenationUsageInRawQueriesDiagnosticAnalyzer>;

public class StringConcatenationUsageInRawQueriesTests
{
    private const string MyDbContext =
        """
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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

    #region FromSqlRaw

    [Fact]
    public Task FromSqlRaw_constant_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.Users.FromSqlRaw("SELECT * FROM [Users] WHERE [Id] = 1");
    }
}
""");

    [Fact]
    public Task FromSqlRaw_constant_strings_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.Users.FromSqlRaw("SELECT * FROM [Users]"
            + " WHERE [Id] = 1");
    }
}
""");

    [Fact]
    public Task FromSqlRaw_constant_string_with_parameters_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.FromSqlRaw("SELECT * FROM [Users] WHERE [Id] = {0}", id);
    }
}
""");

    [Fact]
    public Task FromSqlRaw_argument_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.FromSqlRaw("SELECT * FROM [Users] WHERE [Id] = " + id);
    }
}
""", new DiagnosticResult(EFDiagnostics.StringConcatenationUsageInRawQueries, DiagnosticSeverity.Warning).WithLocation(20, 18));

    [Fact]
    public Task FromSqlRaw_method_call_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.Users.FromSqlRaw("SELECT * FROM [Users] WHERE [Id] = " + GetId());
    }

    int GetId() => 1;
}
""", new DiagnosticResult(EFDiagnostics.StringConcatenationUsageInRawQueries, DiagnosticSeverity.Warning).WithLocation(20, 18));

    [Fact]
    public Task FromSqlRaw_constant_string_member_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    const string Id = "1";

    void M(MyDbContext db)
    {
        db.Users.FromSqlRaw("SELECT * FROM [Users] WHERE [Id] = " + Id);
    }
}
""");

    [Fact]
    public Task FromSqlRaw_readonly_static_string_variable_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    readonly static string Id = "1";

    void M(MyDbContext db)
    {
        db.Users.FromSqlRaw("SELECT * FROM [Users] WHERE [Id] = " + Id);
    }
}
""", new DiagnosticResult(EFDiagnostics.StringConcatenationUsageInRawQueries, DiagnosticSeverity.Warning).WithLocation(22, 18));

    #endregion

    #region ExecuteSqlRaw

    [Fact]
    public Task ExecuteSqlRaw_constant_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM [Users] WHERE [Id] = 1");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_constant_strings_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM [Users]"
            + " WHERE [Id] = 1");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_constant_string_with_parameters_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM [Users] WHERE [Id] = {0}", id);
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_argument_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM [Users] WHERE [Id] = " + id);
    }
}
""", new DiagnosticResult(EFDiagnostics.StringConcatenationUsageInRawQueries, DiagnosticSeverity.Warning).WithLocation(20, 21));

    [Fact]
    public Task ExecuteSqlRaw_method_call_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM [Users] WHERE [Id] = " + GetId());
    }

    int GetId() => 1;
}
""", new DiagnosticResult(EFDiagnostics.StringConcatenationUsageInRawQueries, DiagnosticSeverity.Warning).WithLocation(20, 21));

    [Fact]
    public Task ExecuteSqlRaw_constant_string_member_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    const string Id = "1";

    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM [Users] WHERE [Id] = " + Id);
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_readonly_static_string_variable_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    readonly static string Id = "1";

    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM [Users] WHERE [Id] = " + Id);
    }
}
""", new DiagnosticResult(EFDiagnostics.StringConcatenationUsageInRawQueries, DiagnosticSeverity.Warning).WithLocation(22, 21));

    #endregion

    #region ExecuteSqlRawAsync

    [Fact]
    public Task ExecuteSqlRawAsync_constant_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRawAsync("DELETE FROM [Users] WHERE [Id] = 1");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_constant_strings_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRawAsync("DELETE FROM [Users]"
            + " WHERE [Id] = 1");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_constant_string_with_parameters_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlRawAsync("DELETE FROM [Users] WHERE [Id] = {0}", id);
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_argument_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlRawAsync("DELETE FROM [Users] WHERE [Id] = " + id);
    }
}
""", new DiagnosticResult(EFDiagnostics.StringConcatenationUsageInRawQueries, DiagnosticSeverity.Warning).WithLocation(20, 21));

    [Fact]
    public Task ExecuteSqlRawAsync_method_call_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRawAsync("DELETE FROM [Users] WHERE [Id] = " + GetId());
    }

    int GetId() => 1;
}
""", new DiagnosticResult(EFDiagnostics.StringConcatenationUsageInRawQueries, DiagnosticSeverity.Warning).WithLocation(20, 21));

    [Fact]
    public Task ExecuteSqlRawAsync_constant_string_member_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    const string Id = "1";

    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRawAsync("DELETE FROM [Users] WHERE [Id] = " + Id);
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_readonly_static_string_variable_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    readonly static string Id = "1";

    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRawAsync("DELETE FROM [Users] WHERE [Id] = " + Id);
    }
}
""", new DiagnosticResult(EFDiagnostics.StringConcatenationUsageInRawQueries, DiagnosticSeverity.Warning).WithLocation(22, 21));

    #endregion

    #region SqlQueryRaw

    [Fact]
    public Task SqlQueryRaw_constant_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.Database.SqlQueryRaw<int>("SELECT [Age] FROM [Users] WHERE [Id] = 1");
    }
}
""");

    [Fact]
    public Task SqlQueryRaw_constant_strings_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.Database.SqlQueryRaw<int>("SELECT [Age] FROM [Users]"
            + " WHERE [Id] = 1");
    }
}
""");

    [Fact]
    public Task SqlQueryRaw_constant_string_with_parameters_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.SqlQueryRaw<int>("SELECT [Age] FROM [Users] WHERE [Id] = {0}", id);
    }
}
""");

    [Fact]
    public Task SqlQueryRaw_argument_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.SqlQueryRaw<int>("SELECT [Age] FROM [Users] WHERE [Id] = " + id);
    }
}
""", new DiagnosticResult(EFDiagnostics.StringConcatenationUsageInRawQueries, DiagnosticSeverity.Warning).WithLocation(20, 21));

    [Fact]
    public Task SqlQueryRaw_method_call_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    void M(MyDbContext db)
    {
        db.Database.SqlQueryRaw<int>("SELECT [Age] FROM [Users] WHERE [Id] = " + GetId());
    }

    int GetId() => 1;
}
""", new DiagnosticResult(EFDiagnostics.StringConcatenationUsageInRawQueries, DiagnosticSeverity.Warning).WithLocation(20, 21));

    [Fact]
    public Task SqlQueryRaw_constant_string_member_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    const string Id = "1";

    void M(MyDbContext db)
    {
        db.Database.SqlQueryRaw<int>("SELECT [Age] FROM [Users] WHERE [Id] = " + Id);
    }
}
""");

    [Fact]
    public Task SqlQueryRaw_readonly_static_string_variable_report()
        => VerifyCS.VerifyAnalyzerAsync(
            $$"""
{{MyDbContext}}

class C
{
    readonly static string Id = "1";

    void M(MyDbContext db)
    {
        db.Database.SqlQueryRaw<int>("SELECT [Age] FROM [Users] WHERE [Id] = " + Id);
    }
}
""", new DiagnosticResult(EFDiagnostics.StringConcatenationUsageInRawQueries, DiagnosticSeverity.Warning).WithLocation(22, 21));

    #endregion
}
