// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

using VerifyCS =
    CSharpCodeFixVerifier<InterpolatedStringUsageInRawQueriesDiagnosticAnalyzer, InterpolatedStringUsageInRawQueriesCodeFixProvider>;

public class InterpolatedStringUsageInRawQueriesTests
{
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

    [Fact]
    public Task FromSql_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.FromSql($"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task FromSqlInterpolated_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.FromSqlInterpolated($"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSql_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSql($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlInterpolated_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlInterpolated($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlAsync_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlAsync($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlInterpolatedAsync_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task SqlQuery_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.SqlQuery<int>($"SELECT [Age] FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    #region FromSqlRaw

    [Fact]
    public Task FromSqlRaw_constant_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        db.Users.FromSqlRaw("SELECT * FROM [Users] WHERE [Id] = 1;");
    }
}
""");

    [Fact]
    public Task FromSqlRaw_constant_string_with_parameters_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.FromSqlRaw("SELECT * FROM [Users] WHERE [Id] = {0};", id);
    }
}
""");

    [Fact]
    public Task FromSqlRaw_interpolated_string_report()
        => VerifyCS.VerifyCodeFixAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.[|FromSqlRaw|]($"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.FromSql($"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public async Task FromSqlRaw_interpolated_string_with_other_parameters_report()
    {
        var source = MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.[|FromSqlRaw|]($"SELECT * FROM [Users] WHERE [Id] = {id};", id);
    }
}
""";

        await VerifyCS.VerifyCodeFixAsync(source, source);
    }

    [Fact]
    public Task FromSqlRaw_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        const string id = "1";
        db.Users.FromSqlRaw($"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task FromSqlRaw_pseudo_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        db.Users.FromSqlRaw($"SELECT [{nameof(MyDbContext)}] FROM [Users] WHERE [Id] = {1};");
    }
}
""");

    [Fact]
    public Task FromSqlRaw_direct_extension_class_usage_interpolated_string_report()
        => VerifyCS.VerifyCodeFixAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalQueryableExtensions.[|FromSqlRaw|](db.Users, $"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalQueryableExtensions.FromSql(db.Users, $"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task FromSqlRaw_direct_extension_class_usage_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        const string id = "1";
        RelationalQueryableExtensions.FromSqlRaw(db.Users, $"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task FromSqlRaw_direct_extension_class_usage_pseudo_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        RelationalQueryableExtensions.FromSqlRaw(db.Users, $"SELECT [{nameof(MyDbContext)}] FROM [Users] WHERE [Id] = {1};");
    }
}
""");

    [Fact]
    public Task FromSqlRaw_FixAll()
        => VerifyCS.VerifyCodeFixAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.[|FromSqlRaw|]($"SELECT * FROM [Users] WHERE [Id] = {id};");
        db.Users.[|FromSqlRaw|]($"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.FromSql($"SELECT * FROM [Users] WHERE [Id] = {id};");
        db.Users.FromSql($"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    #endregion

    #region ExecuteSqlRaw

    [Fact]
    public Task ExecuteSqlRaw_constant_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM [Users] WHERE [Id] = 1;");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_constant_string_with_parameters_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM FROM [Users] WHERE [Id] = {0};", id);
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_interpolated_string_report()
        => VerifyCS.VerifyCodeFixAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRaw|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSql($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public async Task ExecuteSqlRaw_interpolated_string_with_other_parameters_report()
    {
        var source = MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRaw|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};", id);
    }
}
""";

        await VerifyCS.VerifyCodeFixAsync(source, source);
    }

