// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.Extensions.CommandLineUtils
{
    internal static class CommandLineApplicationExtensions
    {
        public static void Confirm(this CommandLineApplication command, string message, Func<int> confirmCallback, Func<int> cancelCallback, Func<bool> forceCheck)
        {
            command.OnExecute(
                () =>
                {
                    var readedKey = 'N';
                    if (!forceCheck())
                    {
                        Reporter.Output.WriteLine(message);
                        readedKey = Console.ReadKey().KeyChar;
                    }

                    if (forceCheck() || (readedKey == 'y') || (readedKey == 'Y'))
                    {
                        return confirmCallback();
                    }

                    return cancelCallback();
                });
        }

        public static void OnExecute(this CommandLineApplication command, Action invoke)
            => command.OnExecute(
                () =>
                {
                    invoke();

                    return 0;
                });

        public static CommandOption Option(this CommandLineApplication command, string template, string description)
            => command.Option(
                template,
                description,
                template.IndexOf('<') != -1
                    ? CommandOptionType.SingleValue
                    : CommandOptionType.NoValue);

        public static CommandOption HelpOption(this CommandLineApplication command)
            => command.HelpOption("-h|--help");

        public static CommandOption VerboseOption(this CommandLineApplication command)
            => command.Option("-v|--verbose", "Enable verbose output");

        public static CommandOption VersionOption(
            this CommandLineApplication command,
            Func<string> shortFormVersionGetter)
            => command.VersionOption("--version", shortFormVersionGetter);
    }
}
