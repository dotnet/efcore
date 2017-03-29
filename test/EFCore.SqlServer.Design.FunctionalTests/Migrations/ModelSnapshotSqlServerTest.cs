// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.FunctionalTests.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Xunit;

#if NETCOREAPP2_0
using System.Reflection;
#endif

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class ModelSnapshotSqlServerTest : ModelSnapshotTest
    {
        public override void Model_annotations_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.HasAnnotation("AnnotationName", "AnnotationValue"); },
                @"builder
    .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
    .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);
",
                o =>
                    {
                        Assert.Equal(2, o.GetAnnotations().Count());
                        Assert.Equal("AnnotationValue", o["AnnotationName"]);
                    });
        }

        public override void Model_default_schema_annotation_is_stored_in_snapshot_as_fluent_api()
        {
            Test(
                builder =>
                    {
                        builder.HasDefaultSchema("DefaultSchema");
                        builder.HasAnnotation("AnnotationName", "AnnotationValue");
                    },
                @"builder
    .HasDefaultSchema(""DefaultSchema"")
    .HasAnnotation(""AnnotationName"", ""AnnotationValue"")
    .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);
",
                o =>
                    {
                        Assert.Equal(3, o.GetAnnotations().Count());
                        Assert.Equal("AnnotationValue", o["AnnotationName"]);
                        Assert.Equal("DefaultSchema", o[RelationalFullAnnotationNames.Instance.DefaultSchema]);
                    });
        }

        protected override string GetHeading() => @"builder
    .HasAnnotation(""SqlServer:ValueGenerationStrategy"", SqlServerValueGenerationStrategy.IdentityColumn);
";

        protected override ICollection<BuildReference> GetReferences()
        {
            var references = base.GetReferences();
            references.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer"));

            return references;
        }

        protected override ModelBuilder CreateConventionalModelBuilder() => new ModelBuilder(SqlServerConventionSetBuilder.Build());
    }
}
