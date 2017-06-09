// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public class GearsOfWarQueryInMemoryTest : GearsOfWarQueryTestBase<InMemoryTestStore, GearsOfWarQueryInMemoryFixture>
    {
        public GearsOfWarQueryInMemoryTest(GearsOfWarQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalFact(Skip = "issue #8804")]
        public override void Include_reference_on_derived_type_using_lambda()
        {
            base.Include_reference_on_derived_type_using_lambda();
        }

        [ConditionalFact(Skip = "issue #8804")]
        public override void Include_reference_on_derived_type_using_lambda_with_tracking()
        {
            base.Include_reference_on_derived_type_using_lambda_with_tracking();
        }

        [ConditionalFact(Skip = "issue #8804")]
        public override void Include_reference_on_derived_type_using_string()
        {
            base.Include_reference_on_derived_type_using_string();
        }
    }
}
