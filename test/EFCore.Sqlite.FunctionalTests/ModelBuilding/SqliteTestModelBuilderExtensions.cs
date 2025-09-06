// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public static class SqliteTestModelBuilderExtensions
{
    public static ModelBuilderTest.TestPropertyBuilder<TProperty> UseAutoincrement<TProperty>(
        this ModelBuilderTest.TestPropertyBuilder<TProperty> builder)
    {
        switch (builder)
        {
            case IInfrastructure<PropertyBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.UseAutoincrement();
                break;
            case IInfrastructure<PropertyBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.UseAutoincrement();
                break;
        }

        return builder;
    }
}
