// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public sealed class SqlServerConfiguredConditionAttribute : Attribute, ITestCondition
    {
        private static readonly string _dataSource = new SqlConnectionStringBuilder(SqlServerTestStore.CreateConnectionString("sample")).DataSource;
        private readonly bool _isLocalDb = _dataSource.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase);

        public bool IsMet => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !_isLocalDb;

        public string SkipReason => _isLocalDb
            ? "LocalDb is not accessible on this platform. An external SQL Server must be configured."
            : "No test SQL Server has been configured.";
    }
}
