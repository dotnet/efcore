// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class SqLiteBuiltInDataTypesTest : BuiltInDataTypesTestBase<SqLiteTestStore, SqLiteBuiltInDataTypesFixture>
    {
        public SqLiteBuiltInDataTypesTest(SqLiteBuiltInDataTypesFixture fixture)
            : base(fixture)
        {
        }

        public override void Can_insert_and_read_back_all_non_nullable_data_types()
        {
            base.Can_insert_and_read_back_all_non_nullable_data_types();
        }
    }
}
