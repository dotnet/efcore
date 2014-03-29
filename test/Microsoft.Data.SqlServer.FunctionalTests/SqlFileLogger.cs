// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.Tests;

namespace Microsoft.Data.SqlServer.FunctionalTests
{
    // Watch the log in PS with: "tail -f $env:userprofile\.klog\sql.log"
    public class SqlFileLogger : TestFileLogger
    {
        public new static readonly ILogger Instance = new SqlFileLogger();

        private SqlFileLogger()
            : base("sql.log")
        {
        }
    }
}
