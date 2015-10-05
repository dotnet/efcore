// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Metadata.Conventions
{
    public class SqlServerValueGenerationStrategyConventionTest
    {
        [Fact]
        public void Annotations_are_added_when_conventional_model_builder_is_used()
        {
            var model = SqlServerTestHelpers.Instance.CreateConventionBuilder().Model;

            Assert.Equal(1, model.Annotations.Count());

            Assert.Equal(SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGenerationStrategy, model.Annotations.Single().Name);
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, model.Annotations.Single().Value);
        }

        [Fact]
        public void Annotations_are_added_when_conventional_model_builder_is_used_with_sequences()
        {
            var model = SqlServerTestHelpers.Instance.CreateConventionBuilder()
                .ForSqlServerUseSequenceHiLo()
                .Model;

            var annotations = model.Annotations.OrderBy(a => a.Name);
            Assert.Equal(3, annotations.Count());

            Assert.Equal(SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.HiLoSequenceName, annotations.ElementAt(0).Name);
            Assert.Equal(SqlServerAnnotationNames.DefaultHiLoSequenceName, annotations.ElementAt(0).Value);

            Assert.Equal(
                SqlServerAnnotationNames.Prefix +
                RelationalAnnotationNames.Sequence +
                "." +
                SqlServerAnnotationNames.DefaultHiLoSequenceName,
                annotations.ElementAt(1).Name);
            Assert.NotNull(annotations.ElementAt(1).Value);

            Assert.Equal(SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGenerationStrategy, annotations.ElementAt(2).Name);
            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, annotations.ElementAt(2).Value);
        }
    }
}
