// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQueryOracleTest : GearsOfWarQueryTestBase<GearsOfWarQueryOracleFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public GearsOfWarQueryOracleTest(GearsOfWarQueryOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

#if !Test21
        [ConditionalTheory(Skip = "issue #10513")]
        public override Task Correlated_collection_with_complex_order_by_funcletized_to_constant_bool(bool isAsync)
        {
            return Task.CompletedTask;
        }
#endif
    }
}
