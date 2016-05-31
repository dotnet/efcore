// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.Core
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class SqlServerRequiredAttribute : Attribute, ITestCondition
    {
        public bool IsMet => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                             || !BenchmarkConfig.Instance.BenchmarkDatabase.Contains("(localdb)");

        public string SkipReason => "Must configured an external SQL Server to run the tests on this platform";
    }
}
