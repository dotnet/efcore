// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class CSharpEntityTypeGeneratorTest : ModelCodeGeneratorTestBase
    {
        [Fact]
        public void Navigation_properties()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Person",
                        x => x.Property<int>("Id"))
                    .Entity(
                        "Contribution",
                        x => x.Property<int>("Id"))
                    .Entity(
                        "Post",
                        x =>
                        {
                            x.Property<int>("Id");

                            x.HasOne("Person", "Author").WithMany();
                            x.HasMany("Contribution", "Contributions").WithOne();
                        }),
                new ModelCodeGenerationOptions
                {
                    UseDataAnnotations = true
                },
                code =>
                {
                    var postFile = code.AdditionalFiles.First(f => f.Path == "Post.cs");
                    Assert.Equal(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestNamespace
{
    public partial class Post
    {
        public Post()
        {
            Contributions = new HashSet<Contribution>();
        }

        public int Id { get; set; }
        public int? AuthorId { get; set; }

        [ForeignKey(""AuthorId"")]
        public virtual Person Author { get; set; }
        public virtual ICollection<Contribution> Contributions { get; set; }
    }
}
",
                        postFile.Code);
                },
                model =>
                {
                    var postType = model.FindEntityType("TestNamespace.Post");
                    var authorNavigation = postType.FindNavigation("Author");
                    Assert.True(authorNavigation.IsDependentToPrincipal());
                    Assert.Equal("TestNamespace.Person", authorNavigation.ForeignKey.PrincipalEntityType.Name);

                    var contributionsNav = postType.FindNavigation("Contributions");
                    Assert.False(contributionsNav.IsDependentToPrincipal());
                    Assert.Equal("TestNamespace.Contribution", contributionsNav.ForeignKey.DeclaringEntityType.Name);
                });
        }
    }
}
