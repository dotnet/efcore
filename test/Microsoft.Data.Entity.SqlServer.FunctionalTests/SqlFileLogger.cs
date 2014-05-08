// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.Logging;

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
