// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public interface IProjectContext
    {
        NuGetFramework TargetFramework { get; }
        bool IsClassLibrary { get; }
        string Config { get; }
        string DepsJson { get; }
        string RuntimeConfigJson { get; }
        string PackagesDirectory { get; }
        string AssemblyFullPath { get; }
        string ProjectName { get; }
        string Configuration { get; }
        string ProjectFullPath { get; }
        string RootNamespace { get; }
        string TargetDirectory { get; }
    }
}