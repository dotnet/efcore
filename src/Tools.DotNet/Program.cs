// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.EntityFrameworkCore.Tools.DotNet.Internal;

// ReSharper disable ArgumentsStyleNamedExpression
namespace Microsoft.EntityFrameworkCore.Tools.DotNet
{
    public class Program
    {
        public static int Main([NotNull] string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            try
            {
                var options = CommandLineOptions.Parse(args);
                if (options.IsHelp)
                {
                    return 2;
                }

                HandleVerboseContext(options);

                return new DispatchOperationExecutor(
                        new ProjectContextCommandFactory(new InternalNugetPackageCommandResolver()), 
                        GetVersion())
                    .Execute(options);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }

                if (!(ex is OperationErrorException))
                {
                    Reporter.Error.WriteLine(ex.ToString());
                }

                Reporter.Error.WriteLine(ex.Message.Bold().Red());
                return 1;
            }
        }

        private static void HandleVerboseContext(CommandLineOptions options)
        {
            bool isVerbose;
            bool.TryParse(Environment.GetEnvironmentVariable(CommandContext.Variables.Verbose), out isVerbose);

            options.IsVerbose = options.IsVerbose || isVerbose;

            if (options.IsVerbose)
            {
                Environment.SetEnvironmentVariable(CommandContext.Variables.Verbose, bool.TrueString);
            }
        }

        private static readonly Assembly ThisAssembly = typeof(Program).GetTypeInfo().Assembly;

        private static readonly string ThisAssemblyVersion = ThisAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                                                             ?? ThisAssembly.GetName().Version.ToString();

        public static string GetVersion() => ThisAssemblyVersion;
    }
}
