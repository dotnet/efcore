// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Tests
{
    public static class RelationalTestModelBuilderExtensions
    {
        public static ModelBuilderTest.TestPropertyBuilder<TProperty> HasColumnName<TProperty>(
            this ModelBuilderTest.TestPropertyBuilder<TProperty> builder, string name)
        {
            var genericBuilder = (builder as IInfrastructure<PropertyBuilder<TProperty>>)?.Instance;
            if (genericBuilder != null)
            {
                genericBuilder.HasColumnName(name);
            }

            return builder;
        }
    }
}
