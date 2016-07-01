// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETCOREAPP1_0
using Microsoft.EntityFrameworkCore.Tools.Internal;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Tools.FunctionalTests
{
    [FrameworkSkipCondition(RuntimeFrameworks.CoreCLR, SkipReason = "Code compilation fails")]
    public class ReflectionOperationExecutorTest : OperationExecutorTestBase
    {
        protected override IOperationExecutor CreateExecutorFromBuildResult(BuildFileResult build, string rootNamespace = null) 
            => new ReflectionOperationExecutor(build.TargetPath, build.TargetPath, build.TargetDir, build.TargetDir, build.TargetDir, rootNamespace, environment: null);
    }
}
#endif