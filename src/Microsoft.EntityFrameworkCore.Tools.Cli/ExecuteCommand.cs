// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore.Design;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class ExecuteCommand
    {
        public static EfCommandLineApplication Configure()
        {
            var app = new EfCommandLineApplication();

            app.HelpOption();
            app.VerboseOption();
            app.VersionOption(app.GetVersion);

            app.Command("database", DatabaseCommand.Configure);
            app.Command("dbcontext", DbContextCommand.Configure);
            app.Command("migrations", MigrationsCommand.Configure);

            app.OnExecute(
                () =>
                {
                    app.WriteLogo();
                    app.ShowHelp();
                });

            return app;
        }
    }
}
