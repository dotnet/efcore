// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindWhereQueryInMemoryTest : NorthwindWhereQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public NorthwindWhereQueryInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
            ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Where_bool_client_side_negated(bool async)
        {
            return base.Where_bool_client_side_negated(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Where_equals_method_string_with_ignore_case(bool async)
        {
            return base.Where_equals_method_string_with_ignore_case(async);
        }

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Where_equals_on_null_nullable_int_types(bool async)
        {
            return base.Where_equals_on_null_nullable_int_types(async);
        }

        public override async Task<string> Where_simple_closure(bool async)
        {
            var queryString = await base.Where_simple_closure(async);

            Assert.Equal(InMemoryStrings.NoQueryStrings, queryString);

            return null;
        }

        // Casting int to object to string is invalid for InMemory
        public override Task Like_with_non_string_column_using_double_cast(bool async)
            => Task.CompletedTask;
    }
}
