// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.CommandLineUtils
{
    public static class CommandLineApplicationExtensions
    {
        public static void OnExecute(this CommandLineApplication command, Action invoke)
            => command.OnExecute(
                () =>
                {
                    invoke();

                    return 0;
                });

        public static CommandOption Option(this CommandLineApplication command, string template, string description, bool inherited = false)
            => command.Option(
                template,
                description,
                template.IndexOf('<') != -1
                    ? CommandOptionType.SingleValue
                    : CommandOptionType.NoValue,
                inherited: inherited);

        public static CommandOption HelpOption(this CommandLineApplication command)
            => command.HelpOption("-h|--help");

        public static CommandOption VerboseOption(this CommandLineApplication command)
            => command.Option("-v|--verbose", "Enable verbose output");

        public static CommandOption JsonOption(this CommandLineApplication command)
            => command.Option("--json", "Use json output. JSON is wrapped by '//BEGIN' and '//END'");

        public static CommandOption VersionOption(
            this CommandLineApplication command,
            Func<string> shortFormVersionGetter)
            => command.VersionOption("--version", shortFormVersionGetter);
    }
}
