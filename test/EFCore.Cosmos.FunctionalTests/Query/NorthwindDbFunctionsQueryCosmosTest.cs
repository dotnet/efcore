// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
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
        {
            return AssertTranslationFailed(() => base.Like_all_literals(async));
        }

        public override Task Like_all_literals_with_escape(bool async)
        {
            return AssertTranslationFailed(() => base.Like_all_literals_with_escape(async));
        }

        public override Task Like_literal(bool async)
        {
            return AssertTranslationFailed(() => base.Like_literal(async));
        }

        public override Task Like_literal_with_escape(bool async)
        {
            return AssertTranslationFailed(() => base.Like_literal_with_escape(async));
        }

        public override Task Like_identity(bool async)
        {
            return AssertTranslationFailed(() => base.Like_identity(async));
        }

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
