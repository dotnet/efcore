// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class CosmosConventionSetBuilderTests : ConventionSetBuilderTests
{
    public override IReadOnlyModel Can_build_a_model_with_default_conventions_without_DI_new()
    {
        var model = base.Can_build_a_model_with_default_conventions_without_DI_new();

        Assert.Equal("DbContext", model.GetEntityTypes().Single().GetContainer());

        return model;
    }

    protected override ModelBuilder GetModelBuilder()
        => CosmosConventionSetBuilder.CreateModelBuilder();
}
