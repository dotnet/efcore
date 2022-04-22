// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore;

public class UninitializedDbSetDiagnosticSuppressorTest : DiagnosticAnalyzerTestBase
{
    [ConditionalFact]
    public async Task DbSet_property_on_DbContext_is_suppressed()
    {
        var source = @"
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public Microsoft.EntityFrameworkCore.DbSet<Blog> Blogs { get; set; }
}

public class Blog
{
    public int Id { get; set; }
}";

        var diagnostic = Assert.Single(await GetDiagnosticsFullSourceAsync(source));

        Assert.Equal("CS8618", diagnostic.Id);
        Assert.True(diagnostic.IsSuppressed);
    }

    [ConditionalFact]
    public async Task Non_public_DbSet_property_on_DbContext_is_suppressed()
    {
        var source = @"
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    private Microsoft.EntityFrameworkCore.DbSet<Blog> Blogs { get; set; }
}

public class Blog
{
    public int Id { get; set; }
}";

        var diagnostic = Assert.Single(await GetDiagnosticsFullSourceAsync(source));

        Assert.Equal("CS8618", diagnostic.Id);
        Assert.True(diagnostic.IsSuppressed);
    }

    [ConditionalFact]
    public async Task DbSet_property_with_non_public_setter_on_DbContext_is_suppressed()
    {
        var source = @"
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public Microsoft.EntityFrameworkCore.DbSet<Blog> Blogs { get; private set; }
}

public class Blog
{
    public int Id { get; set; }
}";

        var diagnostic = Assert.Single(await GetDiagnosticsFullSourceAsync(source));

        Assert.Equal("CS8618", diagnostic.Id);
        Assert.True(diagnostic.IsSuppressed);
    }

    [ConditionalFact]
    public async Task DbSet_property_without_setter_on_DbContext_is_not_suppressed()
    {
        var source = @"
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public Microsoft.EntityFrameworkCore.DbSet<Blog> Blogs { get; }
}

public class Blog
{
    public int Id { get; set; }
}";

        var diagnostic = Assert.Single(await GetDiagnosticsFullSourceAsync(source));

        Assert.Equal("CS8618", diagnostic.Id);
        Assert.False(diagnostic.IsSuppressed);
    }

    [ConditionalFact]
    public async Task Static_DbSet_property_on_DbContext_is_not_suppressed()
    {
        var source = @"
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public static Microsoft.EntityFrameworkCore.DbSet<Blog> Blogs { get; set; }
}

public class Blog
{
    public int Id { get; set; }
}";

        var diagnostic = Assert.Single(await GetDiagnosticsFullSourceAsync(source));

        Assert.Equal("CS8618", diagnostic.Id);
        Assert.False(diagnostic.IsSuppressed);
    }

    [ConditionalFact]
    public async Task Non_DbSet_property_on_DbContext_is_not_suppressed()
    {
        var source = @"
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public string Name { get; set; }
}";

        var diagnostic = Assert.Single(await GetDiagnosticsFullSourceAsync(source));

        Assert.Equal("CS8618", diagnostic.Id);
        Assert.False(diagnostic.IsSuppressed);
    }

    [ConditionalFact]
    public async Task DbSet_property_on_non_DbContext_is_not_suppressed()
    {
        var source = @"
public class Foo
{
    public Microsoft.EntityFrameworkCore.DbSet<Blog> Blogs { get; set; }
}

public class Blog
{
    public int Id { get; set; }
}";

        var diagnostic = Assert.Single(await GetDiagnosticsFullSourceAsync(source));

        Assert.Equal("CS8618", diagnostic.Id);
        Assert.False(diagnostic.IsSuppressed);
    }

    [ConditionalFact]
    public async Task DbSet_property_on_DbContext_with_ctor_is_suppressed()
    {
        var source = @"
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public MyDbContext() {}

    public Microsoft.EntityFrameworkCore.DbSet<Blog> Blogs { get; set; }
}

public class Blog
{
    public int Id { get; set; }
}";

        var diagnostic = Assert.Single(await GetDiagnosticsFullSourceAsync(source));

        Assert.Equal("CS8618", diagnostic.Id);
        Assert.True(diagnostic.IsSuppressed);
    }

    [ConditionalFact]
    public async Task DbSet_property_on_DbContext_with_ctors_is_suppressed()
    {
        var source = @"
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public MyDbContext() {}

    public MyDbContext(int foo) {}

    public Microsoft.EntityFrameworkCore.DbSet<Blog> Blogs { get; set; }
}

public class Blog
{
    public int Id { get; set; }
}";

        var diagnostics = await GetDiagnosticsFullSourceAsync(source);

        Assert.All(
            diagnostics,
            diagnostic =>
            {
                Assert.Equal("CS8618", diagnostic.Id);
                Assert.True(diagnostic.IsSuppressed);
            });
    }

    protected Task<Diagnostic[]> GetDiagnosticsFullSourceAsync(string source)
        => base.GetDiagnosticsFullSourceAsync(source, analyzerDiagnosticsOnly: false);

    protected override DiagnosticAnalyzer CreateDiagnosticAnalyzer()
        => new UninitializedDbSetDiagnosticSuppressor();
}
