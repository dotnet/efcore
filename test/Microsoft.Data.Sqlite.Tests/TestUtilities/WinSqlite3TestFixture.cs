// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: Xunit.TestFramework("Microsoft.Data.Sqlite.WinSqlite3TestFramework", "Microsoft.Data.Sqlite.Tests")]

namespace Microsoft.Data.Sqlite
{
    public class WinSqlite3TestFramework : XunitTestFramework
    {
        public WinSqlite3TestFramework(IMessageSink messageSink)
            : base(messageSink)
        {
            messageSink.OnMessage(new DiagnosticMessage { Message = "Using custom test framework" });
            if (Environment.GetEnvironmentVariable("TEST_WINSQLITE3") != null)
            {
                messageSink.OnMessage(new DiagnosticMessage { Message = "Using winsqlite3" });
                SqliteEngine.UseWinSqlite3();
            }
        }
    }
}