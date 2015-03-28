// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Metadata.ModelConventions
{
    public class SqlServerValueGenerationStrategyConventionTest
    {
        [Fact]
        public void Annotation_is_added_when_conventional_model_builder_is_used()
        {
            var model = new SqlServerModelBuilderFactory().CreateConventionBuilder(new Model()).Model;

            Assert.Equal(1, model.Annotations.Count());
            Assert.Equal(SqlServerValueGenerationStrategy.Sequence.ToString(), model.Annotations.First().Value);
        }
    }
}
