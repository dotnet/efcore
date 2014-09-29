// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class BuiltInDataTypesTest : BuiltInDataTypesTestBase, IClassFixture<BuiltInDataTypesFixture>
    {
        public BuiltInDataTypesTest(BuiltInDataTypesFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public override void Can_insert_and_read_back_all_non_nullable_data_types()
        {
            using (var testDatabase = ((BuiltInDataTypesFixture)_fixture).CreateSQLiteTestDatabase())
            {
                using (var context = ((BuiltInDataTypesFixture)_fixture).CreateSQLiteContext(testDatabase))
                {
                    Test_insert_and_read_back_all_non_nullable_data_types(context);
                }
            }
        }

        [Fact]
        public override void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_null()
        {
            using (var testDatabase = ((BuiltInDataTypesFixture)_fixture).CreateSQLiteTestDatabase())
            {
                using (var context = ((BuiltInDataTypesFixture)_fixture).CreateSQLiteContext(testDatabase))
                {
                    Test_insert_and_read_back_all_nullable_data_types_with_values_set_to_null(context);
                }
            }
        }

        [Fact]
        public override void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_non_null()
        {
            using (var testDatabase = ((BuiltInDataTypesFixture)_fixture).CreateSQLiteTestDatabase())
            {
                using (var context = ((BuiltInDataTypesFixture)_fixture).CreateSQLiteContext(testDatabase))
                {
                    Test_insert_and_read_back_all_nullable_data_types_with_values_set_to_non_null(context);
                }
            }
        }
    }
}
