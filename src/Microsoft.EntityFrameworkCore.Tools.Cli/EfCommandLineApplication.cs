// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class EfCommandLineApplication : CommandLineApplication
    {
        public EfCommandLineApplication(bool throwOnUnexpectedArg = true)
            : base(throwOnUnexpectedArg)
        {
            Name = "dotnet ef";
            FullName = "Entity Framework .NET Core CLI Commands";
        }

        public virtual void WriteLogo()
        {
            const string Bold = "\x1b[1m";
            const string Normal = "\x1b[22m";
            const string Magenta = "\x1b[35m";
            const string White = "\x1b[37m";
            const string Default = "\x1b[39m";

            Reporter.Output.WriteLine();
            Reporter.Output.WriteLine(@"                     _/\__       ".Insert(21, Bold + White));
            Reporter.Output.WriteLine(@"               ---==/    \\      ");
            Reporter.Output.WriteLine(@"         ___  ___   |.    \|\    ".Insert(26, Bold).Insert(21, Normal).Insert(20, Bold + White).Insert(9, Normal + Magenta));
            Reporter.Output.WriteLine(@"        | __|| __|  |  )   \\\   ".Insert(20, Bold + White).Insert(8, Normal + Magenta));
            Reporter.Output.WriteLine(@"        | _| | _|   \_/ |  //|\\ ".Insert(20, Bold + White).Insert(8, Normal + Magenta));
            Reporter.Output.WriteLine(@"        |___||_|       /   \\\/\\".Insert(33, Normal + Default).Insert(23, Bold + White).Insert(8, Normal + Magenta));
            Reporter.Output.WriteLine();
        }

        public virtual string GetVersion()
            => typeof(EfCommandLineApplication).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
    }
}
