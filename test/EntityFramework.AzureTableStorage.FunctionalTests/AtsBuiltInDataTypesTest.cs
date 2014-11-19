// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
    public class AtsBuiltInDataTypesTest : BuiltInDataTypesTestBase<AtsTestStore, AtsBuiltInDataTypesFixture>
    {
        public AtsBuiltInDataTypesTest(AtsBuiltInDataTypesFixture fixture)
            : base(fixture)
        {
        }

        // TODO: Enable tests once #965 is fixed
        public override void Can_insert_and_read_back_all_non_nullable_data_types()
        {
        }

        public override void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_non_null()
        {
        }

        public override void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_null()
        {
        }
    }
}
