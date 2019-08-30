// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    internal class ContextCommandBase : ProjectCommandBase
    {
        protected CommandOption Context { get; private set; }

        public override void Configure(CommandLineApplication command)
        {
            Context = command.Option("-c|--context <DBCONTEXT>", Resources.ContextDescription);

            base.Configure(command);
        }
    }
}
