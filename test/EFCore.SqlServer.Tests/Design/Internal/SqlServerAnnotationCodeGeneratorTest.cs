﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class SqlServerAnnotationCodeGeneratorTest
    {
        [ConditionalFact]
        public void GenerateFluentApi_IKey_works_when_clustered()
        {
            var generator = CreateGenerator();

            var modelBuilder = SqlServerConventionSetBuilder.CreateModelBuilder();
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id").IsClustered();
                });
            var key = (IKey)modelBuilder.Model.FindEntityType("Post").GetKeys().Single();

            var result = generator.GenerateFluentApiCalls(key, key.GetAnnotations().ToDictionary(a => a.Name, a => a))
                .Single();

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(0, result.Arguments.Count);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IKey_works_when_nonclustered()
        {
            var generator = CreateGenerator();
            var modelBuilder = SqlServerConventionSetBuilder.CreateModelBuilder();
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id").IsClustered(false);
                });
            var key = (IKey)modelBuilder.Model.FindEntityType("Post").GetKeys().Single();

            var result = generator.GenerateFluentApiCalls(key, key.GetAnnotations().ToDictionary(a => a.Name, a => a))
                .Single();

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(1, result.Arguments.Count);
            Assert.Equal(false, result.Arguments[0]);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IIndex_works_when_clustered()
        {
            var generator = CreateGenerator();
            var modelBuilder = SqlServerConventionSetBuilder.CreateModelBuilder();
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasIndex("Name").IsClustered();
                });
            var index = (IIndex)modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();

            var result = generator.GenerateFluentApiCalls(index, index.GetAnnotations().ToDictionary(a => a.Name, a => a))
                .Single();

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(0, result.Arguments.Count);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IIndex_works_when_nonclustered()
        {
            var generator = CreateGenerator();
            var modelBuilder = SqlServerConventionSetBuilder.CreateModelBuilder();
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasIndex("Name").IsClustered(false);
                });
            var index = (IIndex)modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();

            var result = generator.GenerateFluentApiCalls(index, index.GetAnnotations().ToDictionary(a => a.Name, a => a))
                .Single();

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(1, result.Arguments.Count);
            Assert.Equal(false, result.Arguments[0]);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IIndex_works_with_fillfactor()
        {
            var generator = CreateGenerator();
            var modelBuilder = SqlServerConventionSetBuilder.CreateModelBuilder();
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasIndex("Name").HasFillFactor(90);
                });

            var index = (IIndex)modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();
            var result = generator.GenerateFluentApiCalls(index, index.GetAnnotations().ToDictionary(a => a.Name, a => a))
                .Single();

            Assert.Equal("HasFillFactor", result.Method);
            Assert.Equal(1, result.Arguments.Count);
            Assert.Equal(90, result.Arguments[0]);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IIndex_works_with_includes()
        {
            var generator = CreateGenerator();
            var modelBuilder = SqlServerConventionSetBuilder.CreateModelBuilder();
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("FirstName");
                    x.Property<string>("LastName");
                    x.HasIndex("LastName").IncludeProperties("FirstName");
                });

            var index = (IIndex)modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();
            var result = generator.GenerateFluentApiCalls(index, index.GetAnnotations().ToDictionary(a => a.Name, a => a))
                .Single();

            Assert.Equal("IncludeProperties", result.Method);

            Assert.Equal(1, result.Arguments.Count);
            var properties = Assert.IsType<string[]>(result.Arguments[0]);
            Assert.Equal(new[] { "FirstName" }, properties.AsEnumerable());
        }

        [ConditionalFact]
        public void GenerateFluentApi_IModel_works_with_identity()
        {
            var generator = CreateGenerator();
            var modelBuilder = SqlServerConventionSetBuilder.CreateModelBuilder();
            modelBuilder.UseIdentityColumns(seed: 5, increment: 10);

            var annotations = modelBuilder.Model.GetAnnotations().ToDictionary(a => a.Name, a => a);
            var result = generator.GenerateFluentApiCalls((IModel)modelBuilder.Model, annotations).Single();

            Assert.Equal("UseIdentityColumns", result.Method);

            Assert.Collection(
                result.Arguments,
                seed => Assert.Equal(5L, seed),
                increment => Assert.Equal(10, increment));
        }

        [ConditionalFact]
        public void GenerateFluentApi_IProperty_works_with_identity()
        {
            var generator = CreateGenerator();
            var modelBuilder = SqlServerConventionSetBuilder.CreateModelBuilder();
            modelBuilder.Entity("Post", x => x.Property<int>("Id").UseIdentityColumn(5, 10));
            var property = modelBuilder.Model.FindEntityType("Post").FindProperty("Id");

            var annotations = property.GetAnnotations().ToDictionary(a => a.Name, a => a);
            var result = generator.GenerateFluentApiCalls((IProperty)property, annotations).Single();

            Assert.Equal("UseIdentityColumn", result.Method);

            Assert.Collection(
                result.Arguments,
                seed => Assert.Equal(5L, seed),
                increment => Assert.Equal(10, increment));
        }

        [ConditionalFact]
        public void GenerateFluentApi_IModel_works_with_HiLo()
        {
            var generator = CreateGenerator();
            var modelBuilder = SqlServerConventionSetBuilder.CreateModelBuilder();
            modelBuilder.UseHiLo("HiLoIndexName", "HiLoIndexSchema");

            var annotations = modelBuilder.Model.GetAnnotations().ToDictionary(a => a.Name, a => a);
            var result = generator.GenerateFluentApiCalls((IModel)modelBuilder.Model, annotations).Single();

            Assert.Equal("UseHiLo", result.Method);

            Assert.Collection(
                result.Arguments,
                name => Assert.Equal("HiLoIndexName", name),
                schema => Assert.Equal("HiLoIndexSchema", schema));
        }

        [ConditionalFact]
        public void GenerateFluentApi_IProperty_works_with_HiLo()
        {
            var generator = CreateGenerator();
            var modelBuilder = SqlServerConventionSetBuilder.CreateModelBuilder();
            modelBuilder.Entity("Post", x => x.Property<int>("Id").UseHiLo("HiLoIndexName", "HiLoIndexSchema"));
            var property = modelBuilder.Model.FindEntityType("Post").FindProperty("Id");

            var annotations = property.GetAnnotations().ToDictionary(a => a.Name, a => a);
            var result = generator.GenerateFluentApiCalls((IProperty)property, annotations).Single();

            Assert.Equal("UseHiLo", result.Method);

            Assert.Collection(
                result.Arguments,
                name => Assert.Equal("HiLoIndexName", name),
                schema => Assert.Equal("HiLoIndexSchema", schema));
        }

        [ConditionalFact]
        public void GenerateFluentApi_IProperty_works_with_IsSparse()
        {
            var generator = CreateGenerator();
            var modelBuilder = SqlServerConventionSetBuilder.CreateModelBuilder();
            modelBuilder.Entity("SomeEntity", x =>
            {
                x.Property<string>("Default");
                x.Property<string>("Sparse").IsSparse();
                x.Property<string>("NonSparse").IsSparse(false);
            });

            Assert.Null(GenerateFluentApiCall("SomeEntity", "Default"));

            var sparseCall = GenerateFluentApiCall("SomeEntity", "Sparse");
            Assert.Equal("IsSparse", sparseCall.Method);
            Assert.Empty(sparseCall.Arguments);

            var nonSparseCall = GenerateFluentApiCall("SomeEntity", "NonSparse");
            Assert.Equal("IsSparse", nonSparseCall.Method);
            Assert.Collection(nonSparseCall.Arguments, o => Assert.False((bool)o));

            MethodCallCodeFragment GenerateFluentApiCall(string entityTypeName, string propertyName)
            {
                var property = modelBuilder.Model.FindEntityType(entityTypeName).FindProperty(propertyName);
                var annotations = property.GetAnnotations().ToDictionary(a => a.Name, a => a);
                return generator.GenerateFluentApiCalls((IProperty)property, annotations).SingleOrDefault();
            }
        }

        private SqlServerAnnotationCodeGenerator CreateGenerator()
            => new(
                new AnnotationCodeGeneratorDependencies(
                    new SqlServerTypeMappingSource(
                        new TypeMappingSourceDependencies(
                            new ValueConverterSelector(
                                new ValueConverterSelectorDependencies()),
                            Array.Empty<ITypeMappingSourcePlugin>()),
                        new RelationalTypeMappingSourceDependencies(
                            Array.Empty<IRelationalTypeMappingSourcePlugin>()))));
    }
}
