// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
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
