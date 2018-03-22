using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class CSharpDbContextGeneratorTest
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
                model =>
                {
                    Assert.Empty(model.GetEntityTypes());
                });
        }

        [Fact]
        public void SuppressConnectionStringWarning_works()
        {
            Test(
                modelBuilder => { },
                new ModelCodeGenerationOptions { SuppressConnectionStringWarning = true },
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
                model =>
                {
                    Assert.Empty(model.GetEntityTypes());
                });
        }

        private void Test(
            Action<ModelBuilder> buildModel,
            ModelCodeGenerationOptions options,
            Action<ScaffoldedModel> assertScaffold,
            Action<IModel> assertModel)
        {
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            buildModel(modelBuilder);
            modelBuilder.GetInfrastructure().Metadata.Validate();

            var model = modelBuilder.Model;

            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices();
            new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);

            var generator = services
                .BuildServiceProvider()
                .GetRequiredService<IModelCodeGenerator>();

            var scaffoldedModel = generator.GenerateModel(
                model,
                "TestNamespace",
                /*contextDir:*/ string.Empty,
                "TestDbContext",
                "Initial Catalog=TestDatabase",
                options);
            assertScaffold(scaffoldedModel);

            var build = new BuildSource
            {
                References =
                {
                    BuildReference.ByName("Microsoft.EntityFrameworkCore"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
                    BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer")
                },
                Sources = new List<string>(
                    Enumerable.Concat(
                        new[] { scaffoldedModel.ContextFile.Code },
                        scaffoldedModel.AdditionalFiles.Select(f => f.Code)))
            };

            var assembly = build.BuildInMemory();
            var context = (DbContext)assembly.CreateInstance("TestNamespace.TestDbContext");
            var compiledModel = context.Model;
            assertModel(compiledModel);
        }
    }
}
