// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

#if NETCOREAPP2_0
[assembly: OsSkipCondition(TestPlatform.Windows, WindowsVersions.Win2008R2, WindowsVersions.Win7, SkipReason = "SqlClient has issue with .netcoreapp2.0 on win7.")]
#endif
