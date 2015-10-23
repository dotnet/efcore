// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Sqlite.Interop;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("Microsoft.Data.Sqlite.Tests.TestUtilities.SqliteTestFramework", "Microsoft.Data.Sqlite.Tests")]

namespace Microsoft.Data.Sqlite.Tests.TestUtilities
{
    public class SqliteTestFramework : XunitTestFramework
    {
        private readonly IMessageSink _messageSink;

        public SqliteTestFramework(IMessageSink messageSink)
            : base(messageSink)
        {
            _messageSink = messageSink;
        }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            _messageSink.OnMessage(new Xunit.Sdk.DiagnosticMessage
            {
                Message = $"Using SQLite v{NativeMethods.sqlite3_libversion()}"
            });

            return base.CreateExecutor(assemblyName);
        }
    }
}
