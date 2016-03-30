// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests.Metadata.Conventions
{
    public class SqlServerValueGenerationStrategyConventionTest
    {
        [Fact]
        public void Annotations_are_added_when_conventional_model_builder_is_used()
        {
            var model = SqlServerTestHelpers.Instance.CreateConventionBuilder().Model;

            Assert.Equal(1, model.GetAnnotations().Count());

            Assert.Equal(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy, model.GetAnnotations().Single().Name);
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, model.GetAnnotations().Single().Value);
        }

        [Fact]
        public void Annotations_are_added_when_conventional_model_builder_is_used_with_sequences()
        {
            var model = SqlServerTestHelpers.Instance.CreateConventionBuilder()
                .ForSqlServerUseSequenceHiLo()
                .Model;

            var annotations = model.GetAnnotations().OrderBy(a => a.Name);
            Assert.Equal(3, annotations.Count());

            Assert.Equal(SqlServerFullAnnotationNames.Instance.HiLoSequenceName, annotations.ElementAt(0).Name);
            Assert.Equal(SqlServerModelAnnotations.DefaultHiLoSequenceName, annotations.ElementAt(0).Value);

            Assert.Equal(
                SqlServerFullAnnotationNames.Instance.SequencePrefix +
                "." +
                SqlServerModelAnnotations.DefaultHiLoSequenceName,
                annotations.ElementAt(1).Name);
            Assert.NotNull(annotations.ElementAt(1).Value);

            Assert.Equal(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy, annotations.ElementAt(2).Name);
            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, annotations.ElementAt(2).Value);
        }
    }
}
