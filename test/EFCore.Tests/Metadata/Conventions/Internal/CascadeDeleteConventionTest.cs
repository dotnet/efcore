// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class CascadeDeleteConventionTest
    {
        [Fact]
        public void Cascade_delete_is_set_when_required_FK_is_added()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Post>()
                .Property(e => e.BlogId)
                .IsRequired();

            var fk = modelBuilder.Entity<Blog>()
                .HasMany(e => e.Posts)
                .WithOne(e => e.Blog).Metadata;

            Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
        }

        [Fact]
        public void Cascade_delete_is_not_set_when_optional_FK_is_added()
        {
            var modelBuilder = CreateModelBuilder();

            var fk = modelBuilder.Entity<Blog>()
                .HasMany(e => e.Posts)
                .WithOne(e => e.Blog).Metadata;

            Assert.Equal(DeleteBehavior.ClientSetNull, fk.DeleteBehavior);
        }

        [Fact]
        public void Cascade_delete_is_set_when_optional_FK_is_made_required()
        {
            var modelBuilder = CreateModelBuilder();

            var fk = modelBuilder.Entity<Blog>()
                .HasMany(e => e.Posts)
                .WithOne(e => e.Blog).Metadata;

            modelBuilder.Entity<Post>()
                .Property(e => e.BlogId)
                .IsRequired();

            Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
        }

        [Fact]
        public void Cascade_delete_is_not_set_when_required_FK_is_made_optional()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Post>()
                .Property(e => e.BlogId)
                .IsRequired();

            var fk = modelBuilder.Entity<Blog>()
                .HasMany(e => e.Posts)
                .WithOne(e => e.Blog).Metadata;

            modelBuilder.Entity<Post>()
                .Property(e => e.BlogId)
                .IsRequired(false);

            Assert.Equal(DeleteBehavior.ClientSetNull, fk.DeleteBehavior);
        }

        [Fact]
        public void Cascade_delete_is_not_changed_when_set_explicitly_on_added_FK()
        {
            var modelBuilder = CreateModelBuilder();

            var fk = modelBuilder.Entity<Blog>()
                .HasMany(e => e.Posts)
                .WithOne(e => e.Blog)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata;

            Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
        }

        [Fact]
        public void Cascade_delete_is_not_changed_when_set_explicitly_on_FK()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Post>()
                .Property(e => e.BlogId)
                .IsRequired();

            var fk = modelBuilder.Entity<Blog>()
                .HasMany(e => e.Posts)
                .WithOne(e => e.Blog)
                .OnDelete(DeleteBehavior.Cascade)
                .Metadata;

            modelBuilder.Entity<Post>()
                .Property(e => e.BlogId)
                .IsRequired(false);

            Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
        }

        private class Blog
        {
            public int Id { get; set; }

            public ICollection<Post> Posts { get; set; }
        }

        private class Post
        {
            public int Id { get; set; }

            public Blog Blog { get; set; }
            public int? BlogId { get; set; }
        }

        private static ModelBuilder CreateModelBuilder()
            => new ModelBuilder(
                new CoreConventionSetBuilder(
                        new CoreConventionSetBuilderDependencies(new CoreTypeMapper()))
                    .CreateConventionSet());
    }
}
