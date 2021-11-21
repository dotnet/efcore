// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindDbFunctionsQueryCosmosTest : NorthwindDbFunctionsQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
    {
        public NorthwindDbFunctionsQueryCosmosTest(
            NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
        }

        public override Task Like_all_literals(bool async)
            => AssertTranslationFailed(() => base.Like_all_literals(async));

        public override Task Like_all_literals_with_escape(bool async)
            => AssertTranslationFailed(() => base.Like_all_literals_with_escape(async));

        public override Task Like_literal(bool async)
            => AssertTranslationFailed(() => base.Like_literal(async));

        public override Task Like_literal_with_escape(bool async)
            => AssertTranslationFailed(() => base.Like_literal_with_escape(async));

        public override Task Like_identity(bool async)
            => AssertTranslationFailed(() => base.Like_identity(async));

        public override async Task Random_return_less_than_1(bool async)
        {
            await base.Random_return_less_than_1(async);

            AssertSql(
                @"SELECT COUNT(1) AS c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (RAND() < 1.0))");
        }

        public override async Task Random_return_greater_than_0(bool async)
        {
            await base.Random_return_greater_than_0(async);

            AssertSql(
                @"SELECT COUNT(1) AS c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (RAND() >= 0.0))");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
