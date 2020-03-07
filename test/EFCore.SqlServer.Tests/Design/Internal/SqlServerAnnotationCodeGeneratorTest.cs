// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class SqlServerAnnotationCodeGeneratorTest
    {
        [ConditionalFact]
        public void GenerateFluentApi_IKey_works_when_clustered()
        {
            var generator = new SqlServerAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies());
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id").IsClustered();
                });
            var key = modelBuilder.Model.FindEntityType("Post").GetKeys().Single();
            var annotation = key.FindAnnotation(SqlServerAnnotationNames.Clustered);

            var result = generator.GenerateFluentApi(key, annotation);

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(0, result.Arguments.Count);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IKey_works_when_nonclustered()
        {
            var generator = new SqlServerAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies());
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id").IsClustered(false);
                });
            var key = modelBuilder.Model.FindEntityType("Post").GetKeys().Single();
            var annotation = key.FindAnnotation(SqlServerAnnotationNames.Clustered);

            var result = generator.GenerateFluentApi(key, annotation);

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(1, result.Arguments.Count);
            Assert.Equal(false, result.Arguments[0]);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IIndex_works_when_clustered()
        {
            var generator = new SqlServerAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies());
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasIndex("Name").IsClustered();
                });
            var index = modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();
            var annotation = index.FindAnnotation(SqlServerAnnotationNames.Clustered);

            var result = generator.GenerateFluentApi(index, annotation);

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(0, result.Arguments.Count);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IIndex_works_when_nonclustered()
        {
            var generator = new SqlServerAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies());
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasIndex("Name").IsClustered(false);
                });
            var index = modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();
            var annotation = index.FindAnnotation(SqlServerAnnotationNames.Clustered);

            var result = generator.GenerateFluentApi(index, annotation);

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(1, result.Arguments.Count);
            Assert.Equal(false, result.Arguments[0]);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IIndex_works_with_includes()
        {
            var generator = new SqlServerAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies());
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("FirstName");
                    x.Property<string>("LastName");
                    x.HasIndex("LastName").IncludeProperties("FirstName");
                });
            var index = modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();
            var annotation = index.FindAnnotation(SqlServerAnnotationNames.Include);

            var result = generator.GenerateFluentApi(index, annotation);

            Assert.Equal("IncludeProperties", result.Method);

            Assert.Equal(1, result.Arguments.Count);
            var properties = Assert.IsType<string[]>(result.Arguments[0]);
            Assert.Equal(new[] { "FirstName" }, properties.AsEnumerable());
        }
    }
}
