// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

public class ExeTest
{
    [Fact]
    public void ToArguments_works()
    {
        var result = ToArguments(
            new[]
            {
                "",
                "Good",
                "Good\\",
                "Needs quotes",
                "Needs escaping\\",
                "Needs escaping\\\\",
                "Needs \"escaping\"",
                "Needs \\\"escaping\"",
                "Needs escaping\\\\too"
            });

        Assert.Equal(
            "\"\" "
            + "Good "
            + "Good\\ "
            + "\"Needs quotes\" "
            + "\"Needs escaping\\\\\" "
            + "\"Needs escaping\\\\\\\\\" "
            + "\"Needs \\\"escaping\\\"\" "
            + "\"Needs \\\\\\\"escaping\\\"\" "
            + "\"Needs escaping\\\\\\\\too\"",
            result);
    }

    private static string ToArguments(IReadOnlyList<string> args)
        => (string)typeof(Exe).GetMethod("ToArguments", BindingFlags.Static | BindingFlags.NonPublic)!
            .Invoke(null, [args])!;
}
