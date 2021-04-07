// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.DotNet.Cli.CommandLine
{
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
}
