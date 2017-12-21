// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class CSharpModelGeneratorTest
    {
        [Fact]
        public void Language_works()
        {
            var generator = CreateGenerator();

            var result = generator.Language;

            Assert.Equal("C#", result);
        }

        [Fact]
        public void WriteCode_works()
        {
            var generator = CreateGenerator();
            var modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder();
            modelBuilder.Entity("TestEntity").Property<int>("Id").HasAnnotation(ScaffoldingAnnotationNames.ColumnOrdinal, 0);

            var result = generator.GenerateModel(
                modelBuilder.Model,
                "TestNamespace",
                "TestContext",
                "Data Source=Test",
                dataAnnotations: true);

            Assert.Equal("TestContext.cs", result.ContextFile.Path);
            Assert.Equal(
                @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace
{
    public partial class TestContext : DbContext
    {
        public virtual DbSet<TestEntity>  { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseTestProvider(@""Data Source=Test"");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {}
    }
}
",
                result.ContextFile.Code,
                ignoreLineEndingDifferences: true);

            Assert.Equal(1, result.AdditionalFiles.Count);
            Assert.Equal("TestEntity.cs", result.AdditionalFiles[0].Path);
            Assert.Equal(
                @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestNamespace
{
    [Table(""TestEntity"")]
    public partial class TestEntity
    {
        public int Id { get; set; }
    }
}
",
                result.AdditionalFiles[0].Code,
                ignoreLineEndingDifferences: true);
        }

        private static IModelCodeGenerator CreateGenerator()
            => new ServiceCollection()
            .AddEntityFrameworkDesignTimeServices()
            .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
            .AddSingleton<IProviderCodeGenerator, TestProviderCodeGenerator>()
            .BuildServiceProvider()
            .GetRequiredService<IModelCodeGenerator>();
    }
}
