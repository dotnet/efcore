// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class CosmosConventionSetBuilderTests : ConventionSetBuilderTests
    {
        public override IModel Can_build_a_model_with_default_conventions_without_DI()
        {
            return null;
        }

        public override IModel Can_build_a_model_with_default_conventions_without_DI_new()
        {
            var model = base.Can_build_a_model_with_default_conventions_without_DI_new();

            Assert.Equal("DbContext", model.GetEntityTypes().Single().GetContainer());

            return model;
        }

        protected override ModelBuilder GetModelBuilder()
            => CosmosConventionSetBuilder.CreateModelBuilder();
    }
}
