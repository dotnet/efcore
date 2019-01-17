// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class BuiltInDataTypesCosmosTest : BuiltInDataTypesTestBase<BuiltInDataTypesCosmosTest.BuiltInDataTypesCosmosFixture>
    {
        public BuiltInDataTypesCosmosTest(BuiltInDataTypesCosmosFixture fixture)
            : base(fixture)
        {
        }

        public override void Can_query_using_any_nullable_data_type_as_literal()
        {
            // TODO: Requires ReLinq to be removed
        }

        public override void Can_insert_and_read_back_with_binary_key()
        {
            // TODO: For this to work Join needs to be translated or compiled as a Join with custom equality comparer
        }

        public class BuiltInDataTypesCosmosFixture : BuiltInDataTypesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

            public override bool StrictEquality => true;

            public override bool SupportsAnsi => false;

            public override bool SupportsUnicodeToAnsiConversion => false;

            public override bool SupportsLargeStringComparisons => true;

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                var shadowJObject = (Property)modelBuilder.Entity<BuiltInDataTypesShadow>().Property("__jObject").Metadata;
                shadowJObject.SetConfigurationSource(ConfigurationSource.Convention);
                var nullableShadowJObject = (Property)modelBuilder.Entity<BuiltInNullableDataTypesShadow>().Property("__jObject").Metadata;
                nullableShadowJObject.SetConfigurationSource(ConfigurationSource.Convention);
            }
        }
    }
}
