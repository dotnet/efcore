// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis.Testing;

namespace Microsoft.EntityFrameworkCore;

using VerifyCS = CSharpAnalyzerVerifier<UninitializedDbSetDiagnosticSuppressor>;

public class UninitializedDbSetDiagnosticSuppressorTests
{
    [ConditionalFact]
    public Task DbSet_property_on_DbContext_is_suppressed()
        => VerifySingleSuppressionAsync(
            """
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public Microsoft.EntityFrameworkCore.DbSet<Blog> {|#0:Blogs|} { get; set; }
}

public class Blog
{
    public int Id { get; set; }
}
""");

    [ConditionalFact]
    public Task Non_public_DbSet_property_on_DbContext_is_suppressed()
        => VerifySingleSuppressionAsync(
            """
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    private Microsoft.EntityFrameworkCore.DbSet<Blog> {|#0:Blogs|} { get; set; }
}

public class Blog
{
    public int Id { get; set; }
}
""");

    [ConditionalFact]
    public Task DbSet_property_with_non_public_setter_on_DbContext_is_suppressed()
        => VerifySingleSuppressionAsync(
            """
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public Microsoft.EntityFrameworkCore.DbSet<Blog> {|#0:Blogs|} { get; private set; }
}

public class Blog
{
    public int Id { get; set; }
}
""");

    [ConditionalFact]
    public Task DbSet_property_without_setter_on_DbContext_is_not_suppressed()
        => VerifySingleSuppressionAsync(
            """
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public Microsoft.EntityFrameworkCore.DbSet<Blog> {|#0:Blogs|} { get; }
}

public class Blog
{
    public int Id { get; set; }
}
""", isSuppressed: false);

    [ConditionalFact]
    public Task Static_DbSet_property_on_DbContext_is_not_suppressed()
        => VerifySingleSuppressionAsync(
            """
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public static Microsoft.EntityFrameworkCore.DbSet<Blog> {|#0:Blogs|} { get; set; }
}

public class Blog
{
    public int Id { get; set; }
}
""", isSuppressed: false);

    [ConditionalFact]
    public Task Non_DbSet_property_on_DbContext_is_not_suppressed()
        => VerifySingleSuppressionAsync(
            """
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public string {|#0:Name|} { get; set; }
}
""", isSuppressed: false);

    [ConditionalFact]
    public Task DbSet_property_on_non_DbContext_is_not_suppressed()
        => VerifySingleSuppressionAsync(
            """
public class Foo
{
    public Microsoft.EntityFrameworkCore.DbSet<Blog> {|#0:Blogs|} { get; set; }
}

public class Blog
{
    public int Id { get; set; }
}
""", isSuppressed: false);

    [ConditionalFact]
    public async Task DbSet_property_on_DbContext_with_ctor_is_suppressed()
    {
        var source = """
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public {|#0:MyDbContext|}() {}

    public Microsoft.EntityFrameworkCore.DbSet<Blog> {|#1:Blogs|} { get; set; }
}

public class Blog
{
    public int Id { get; set; }
}
""";

        await new VerifyCS.Test
        {
            TestCode = source,
            CompilerDiagnostics = CompilerDiagnostics.Warnings,
            ExpectedDiagnostics =
            {
                DiagnosticResult.CompilerWarning("CS8618")
                    .WithLocation(0)
                    .WithLocation(1)
                    .WithIsSuppressed(true)
            }
        }.RunAsync();
    }

    [ConditionalFact]
    public async Task DbSet_property_on_DbContext_with_ctors_is_suppressed()
    {
        var source = """
public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public {|#0:MyDbContext|}() {}

    public {|#1:MyDbContext|}(int foo) {}

    public Microsoft.EntityFrameworkCore.DbSet<Blog> {|#2:Blogs|} { get; set; }
}

public class Blog
{
    public int Id { get; set; }
}
""";

        await new VerifyCS.Test
        {
            TestCode = source,
            CompilerDiagnostics = CompilerDiagnostics.Warnings,
            ExpectedDiagnostics =
            {
                DiagnosticResult.CompilerWarning("CS8618")
                    .WithLocation(0)
                    .WithLocation(2)
                    .WithIsSuppressed(true),
                DiagnosticResult.CompilerWarning("CS8618")
                    .WithLocation(1)
                    .WithLocation(2)
                    .WithIsSuppressed(true)
            }
        }.RunAsync();
    }

    private Task VerifySingleSuppressionAsync(string source, bool isSuppressed = true)
        => new VerifyCS.Test
        {
            TestCode = source,
            CompilerDiagnostics = CompilerDiagnostics.Warnings,
            ExpectedDiagnostics =
            {
                DiagnosticResult.CompilerWarning("CS8618")
                    .WithLocation(0)
                    .WithLocation(0)
                    .WithIsSuppressed(isSuppressed)
            }
        }.RunAsync();
}
