// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class SqlServerConventionSetBuilderTests : ConventionSetBuilderTests
{
    public override IReadOnlyModel Can_build_a_model_with_default_conventions_without_DI()
    {
        var model = base.Can_build_a_model_with_default_conventions_without_DI();

        Assert.Equal("ProductTable", model.GetEntityTypes().Single().GetTableName());

        return model;
    }

    protected override ConventionSet GetConventionSet()
        => SqlServerConventionSetBuilder.Build();

    protected override ModelBuilder GetModelBuilder()
        => SqlServerConventionSetBuilder.CreateModelBuilder();
}
