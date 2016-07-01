// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451
using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Tools.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools.FunctionalTests
{
    public class AppDomainOperationExecutorTest : OperationExecutorTestBase
    {
        protected override IOperationExecutor CreateExecutorFromBuildResult(BuildFileResult build, string rootNamespace = null) 
            => new AppDomainOperationExecutor(build.TargetPath,
                build.TargetPath, 
                build.TargetDir,
                build.TargetDir, 
                build.TargetDir, 
                rootNamespace, 
                environment: null, 
                configFile: null);


        [Fact]
        public void Assembly_load_errors_are_wrapped()
        {
            var targetDir = AppDomain.CurrentDomain.BaseDirectory;
            using (var executor = new AppDomainOperationExecutor(Assembly.GetExecutingAssembly().Location, Path.Combine(targetDir, "Unknown.dll"), targetDir, null, null, null, null, null))
            {
                Assert.Throws<OperationErrorException>(() => executor.GetContextTypes());
            }
        }
    }
}
#endif
