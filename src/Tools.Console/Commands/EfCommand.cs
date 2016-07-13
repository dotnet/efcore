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

        public static void WriteLogo()
        {
            const string Bold = "\x1b[1m";
            const string Normal = "\x1b[22m";
            const string Magenta = "\x1b[35m";
            const string White = "\x1b[37m";
            const string Default = "\x1b[39m";

            Console.WriteLine();
            Console.WriteLine(@"                     _/\__       ".Insert(21, Bold + White));
            Console.WriteLine(@"               ---==/    \\      ".Insert(20, Bold + White));
            Console.WriteLine(@"         ___  ___   |.    \|\    ".Insert(26, Bold).Insert(21, Normal).Insert(20, Bold + White).Insert(9, Normal + Magenta));
            Console.WriteLine(@"        | __|| __|  |  )   \\\   ".Insert(20, Bold + White).Insert(8, Normal + Magenta));
            Console.WriteLine(@"        | _| | _|   \_/ |  //|\\ ".Insert(20, Bold + White).Insert(8, Normal + Magenta));
            Console.WriteLine(@"        |___||_|       /   \\\/\\".Insert(33, Normal + Default).Insert(23, Bold + White).Insert(8, Normal + Magenta));
            Console.WriteLine();
        }
    }
}