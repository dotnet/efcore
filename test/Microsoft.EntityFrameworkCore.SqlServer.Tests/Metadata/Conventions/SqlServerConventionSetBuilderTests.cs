// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Tests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Metadata.Conventions.Tests
{
    public class SqlServerConventionSetBuilderTests : ConventionSetBuilderTests
    {
        public override IModel Can_build_a_model_with_default_conventions_without_DI()
        {
            var model = base.Can_build_a_model_with_default_conventions_without_DI();

            Assert.Equal("ProductTable", model.GetEntityTypes().Single().SqlServer().TableName);

            return model;
        }

        protected override ConventionSet GetConventionSet() => SqlServerConventionSetBuilder.Build();
    }
}
