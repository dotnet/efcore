// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public class SqlServerConfiguredConditionAttribute : Attribute, ITestCondition
    {
        private static readonly string _dataSource = new SqlConnectionStringBuilder(SqlServerTestStore.CreateConnectionString("sample")).DataSource;
        private readonly bool _isLocalDb = _dataSource.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase);

        public bool IsMet => TestPlatformHelper.IsWindows || !_isLocalDb;

        public string SkipReason => _isLocalDb
            ? "LocalDb is not accessible on this platform. An external SQL Server must be configured."
            : "No test SQL Server has been configured.";
    }
}
