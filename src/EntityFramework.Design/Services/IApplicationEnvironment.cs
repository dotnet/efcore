// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.Versioning;

namespace Microsoft.Framework.Runtime
{
    [AssemblyNeutral]
    public interface IApplicationEnvironment
    {
        string ApplicationName { get; }
        string Version { get; }
        string ApplicationBasePath { get; }
        string Configuration { get; }
        FrameworkName TargetFramework { get; }
    }
}
