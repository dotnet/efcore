// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class CSharpDbContextGeneratorTest : ModelCodeGeneratorTestBase
    {
        [ConditionalFact]
        public void Empty_model()
        {
            Test(
                modelBuilder => { },
                new ModelCodeGenerationOptions(),
                code =>
                {
                    Assert.Equal(
                        @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace
{
    public partial class TestDbContext : DbContext
    {
        public TestDbContext()
        {
        }

        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning " + DesignStrings.SensitiveInformationWarning + @"
                optionsBuilder.UseSqlServer(""Initial Catalog=TestDatabase"");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
",
                        code.ContextFile.Code,
                        ignoreLineEndingDifferences: true);

                    Assert.Empty(code.AdditionalFiles);
                },
                model => Assert.Empty(model.GetEntityTypes()));
        }

        [ConditionalFact]
        public void SuppressConnectionStringWarning_works()
        {
            Test(
                modelBuilder => { },
                new ModelCodeGenerationOptions
                {
                    SuppressConnectionStringWarning = true
                },
                code =>
                {
                    Assert.Equal(
                        @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace
{
    public partial class TestDbContext : DbContext
    {
        public TestDbContext()
        {
        }

        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(""Initial Catalog=TestDatabase"");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
",
                        code.ContextFile.Code,
                        ignoreLineEndingDifferences: true);

                    Assert.Empty(code.AdditionalFiles);
                },
                model => Assert.Empty(model.GetEntityTypes()));
        }

        [ConditionalFact]
        public void Plugins_work()
        {
            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices();
            new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);
            services.AddSingleton<IProviderCodeGeneratorPlugin, TestCodeGeneratorPlugin>();

            var generator = services
                .BuildServiceProvider()
                .GetRequiredService<IModelCodeGenerator>();

            var scaffoldedModel = generator.GenerateModel(
                new Model(),
                new ModelCodeGenerationOptions
                {
                    SuppressConnectionStringWarning = true,
                    ModelNamespace = "TestNamespace",
                    ContextName = "TestDbContext",
                    ConnectionString = "Initial Catalog=TestDatabase"
                });

            Assert.Contains(
                @"optionsBuilder.UseSqlServer(""Initial Catalog=TestDatabase"", x => x.SetProviderOption()).SetContextOption();",
                scaffoldedModel.ContextFile.Code);
        }

        [Fact]
        public void Comments_use_fluent_api()
        {
            Test(
                modelBuilder => modelBuilder.Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("Property")
                            .HasComment("An int property");
                    }),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(
                    ".HasComment(\"An int property\")",
                    code.ContextFile.Code),
                model => Assert.Equal(
                    "An int property",
                    model.FindEntityType("TestNamespace.Entity").GetProperty("Property").GetComment()));
        }

        [Fact]
        public void Entity_comments_use_fluent_api()
        {
            Test(
                modelBuilder => modelBuilder.Entity(
                    "Entity",
                    x =>
                    {
                        x.HasComment("An entity comment");
                    }),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(
                    ".HasComment(\"An entity comment\")",
                    code.ContextFile.Code),
                model => Assert.Equal(
                    "An entity comment",
                    model.FindEntityType("TestNamespace.Entity").GetComment()));
        }

        private class TestCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
        {
            public override MethodCallCodeFragment GenerateProviderOptions()
                => new MethodCallCodeFragment("SetProviderOption");

            public override MethodCallCodeFragment GenerateContextOptions()
                => new MethodCallCodeFragment("SetContextOption");
        }
    }
}
