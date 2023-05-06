// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

using VerifyCS = CSharpCodeFixVerifier<InterpolatedStringUsageInRawQueriesDiagnosticAnalyzer, InterpolatedStringUsageInRawQueriesCodeFixProvider>;

public class InterpolatedStringUsageInRawQueriesTests
{
    private const string MyDbContext = """
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

    #region FromSqlRaw

    [Fact]
    public Task FromSqlRaw_constant_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
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
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
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
        => VerifyCS.VerifyCodeFixAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.[|FromSqlRaw|]($"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.FromSqlInterpolated($"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public async Task FromSqlRaw_interpolated_string_with_other_parameters_report()
    {
        var source = MyDbContext + """
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
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
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
    public Task FromSqlRaw_direct_extension_class_usage_interpolated_string_report()
        => VerifyCS.VerifyCodeFixAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalQueryableExtensions.[|FromSqlRaw|](db.Users, $"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalQueryableExtensions.FromSqlInterpolated(db.Users, $"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task FromSqlRaw_direct_extension_class_usage_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
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
    public Task FromSqlRaw_FixAll()
        => VerifyCS.VerifyCodeFixAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.[|FromSqlRaw|]($"SELECT * FROM [Users] WHERE [Id] = {id};");
        db.Users.[|FromSqlRaw|]($"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.FromSqlInterpolated($"SELECT * FROM [Users] WHERE [Id] = {id};");
        db.Users.FromSqlInterpolated($"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    #endregion

    #region ExecuteSqlRaw

    [Fact]
    public Task ExecuteSqlRaw_constant_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
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
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
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
        => VerifyCS.VerifyCodeFixAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRaw|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlInterpolated($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public async Task ExecuteSqlRaw_interpolated_string_with_other_parameters_report()
    {
        var source = MyDbContext + """
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
        var source = MyDbContext + """
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
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
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
    public Task ExecuteSqlRaw_direct_extension_class_usage_interpolated_string_report()
        => VerifyCS.VerifyCodeFixAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalDatabaseFacadeExtensions.[|ExecuteSqlRaw|](db.Database, $"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalDatabaseFacadeExtensions.ExecuteSqlInterpolated(db.Database, $"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_direct_extension_class_usage_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
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
    public Task ExecuteSqlRaw_FixAll()
        => VerifyCS.VerifyCodeFixAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRaw|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
        db.Database.[|ExecuteSqlRaw|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlInterpolated($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
        db.Database.ExecuteSqlInterpolated($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    #endregion

    #region ExecuteSqlRawAsync

    [Fact]
    public Task ExecuteSqlRawAsync_constant_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
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
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
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
        => VerifyCS.VerifyCodeFixAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRawAsync|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public async Task ExecuteSqlRawAsync_interpolated_string_with_other_parameters_report()
    {
        var source = MyDbContext + """
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
        var source = MyDbContext + """
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
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
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
    public Task ExecuteSqlRawAsync_direct_extension_class_usage_interpolated_string_report()
        => VerifyCS.VerifyCodeFixAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalDatabaseFacadeExtensions.[|ExecuteSqlRawAsync|](db.Database, $"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalDatabaseFacadeExtensions.ExecuteSqlInterpolatedAsync(db.Database, $"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_direct_extension_class_usage_constant_interpolated_string_do_not_report()
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
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
    public Task ExecuteSqlRawAsync_FixAll()
        => VerifyCS.VerifyCodeFixAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRawAsync|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
        db.Database.[|ExecuteSqlRawAsync|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""", MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
        db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    #endregion
}
