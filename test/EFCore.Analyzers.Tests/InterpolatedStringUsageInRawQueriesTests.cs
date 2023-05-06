// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

using VerifyCS = CSharpAnalyzerVerifier<InterpolatedStringUsageInRawQueriesDiagnosticAnalyzer>;

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
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.[|FromSqlRaw|]($"SELECT * FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task FromSqlRaw_interpolated_string_with_other_parameters_report()
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Users.[|FromSqlRaw|]($"SELECT * FROM [Users] WHERE [Id] = {id};", id);
    }
}
""");

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
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalQueryableExtensions.[|FromSqlRaw|](db.Users, $"SELECT * FROM [Users] WHERE [Id] = {id};");
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
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRaw|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_interpolated_string_with_other_parameters_report()
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRaw|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};", id);
    }
}
""");

    [Fact]
    public Task ExecuteSqlRaw_interpolated_string_with_other_parameters_IEnumerable_report()
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRaw|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};", (IEnumerable<object>)null);
    }
}
""");

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
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalDatabaseFacadeExtensions.[|ExecuteSqlRaw|](db.Database, $"DELETE FROM FROM [Users] WHERE [Id] = {id};");
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
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRawAsync|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};");
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_interpolated_string_with_other_parameters_report()
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRawAsync|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};", id);
    }
}
""");

    [Fact]
    public Task ExecuteSqlRawAsync_interpolated_string_with_other_parameters_IEnumerable_report()
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        db.Database.[|ExecuteSqlRawAsync|]($"DELETE FROM FROM [Users] WHERE [Id] = {id};", (IEnumerable<object>)null);
    }
}
""");

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
        => VerifyCS.VerifyAnalyzerAsync(MyDbContext + """
class C
{
    void M(MyDbContext db, int id)
    {
        RelationalDatabaseFacadeExtensions.[|ExecuteSqlRawAsync|](db.Database, $"DELETE FROM FROM [Users] WHERE [Id] = {id};");
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

    #endregion
}
