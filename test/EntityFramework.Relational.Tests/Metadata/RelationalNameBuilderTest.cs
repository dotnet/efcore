// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Metadata
{
    public class RelationalNameBuilderTest
    {
        [Fact]
        public void Names_are_generated_if_not_specified()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity<Blog>(b =>
                {
                    b.Key(k => k.BlogId);
                    b.Property(e => e.BlogId);
                });

            modelBuilder.Entity<Post>(b =>
                {
                    b.Key(k => k.PostId);
                    b.Property(e => e.PostId);
                    b.Property(e => e.BelongsToBlogId);
                    b.ForeignKey<Blog>(p => p.BelongsToBlogId);
                    b.Index(ix => ix.PostId);
                });

            Assert.Equal("Blog", NameBuilder().TableName(modelBuilder.Entity<Blog>().Metadata));
            Assert.Equal("Post", NameBuilder().TableName(modelBuilder.Entity<Post>().Metadata));

            Assert.Equal("BlogId", NameBuilder().ColumnName(modelBuilder.Entity<Blog>().Property(t => t.BlogId).Metadata));
            Assert.Equal("PostId", NameBuilder().ColumnName(modelBuilder.Entity<Post>().Property(t => t.PostId).Metadata));
            Assert.Equal("BelongsToBlogId", NameBuilder().ColumnName(modelBuilder.Entity<Post>().Property(t => t.BelongsToBlogId).Metadata));

            Assert.Equal("PK_Blog", NameBuilder().KeyName(modelBuilder.Entity<Blog>().Metadata.GetPrimaryKey()));
            Assert.Equal("PK_Post", NameBuilder().KeyName(modelBuilder.Entity<Post>().Metadata.GetPrimaryKey()));

            Assert.Equal("FK_Post_Blog_BelongsToBlogId", NameBuilder().ForeignKeyName(modelBuilder.Entity<Post>().Metadata.ForeignKeys.Single()));

            Assert.Equal("IX_Post_PostId", NameBuilder().IndexName(modelBuilder.Entity<Post>().Metadata.Indexes.Single()));
        }

        [Fact]
        public void Build_fills_in_unique_constraint_name_if_not_specified()
        {
            IKey uniqueConstraint1 = null;
            IKey uniqueConstraint2 = null;

            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("A",
                b =>
                    {
                        var id = b.Property<int>("Id").Metadata;
                        var p1 = b.Property<long>("P1").ForRelational(rb => rb.Column("C1")).Metadata;
                        var p2 = b.Property<string>("P2").Metadata;
                        b.Key("Id").ForRelational().Name("PK");
                        uniqueConstraint1 = b.Metadata.AddKey(new[] { id, p1 });
                        uniqueConstraint2 = b.Metadata.AddKey(new[] { p2 });
                    });

            Assert.Equal("UC_A_Id_C1", NameBuilder().KeyName(uniqueConstraint1));
            Assert.Equal("UC_A_P2", NameBuilder().KeyName(uniqueConstraint2));

            modelBuilder.Entity("A").ForRelational().Table("T", "dbo");

            Assert.Equal("UC_dbo.T_Id_C1", NameBuilder().KeyName(uniqueConstraint1));
            Assert.Equal("UC_dbo.T_P2", NameBuilder().KeyName(uniqueConstraint2));
        }

        private class Blog
        {
            public int BlogId { get; set; }
        }

        private class Post
        {
            public int PostId { get; set; }
            public int BelongsToBlogId { get; set; }
        }

        [Fact]
        public void Name_for_multi_column_FKs()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity<Principal>()
                .Key(k => new { k.Id0, k.Id1 });

            modelBuilder.Entity<Dependent>(b =>
                {
                    b.Key(k => k.Id);
                    b.ForeignKey<Principal>(p => new { p.FkAAA, p.FkZZZ });
                });

            Assert.Equal("FK_Dependent_Principal_FkAAA_FkZZZ", 
                NameBuilder().ForeignKeyName(modelBuilder.Entity<Dependent>().Metadata.ForeignKeys.Single()));
        }

        [Fact]
        public void Name_for_multi_column_Indexes()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder.Entity<Dependent>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.Id);
                    b.Property(e => e.FkAAA).ForRelational().Column("ColumnAaa");
                    b.Property(e => e.FkZZZ).ForRelational().Column("ColumnZzz");
                    b.ForRelational().Table("MyTable");
                    b.Index(e => new { e.FkAAA, e.FkZZZ });
                });

            Assert.Equal("IX_MyTable_ColumnAaa_ColumnZzz",
                NameBuilder().IndexName(modelBuilder.Entity<Dependent>().Metadata.Indexes.Single()));
        }

        private class Principal
        {
            public int Id0 { get; set; }
            public int Id1 { get; set; }
        }

        private class Dependent
        {
            public int Id { get; set; }
            public int FkAAA { get; set; }
            public int FkZZZ { get; set; }
        }

        private static RelationalNameBuilder NameBuilder()
        {
            return new RelationalNameBuilder(RelationalTestHelpers.ExtensionProvider());
        }
    }
}
