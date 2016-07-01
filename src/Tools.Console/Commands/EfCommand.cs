// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public static class EfCommand
    {
        public static void Configure(CommandLineApplication app, CommandLineOptions options)
        {
            app.Command("database", c => DatabaseCommand.Configure(c, options));

            app.Command("dbcontext", c => DbContextCommand.Configure(c, options));

            app.Command("migrations", c => MigrationsCommand.Configure(c, options));

            app.OnExecute(() =>
                {
                    WriteLogo();
                    app.ShowHelp();
                });
        }

        private const string Bold = "\x1b[1m";
        private const string Normal = "\x1b[22m";
        private const string Magenta = "\x1b[35m";
        private const string White = "\x1b[37m";
        private const string Default = "\x1b[39m";

        public static void WriteLogo()
        {
            var lines = new[]
            {
                "",
                @"                     _/\__       ".MaybeColor(s => s.Insert(21, Bold + White)),
                @"               ---==/    \\      ".MaybeColor(s => s.Insert(20, Bold + White)),
                @"         ___  ___   |.    \|\    ".MaybeColor(s => s.Insert(26, Bold).Insert(21, Normal).Insert(20, Bold + White).Insert(9, Normal + Magenta)),
                @"        | __|| __|  |  )   \\\   ".MaybeColor(s => s.Insert(20, Bold + White).Insert(8, Normal + Magenta)),
                @"        | _| | _|   \_/ |  //|\\ ".MaybeColor(s => s.Insert(20, Bold + White).Insert(8, Normal + Magenta)),
                @"        |___||_|       /   \\\/\\".MaybeColor(s => s.Insert(33, Normal + Default).Insert(23, Bold + White).Insert(8, Normal + Magenta)),
                ""
            };

            Reporter.Output(string.Join(Environment.NewLine, lines));
        }
    }
}
