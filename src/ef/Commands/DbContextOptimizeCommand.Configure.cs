// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    internal partial class DbContextOptimizeCommand : ContextCommandBase
    {
        private CommandOption? _outputDir;
        private CommandOption? _namespace;

        public override void Configure(CommandLineApplication command)
        {
            command.Description = Resources.DbContextOptimizeDescription;

            _outputDir = command.Option("-o|--output-dir <PATH>", Resources.OutputDirDescription);
            _namespace = command.Option("-n|--namespace <NAMESPACE>", Resources.NamespaceDescription);

            base.Configure(command);
        }
    }
}
