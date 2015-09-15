// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451 || DNXCORE50

using System;

namespace Microsoft.Dnx.Runtime.Common.CommandLine
{
    internal static class CommandLineUtilsExtensions
    {
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
    }
}

#endif
