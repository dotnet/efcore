// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

public class ExeTest
{
    [Fact]
    public void ToArguments_works()
    {
        var result = ToArguments(
        [
            "",
            "Good",
            "Good\\",
            "Needs quotes",
            "Needs escaping\\",
            "Needs escaping\\\\",
            "Needs \"escaping\"",
            "Needs \\\"escaping\"",
            "Needs escaping\\\\too"
        ]);

        Assert.Equal(
            "\"\" "
            + "Good "
            + "Good\\ "
            + "\"Needs quotes\" "
            + "\"Needs escaping\\\\\" "
            + "\"Needs escaping\\\\\\\\\" "
            + "\"Needs \\\"escaping\\\"\" "
            + "\"Needs \\\\\\\"escaping\\\"\" "
            + "\"Needs escaping\\\\too\"",
            result);
    }

    [Fact]
    public void ToArguments_does_not_escape_UNC_prefix()
        => Assert.Equal(
            "\"\\\\FILESERVER\\DevShare\\Sample Solution\\Api Project.csproj\"",
            ToArguments(["\\\\FILESERVER\\DevShare\\Sample Solution\\Api Project.csproj"]));

    private static string ToArguments(IReadOnlyList<string> args)
        => (string)typeof(Exe).GetMethod("ToArguments", BindingFlags.Static | BindingFlags.Public)!
            .Invoke(null, [args])!;
}
