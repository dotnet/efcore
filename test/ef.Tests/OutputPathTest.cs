// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Tools.Commands;

namespace Microsoft.EntityFrameworkCore.Tools;

public class OutputPathTest
{
    private static readonly string Root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "efroot"));

    [Fact]
    public void Relative_output_is_resolved_from_the_working_directory()
        => Assert.Equal(
            Path.Combine(Root, "src", "Startup", "Scripts", "Idempotent.sql"),
            ProjectCommandBase.ResolveOutputPath(Path.Combine("Scripts", "Idempotent.sql"), Path.Combine(Root, "src", "Startup")));

    [Fact]
    public void Absolute_output_ignores_the_working_directory()
    {
        var output = Path.Combine(Root, "out", "script.sql");

        Assert.Equal(output, ProjectCommandBase.ResolveOutputPath(output, Path.Combine(Root, "src", "Startup")));
    }

    [Fact]
    public void Relative_output_without_working_directory_is_resolved_from_the_current_directory()
        => Assert.Equal(Path.GetFullPath("script.sql"), ProjectCommandBase.ResolveOutputPath("script.sql", workingDir: null));

    [Fact]
    public void Dot_segments_are_normalized()
        => Assert.Equal(
            Path.Combine(Root, "src", "Migrations", "script.sql"),
            ProjectCommandBase.ResolveOutputPath(Path.Combine("..", "Migrations", "script.sql"), Path.Combine(Root, "src", "Startup")));
}
