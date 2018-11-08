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
        [Fact]
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
        {}
    }
}
",
                        code.ContextFile.Code,
                        ignoreLineEndingDifferences: true);

                    Assert.Empty(code.AdditionalFiles);
                },
                model => Assert.Empty(model.GetEntityTypes()));
        }

        [Fact]
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
        {}
    }
}
",
                        code.ContextFile.Code,
                        ignoreLineEndingDifferences: true);

                    Assert.Empty(code.AdditionalFiles);
                },
                model => Assert.Empty(model.GetEntityTypes()));
        }

        [Fact]
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
                "TestNamespace",
                "TestNamespace",
                "TestNamespace",
                contextDir: string.Empty,
                "TestDbContext",
                "Initial Catalog=TestDatabase",
                new ModelCodeGenerationOptions { SuppressConnectionStringWarning = true });

            Assert.Contains(
                @"optionsBuilder.UseSqlServer(""Initial Catalog=TestDatabase"", x => x.SetProviderOption()).SetContextOption();",
                scaffoldedModel.ContextFile.Code);
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
