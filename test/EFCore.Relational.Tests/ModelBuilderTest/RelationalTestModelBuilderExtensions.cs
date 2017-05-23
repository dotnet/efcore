// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.ModelBuilderTest
{
    public static class RelationalTestModelBuilderExtensions
    {
        public static ModelBuilding.ModelBuilderTest.TestPropertyBuilder<TProperty> HasColumnName<TProperty>(
            this ModelBuilding.ModelBuilderTest.TestPropertyBuilder<TProperty> builder, string name)
        {
            var genericBuilder = (builder as IInfrastructure<PropertyBuilder<TProperty>>)?.Instance;

            genericBuilder?.HasColumnName(name);

            return builder;
        }
    }
}
