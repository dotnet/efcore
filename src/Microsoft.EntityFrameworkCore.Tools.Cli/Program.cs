// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.EntityFrameworkCore.Design;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class Program
    {
        public static int Main([NotNull] string[] args)
        {
            HandleVerboseOption(ref args);
            DebugHelper.HandleDebugSwitch(ref args);

            try
            {
                return ExecuteCommand.Create().Execute(args);
            }
            catch (Exception ex)
            {
                // TODO ensure always a json response if --json is supplied
                if (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }

                if (!(ex is OperationException))
                {
                    Reporter.Error.WriteLine(ex.ToString());
                }

                Reporter.Error.WriteLine(ex.Message.Bold().Red());
                return 1;
            }
        }

        private static void HandleVerboseOption(ref string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "-v" || args[i] == "--verbose")
                {
                    Environment.SetEnvironmentVariable(CommandContext.Variables.Verbose, bool.TrueString);
                    args = args.Take(i).Concat(args.Skip(i + 1)).ToArray();

                    return;
                }
            }
        }
    }
}
