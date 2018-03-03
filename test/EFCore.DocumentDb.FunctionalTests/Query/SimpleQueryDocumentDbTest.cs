// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQueryDocumentDbTest : SimpleQueryTestBase<NorthwindQueryDocumentDbFixture<NoopModelCustomizer>>
    {
        public SimpleQueryDocumentDbTest(
            NorthwindQueryDocumentDbFixture<NoopModelCustomizer> fixture,
            // ReSharper disable once UnusedParameter.Local
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Select_Property_when_non_shadow()
        {
            base.Select_Property_when_non_shadow();
        }

        public override void Comparing_non_matching_entities_using_Equals()
        {
            base.Comparing_non_matching_entities_using_Equals();
        }

        public override void Select_Where_Subquery_Deep_Single()
        {
            base.Select_Where_Subquery_Deep_Single();
        }

        [ConditionalFact(Skip = "See issue#10231")]
        public override void Where_query_composition_is_not_null()
        {
            base.Where_query_composition_is_not_null();
        }

        [ConditionalFact(Skip = "See issue#10231 & issue#8956")]
        public override void Where_subquery_anon()
        {
            base.Where_subquery_anon();
        }

        [ConditionalFact(Skip = "See issue#10231 & issue#8956")]
        public override void Where_subquery_anon_nested()
        {
            base.Where_subquery_anon_nested();
        }

        [ConditionalFact(Skip = "See issue#10231")]
        public override void Select_correlated_subquery_projection()
        {
            base.Select_correlated_subquery_projection();
        }

        public override void DateTime_parse_is_parameterized()
        {
            base.DateTime_parse_is_parameterized();
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        private void AssertContainsSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
