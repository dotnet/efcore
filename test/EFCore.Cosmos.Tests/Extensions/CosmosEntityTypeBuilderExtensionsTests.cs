// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public class CosmosEntityTypeBuilderExtensionsTests : ModelValidatorTestBase
    {
        protected override TestHelpers TestHelpers => CosmosTestHelpers.Instance;

        [ConditionalFact]
        public void Can_set_etag_concurrency()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().UseEtagConcurrency();
            var model = modelBuilder.Model;

            var etagProperty = model.FindEntityType(typeof(Customer).FullName).FindProperty("_etag");
            Assert.NotNull(etagProperty);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, etagProperty.ValueGenerated);
            Assert.True(etagProperty.IsConcurrencyToken);
        }
    }
}
