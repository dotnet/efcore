// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FunkyDataQueryOracleTest : FunkyDataQueryTestBase<FunkyDataQueryOracleTest.FunkyDataQueryOracleFixture>
    {
        public FunkyDataQueryOracleTest(FunkyDataQueryOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [Fact(Skip = "TODO")]
        public override void String_starts_with_on_argument_with_wildcard_parameter()
        {
            base.String_starts_with_on_argument_with_wildcard_parameter();
        }

        [Fact(Skip = "TODO")]
        public override void String_ends_with_on_argument_with_wildcard_parameter()
        {
            base.String_ends_with_on_argument_with_wildcard_parameter();
        }

        [Fact(Skip = "TODO")]
        public override void String_starts_with_on_argument_with_wildcard_column_negated()
        {
            base.String_starts_with_on_argument_with_wildcard_column_negated();
        }

        [Fact(Skip = "TODO")]
        public override void String_ends_with_inside_conditional_negated()
        {
            base.String_ends_with_inside_conditional_negated();
        }

        [Fact(Skip = "TODO")]
        public override void String_starts_with_on_argument_with_wildcard_constant()
        {
            base.String_starts_with_on_argument_with_wildcard_constant();
        }

        [Fact(Skip = "TODO")]
        public override void String_ends_with_on_argument_with_wildcard_column_negated()
        {
            base.String_ends_with_on_argument_with_wildcard_column_negated();
        }

        [Fact(Skip = "TODO")]
        public override void String_ends_with_on_argument_with_wildcard_constant()
        {
            base.String_ends_with_on_argument_with_wildcard_constant();
        }

        public class FunkyDataQueryOracleFixture : FunkyDataQueryFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;

            public override FunkyDataContext CreateContext()
            {
                var context = base.CreateContext();
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                return context;
            }
        }
    }
}
