// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.ReverseEngineering
{
    public class SqliteAllFluentApiE2ETest : SqliteE2ETestBase
    {
        public SqliteAllFluentApiE2ETest(ITestOutputHelper output)
            : base(output)
        {
        }

        protected override string DbSuffix { get; } = "FluentApi";
        protected override bool UseDataAnnotations { get; } = false;
        protected override string ExpectedResultsParentDir { get; } = Path.Combine("ReverseEngineering", "Expected", "AllFluentApi");

        protected override string ProviderName => "Microsoft.EntityFrameworkCore.Sqlite.Design";
    }
}
