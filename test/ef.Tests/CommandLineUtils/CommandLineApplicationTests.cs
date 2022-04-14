// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.DotNet.Cli.CommandLine;

public class CommandLineApplicationTests
{
    [Fact]
    public void Execute_can_parse_multiple_arguments()
    {
        var app = new CommandLineApplication();
        var one = app.Argument("<ONE>", "Argument one.");
        var two = app.Argument("<TWO>", "Argument two.");
        app.OnExecute(
            _ =>
            {
                Assert.Equal("1", one.Value);
                Assert.Equal("2", two.Value);

                return 0;
            });

        app.Execute("1", "2");
    }
}
