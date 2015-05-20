// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
#if DNXCORE50
using Xunit;
#endif

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class BuiltInDataTypesSqliteTest : BuiltInDataTypesTestBase<BuiltInDataTypesSqliteFixture>
    {
        public BuiltInDataTypesSqliteTest(BuiltInDataTypesSqliteFixture fixture)
            : base(fixture)
        {
        }
        
#if DNXCORE50
        [Fact(Skip = "Fails on Core CLR")]
        public override void Can_insert_and_read_back_all_non_nullable_data_types()
        {
        }

        [Fact(Skip = "Fails on Core CLR")]
        public override void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_non_null()
        {
        }

        [Fact(Skip = "Fails on Core CLR")]
        public override void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_null()
        {
        }
#endif
    }
}
