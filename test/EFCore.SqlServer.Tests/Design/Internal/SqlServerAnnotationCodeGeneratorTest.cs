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
        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void GenerateFluentApi_IKey_works_when_clustered(bool obsolete)
        {
            var generator = new SqlServerAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies());
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");

                    if (obsolete)
                    {
#pragma warning disable 618
                        x.HasKey("Id").ForSqlServerIsClustered();
#pragma warning restore 618
                    }
                    else
                    {
                        x.HasKey("Id").IsClustered();
                    }
                });
            var key = modelBuilder.Model.FindEntityType("Post").GetKeys().Single();
            var annotation = key.FindAnnotation(SqlServerAnnotationNames.Clustered);

            var result = generator.GenerateFluentApi(key, annotation);

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(0, result.Arguments.Count);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void GenerateFluentApi_IKey_works_when_nonclustered(bool obsolete)
        {
            var generator = new SqlServerAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies());
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");

                    if (obsolete)
                    {
#pragma warning disable 618
                        x.HasKey("Id").ForSqlServerIsClustered(false);
#pragma warning restore 618
                    }
                    else
                    {
                        x.HasKey("Id").IsClustered(false);
                    }
                });
            var key = modelBuilder.Model.FindEntityType("Post").GetKeys().Single();
            var annotation = key.FindAnnotation(SqlServerAnnotationNames.Clustered);

            var result = generator.GenerateFluentApi(key, annotation);

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(1, result.Arguments.Count);
            Assert.Equal(false, result.Arguments[0]);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void GenerateFluentApi_IIndex_works_when_clustered(bool obsolete)
        {
            var generator = new SqlServerAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies());
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    if (obsolete)
                    {
#pragma warning disable 618
                        x.HasIndex("Name").ForSqlServerIsClustered();
#pragma warning restore 618
                    }
                    else
                    {
                        x.HasIndex("Name").IsClustered();
                    }
                });
            var index = modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();
            var annotation = index.FindAnnotation(SqlServerAnnotationNames.Clustered);

            var result = generator.GenerateFluentApi(index, annotation);

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(0, result.Arguments.Count);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void GenerateFluentApi_IIndex_works_when_nonclustered(bool obsolete)
        {
            var generator = new SqlServerAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies());
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    if (obsolete)
                    {
#pragma warning disable 618
                        x.HasIndex("Name").ForSqlServerIsClustered(false);
#pragma warning restore 618
                    }
                    else
                    {
                        x.HasIndex("Name").IsClustered(false);
                    }
                });
            var index = modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();
            var annotation = index.FindAnnotation(SqlServerAnnotationNames.Clustered);

            var result = generator.GenerateFluentApi(index, annotation);

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(1, result.Arguments.Count);
            Assert.Equal(false, result.Arguments[0]);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void GenerateFluentApi_IIndex_works_with_includes(bool obsolete)
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
                    if (obsolete)
                    {
#pragma warning disable 618
                        x.HasIndex("LastName").ForSqlServerInclude("FirstName");
#pragma warning restore 618
                    }
                    else
                    {
                        x.HasIndex("LastName").IncludeProperties("FirstName");
                    }
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
