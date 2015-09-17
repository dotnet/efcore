// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Xunit.Abstractions;

namespace EntityFramework.Sqlite.Design.FunctionalTests.ReverseEngineering
{
    public class SqliteAttributesInsteadOfFluentApiE2ETest : SqliteE2ETestBase
    {
        public SqliteAttributesInsteadOfFluentApiE2ETest(ITestOutputHelper output)
            : base(output)
        {
        }

        protected override string DbSuffix { get; } = "Attributes";
        protected override bool UseFluentApiOnly { get; } = false;
        protected override string ExpectedResultsParentDir { get; } = Path.Combine("ReverseEngineering", "Expected", "UseAttributesInsteadOfFluentApi");

        protected override string ProviderName => "EntityFramework.Sqlite.Design";
    }
}
