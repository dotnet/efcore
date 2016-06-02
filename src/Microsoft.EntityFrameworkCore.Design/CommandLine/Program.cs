// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class Program
    {
        public static int Main([NotNull] string[] args)
        {
            ConsoleCommandLogger.IsVerbose = HandleVerboseOption(ref args);
            HandleDebugSwitch(ref args);

            try
            {
                return ExecuteCommand.Create(ref args).Execute(args);
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
                    ConsoleCommandLogger.Error(ex.ToString());
                }

                ConsoleCommandLogger.Error(ex.Message.Bold().Red());
                return 1;
            }
        }

        private static bool HandleVerboseOption(ref string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "-v" || args[i] == "--verbose")
                {
                    args = args.Take(i).Concat(args.Skip(i + 1)).ToArray();
                    return true;
                }
            }
            return false;
        }

        [Conditional("DEBUG")]
        private static void HandleDebugSwitch(ref string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "--debug")
                {
                    args = args.Take(i).Concat(args.Skip(i + 1)).ToArray();
                    Console.WriteLine("Waiting for debugger to attach. Press ENTER to continue");
                    Console.WriteLine($"Process ID: {Process.GetCurrentProcess().Id}");
                    Console.ReadLine();
                }
            }
        }
    }
}
