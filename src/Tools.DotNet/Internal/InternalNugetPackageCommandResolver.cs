// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.EntityFrameworkCore.Internal;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public class InternalNugetPackageCommandResolver : ICommandResolver
    {
        private readonly string _basePath = AppContext.BaseDirectory;
        protected virtual string NetCoreToolDir
            => Path.Combine(_basePath, "tools", "netcoreapp1.0");
        protected virtual string DesktopToolDir
            => Path.Combine(_basePath, "tools", "net451");


        public virtual CommandSpec Resolve(ResolverArguments arguments)
            => arguments.Framework.IsDesktop()
                ? CreateDesktopCommandSpec(arguments)
                : CreateNetCoreCommandSpec(arguments);

        private CommandSpec CreateNetCoreCommandSpec(ResolverArguments arguments)
        {
            var runtimeConfigPath = Path.Combine(NetCoreToolDir, "ef" + FileNameSuffixes.RuntimeConfigJson);
            var args = new List<string>();

            args.Add("exec");

            args.Add("--runtimeconfig");
            args.Add(runtimeConfigPath);

            if (arguments.DepsJsonFile == null)
            {
                throw new ArgumentNullException("Deps json file cannot be null for this framework");
            }

            args.Add("--depsfile");
            args.Add(arguments.DepsJsonFile);

            if (!string.IsNullOrEmpty(arguments.NuGetPackageRoot))
            {
                args.Add("--additionalprobingpath");
                args.Add(arguments.NuGetPackageRoot);
            }

            var commandPath = Path.Combine(NetCoreToolDir, "ef" + FileNameSuffixes.DotNet.DynamicLib);
            args.Add(commandPath);
            args.AddRange(arguments.CommandArguments);

            var muxer = new Muxer();
            var escapedArgs = ArgumentEscaper.EscapeAndConcatenateArgArrayForProcessStart(args.OrEmptyIfNull());

            Reporter.Verbose.WriteLine("Executing " + muxer.MuxerPath + " " + escapedArgs);

            return new CommandSpec(muxer.MuxerPath, escapedArgs, CommandResolutionStrategy.ProjectToolsPackage);
        }

        private CommandSpec CreateDesktopCommandSpec(ResolverArguments arguments)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new OperationErrorException(ToolsDotNetStrings.DesktopCommandsRequiresWindows(arguments.Framework));
            }

            var exeName = RuntimeInformation.OSArchitecture == Architecture.X86
                ? "ef.x86" + FileNameSuffixes.Windows.Exe
                : "ef" + FileNameSuffixes.Windows.Exe;
            var path = Path.Combine(DesktopToolDir, exeName);

            return new CommandSpec(path,
                ArgumentEscaper.EscapeAndConcatenateArgArrayForProcessStart(arguments.CommandArguments.OrEmptyIfNull()),
                CommandResolutionStrategy.ProjectToolsPackage);
        }
    }
}