    [Fact]
    public async Task ExecuteSqlRaw_interpolated_string_with_other_parameters_IEnumerable_report()
    {
        var source = MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRaw|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};", (IEnumerable<object>)null);
    }
}
""";

        await VerifyCS.VerifyCodeFixAsync(source, source);
    }

    [Fact]
    public Task ExecuteSqlRaw_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        const string id = "1";
        db.Database.ExecuteSqlRaw($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_pseudo_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRaw($"DELETE FROM FROM [Users] WHERE [{nameof(MyDbContext)}] = {1};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_direct_extension_class_usage_interpolated_string_report()
        => VerifyCS.VerifyCodeFixAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalDatabaseFacadeExtensions.[|ExecuteSqlRaw|](db.Database, $"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalDatabaseFacadeExtensions.ExecuteSql(db.Database, $"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_direct_extension_class_usage_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        const string id = "1";
        RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(db.Database, $"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_direct_extension_class_usage_pseudo_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(db.Database, $"DELETE FROM FROM [{nameof(MyDbContext)}] WHERE [Id] = {1};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_FixAll()
        => VerifyCS.VerifyCodeFixAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRaw|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
        db.Database.[|ExecuteSqlRaw|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSql($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
        db.Database.ExecuteSql($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    #endregion

    #region ExecuteSqlRawAsync

    [Fact]
    public Task ExecuteSqlRawAsync_constant_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        db.Database.ExecuteSqlRawAsync("DELETE FROM [Users] WHERE [Id] = 1;");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_constant_string_with_parameters_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM FROM [Users] WHERE [Id] = {0};", id);
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_interpolated_string_report()
        => VerifyCS.VerifyCodeFixAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRawAsync|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlAsync($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public async Task ExecuteSqlRawAsync_interpolated_string_with_other_parameters_report()
    {
        var source = MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRawAsync|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};", id);
    }
}
""";

        await VerifyCS.VerifyCodeFixAsync(source, source);
    }

    [Fact]
    public async Task ExecuteSqlRawAsync_interpolated_string_with_other_parameters_IEnumerable_report()
    {
        var source = MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRawAsync|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};", (IEnumerable<object>)null);
    }
}
""";

        await VerifyCS.VerifyCodeFixAsync(source, source);
    }

    [Fact]
    public Task ExecuteSqlRawAsync_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        const string id = "1";
        db.Database.ExecuteSqlRawAsync($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_pseudo_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        const string id = "1";
        db.Database.ExecuteSqlRawAsync($"DELETE FROM FROM [{nameof(MyDbContext)}] WHERE [Id] = {1};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_direct_extension_class_usage_interpolated_string_report()
        => VerifyCS.VerifyCodeFixAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalDatabaseFacadeExtensions.[|ExecuteSqlRawAsync|](db.Database, $"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalDatabaseFacadeExtensions.ExecuteSqlAsync(db.Database, $"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_direct_extension_class_usage_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        const string id = "1";
        RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(db.Database, $"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_direct_extension_class_usage_pseudo_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(db.Database, $"DELETE FROM FROM [{nameof(MyDbContext)}] WHERE [Id] = {1};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_FixAll()
        => VerifyCS.VerifyCodeFixAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRawAsync|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
        db.Database.[|ExecuteSqlRawAsync|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlAsync($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
        db.Database.ExecuteSqlAsync($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    #endregion

    #region SqlQueryRaw

    [Fact]
    public Task SqlQueryRaw_constant_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        db.Database.SqlQueryRaw<int>("SELECT [Age] FROM [Users] WHERE [Id] = 1;");
    }
}
""");

    [Fact]
    public Task SqlQueryRaw_constant_string_with_parameters_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.SqlQueryRaw<int>("SELECT [Age] FROM [Users] WHERE [Id] = {0};", id);
    }
}
""");

    [Fact]
    public Task SqlQueryRaw_interpolated_string_report()
        => VerifyCS.VerifyCodeFixAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|SqlQueryRaw|]<int>($"SELECT [Age] FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.SqlQuery<int>($"SELECT [Age] FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public async Task SqlQueryRaw_interpolated_string_with_other_parameters_report()
    {
        var source = MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|SqlQueryRaw|]<int>($"SELECT [Age] FROM [Users] WHERE [Id] = {id};", id);
    }
}
""";

        await VerifyCS.VerifyCodeFixAsync(source, source);
    }

    [Fact]
    public Task SqlQueryRaw_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        const string id = "1";
        db.Database.SqlQueryRaw<int>($"SELECT [Age] FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task SqlQueryRaw_pseudo_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        db.Database.SqlQueryRaw<int>($"SELECT [{nameof(MyDbContext)}] FROM [Users] WHERE [Id] = {1};");
    }
}
""");

    [Fact]
    public Task SqlQueryRaw_direct_extension_class_usage_interpolated_string_report()
        => VerifyCS.VerifyCodeFixAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalDatabaseFacadeExtensions.[|SqlQueryRaw|]<int>(db.Database, $"SELECT [Age] FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalDatabaseFacadeExtensions.SqlQuery<int>(db.Database, $"SELECT [Age] FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task SqlQueryRaw_direct_extension_class_usage_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        const string id = "1";
        RelationalDatabaseFacadeExtensions.SqlQueryRaw<int>(db.Database, $"SELECT [Age] FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task SqlQueryRaw_direct_extension_class_usage_pseudo_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db)
    {
        RelationalDatabaseFacadeExtensions.SqlQueryRaw<int>(db.Database, $"SELECT [{nameof(MyDbContext)}] FROM [Users] WHERE [Id] = {1};");
    }
}
""");

    [Fact]
    public Task SqlQueryRaw_FixAll()
        => VerifyCS.VerifyCodeFixAsync(
            MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|SqlQueryRaw|]<int>($"SELECT [Age] FROM [Users] WHERE [Id] = {id};");
        db.Database.[|SqlQueryRaw|]<int>($"SELECT [Age] FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext
            + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.SqlQuery<int>($"SELECT [Age] FROM [Users] WHERE [Id] = {id};");
        db.Database.SqlQuery<int>($"SELECT [Age] FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    #endregion
}
