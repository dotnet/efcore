// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Metadata
{
    public class AtsModelBuilderSelectorTest
    {
        private readonly AtsModelBuilderSelector _selector = new AtsModelBuilderSelector();

        [Fact]
        public void It_creates_ats_convention_builder()
        {
            var builder = _selector.CreateConventionBuilder(Mock.Of<Model>());
            Assert.NotNull(builder);
            Assert.IsType<AtsConventionModelBuilder>(builder);
        }
    }
}