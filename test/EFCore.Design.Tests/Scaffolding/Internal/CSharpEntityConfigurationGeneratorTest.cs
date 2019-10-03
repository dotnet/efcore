using System;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class CSharpEntityConfigurationGeneratorTest : ModelCodeGeneratorTestBase
    {
        [ConditionalFact]
        public void Composite_key()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Post",
                        x =>
                        {
                            x.Property<int>("Key");
                            x.Property<int>("Serial");
                            x.HasKey("Key", "Serial");
                        }),
                new ModelCodeGenerationOptions { GenerateEntityTypeConfigurationFiles = true, EntityTypeConfigurationClassSuffix = "EntityConfiguration" },
                code =>
                {
                    var postFile = code.AdditionalFiles.First(f => f.Path == "PostEntityConfiguration.cs");
                    Assert.Equal(
                        @"using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TestNamespace
{
    public partial class PostEntityConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(e => new { e.Key, e.Serial });
        }
    }
}
",
                        postFile.Code);
                },
                model =>
                {
                    var postType = model.FindEntityType("TestNamespace.Post");
                    Assert.NotNull(postType.FindPrimaryKey());
                });
        }

        [ConditionalFact]
        public void Views_generate_ToView_method()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Vista").ToView("Vistas", "test"),
                new ModelCodeGenerationOptions { GenerateEntityTypeConfigurationFiles = true, EntityTypeConfigurationClassSuffix = "EntityConfiguration" },
                code => Assert.Contains(
                    "ToView(\"Vistas\", \"test\");",
                    code.AdditionalFiles.First(f => f.Path == "VistaEntityConfiguration.cs").Code),
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Vista");
                    Assert.Equal("Vistas", entityType.GetTableName());
                    Assert.Equal("dbo", entityType.GetSchema());
                });
        }

        [ConditionalFact]
        public void Navigation_property_with_same_type_and_property_name()
        {
            Test(
                modelBuilder =>modelBuilder
                   .Entity(
                       "Blog",
                       x => x.Property<int>("Id"))
                   .Entity(
                       "Post",
                       x =>
                       {
                           x.Property<int>("Id");
                           x.Property<int>("BlogId");
                           x.HasOne("Blog", "Blog").WithMany("Posts").HasForeignKey("BlogId");
                       }),
                new ModelCodeGenerationOptions { GenerateEntityTypeConfigurationFiles = true, EntityTypeConfigurationClassSuffix = "EntityConfiguration" },
                code =>
                {
                    var postFile = code.AdditionalFiles.First(f => f.Path == "PostEntityConfiguration.cs");

                    Assert.Equal(@"using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TestNamespace
{
    public partial class PostEntityConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.Property(e => e.Id);

            builder.Property(e => e.BlogId);

            builder.HasOne(d => d.Blog)
                .WithMany(p => p.Posts)
                .WithForeignKey(d => d.BlogId);

            builder.HasIndex(e => e.BlogId);
        }
    }
}
",
                        postFile.Code);
                },
                model =>
                {
                    var postType = model.FindEntityType("TestNamespace.Post");
                    var blogNavigation = postType.FindNavigation("Blog");

                    var foreignKeyProperty = Assert.Single(blogNavigation.ForeignKey.Properties);
                    Assert.Equal("BlogId", foreignKeyProperty.Name);

                    var inverseNavigation = blogNavigation.FindInverse();
                    Assert.Equal("TestNamespace.Blog", inverseNavigation.DeclaringEntityType.Name);
                    Assert.Equal("Posts", inverseNavigation.Name);
                });
        }
    }
}
