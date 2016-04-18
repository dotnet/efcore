// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class BuildCommandFactory
    {
        public static ICommand Create(
               [NotNull] string project,
               [NotNull] string configuration,
               [NotNull] NuGetFramework framework,
               [CanBeNull] string buildBasePath,
               [CanBeNull] string output)
        {
            // TODO: Specify --runtime?
            var args = new List<string>()
            {
                project,
                "--configuration", configuration,
                "--framework", framework.GetShortFolderName()
            };

            if (buildBasePath != null)
            {
                args.Add("--build-base-path");
                args.Add(buildBasePath);
            }

            if (output != null)
            {
                args.Add("--output");
                args.Add(output);
            }

            return Command.CreateDotNet(
                    "build",
                    args,
                    framework,
                    configuration);
        }
    }
}
