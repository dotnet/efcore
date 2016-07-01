// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
#if NET451
using System.Runtime.Serialization;
#endif
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore.Tools.Internal;

// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class Program
    {
        public static int Main(string[] args)
        {
            HandleDebugSwitch(ref args);

            try
            {
                var options = CommandLineOptions.Parse(args);
                if (options == null)
                {
                    Reporter.Output("Specify --help for a list of available options and commands.");
                    return 1;
                }

                Reporter.IsVerbose = options.Verbose;

                if (options.IsHelp)
                {
                    return 2;
                }

                if (!string.IsNullOrEmpty(options.DispatcherVersion)
                    && !string.Equals(options.DispatcherVersion, GetVersion(), StringComparison.Ordinal))
                {
                    Reporter.Output($"Expected dispatch version {GetVersion()} but received {options.DispatcherVersion}");
                    Reporter.Error("Could not invoke command.");
                    return 3;
                }

                if (string.IsNullOrEmpty(options.Assembly))
                {
                    Reporter.Error("Missing required option --assembly");
                    Reporter.Output("Specify --help for a list of available options and commands.");
                    return 1;
                }

                if (!File.Exists(options.Assembly))
                {
                    Reporter.Error($"Could not find assembly '{options.Assembly}'.");
                    return 1;
                }

                if (options.Command == null)
                {
                    Reporter.Error("Error in parsing command line arguments");
                    return 1;
                }

                using (var executor = GetExecutor(options))
                {
                    var currentDirectory = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(executor.AppBasePath);
                    try
                    {
                        options.Command.Run(executor);
                    }
                    finally
                    {
                        Directory.SetCurrentDirectory(currentDirectory);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }

                if (!(ex is OperationErrorException) && !(ex is CommandParsingException))
                {
                    Reporter.Error(ex.ToString());
                }

                Reporter.Error(ex.Message);
                return 1;
            }
        }

        private static OperationExecutorBase GetExecutor(CommandLineOptions options)
        {
#if NET451
            try
            {
                return new AppDomainOperationExecutor(
                        configFile: options.AppConfigFile,
                        assembly: options.Assembly,
                        startupAssembly: options.StartupAssembly,
                        projectDir: options.ProjectDirectory ?? Directory.GetCurrentDirectory(),
                        dataDirectory: options.DataDirectory ?? Directory.GetCurrentDirectory(),
                        contentRootPath: options.ContentRootPath,
                        rootNamespace: options.RootNamespace,
                        environment: options.EnvironmentName);
            }
            catch (SerializationException ex)
            {
                Reporter.Verbose("Creating app-domain executor failed. Reverting to reflection operation executor.");
                Reporter.Verbose(ex.Message);
            }
#endif
            return new ReflectionOperationExecutor(
                   assembly: options.Assembly,
                   startupAssembly: options.StartupAssembly,
                   projectDir: options.ProjectDirectory ?? Directory.GetCurrentDirectory(),
                   dataDirectory: options.DataDirectory ?? Directory.GetCurrentDirectory(),
                   contentRootPath: options.ContentRootPath,
                   rootNamespace: options.RootNamespace,
                   environment: options.EnvironmentName);
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

        private static readonly Assembly ThisAssembly
            = typeof(CommandLineOptions).GetTypeInfo().Assembly;

        public static string GetVersion()
            => ThisAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
               ?? ThisAssembly.GetName().Version.ToString();
    }
}
