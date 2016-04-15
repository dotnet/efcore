// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class DataAnnotationFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract DataAnnotationContext CreateContext(TTestStore testStore);

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }

    public class DataAnnotationContext : DbContext
    {
        public DataAnnotationContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<One> Ones { get; set; }

        public DbSet<Two> Twos { get; set; }

        public DbSet<Book> Books { get; set; }

        public DbSet<BookDetail> BookDetails { get; set; }

        public DbSet<AdditionalBookDetail> AdditionalBookDetails { get; set; }
    }

    [Table("Sample")]
    public class One
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UniqueNo { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; }

        [NotMapped]
        public string IgnoredProperty { get; set; }

        [Required]
        [Column("Name")]
        public string RequiredColumn { get; set; }

        [MaxLength(10)]
        public string MaxLengthProperty { get; set; }
    }

    public class Two
    {
        [Key]
        public int Id { get; set; }

        [StringLength(16)]
        public string Data { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        public virtual C NavC { get; set; }
    }

    [NotMapped]
    public class C
    {
        public int Id { get; set; }
    }

    public class Book
    {
        public string Id { get; set; }

        public virtual BookDetail BookDetail { get; set; }

        [NotMapped]
        public virtual UselessBookDetails UselessBookDetails { get; set; }
    }

    public class BookDetail
    {
        public int Id { get; set; }

        public string BookId { get; set; }

        public int? AdditionalBookDetailId { get; set; }

        [Required]
        public virtual Book Book { get; set; }
    }

    public class AdditionalBookDetail
    {
        public int Id { get; set; }

        [Required]
        public virtual BookDetail BookDetail { get; set; }
    }

    public class UselessBookDetails
    {
        public int Id { get; set; }

        [NotMapped]
        public virtual Book Book { get; set; }
    }

    public class DataAnnotationModelInitializer
    {
        public static void Seed(DataAnnotationContext context)
        {
            context.Ones.Add(new One { RequiredColumn = "First", RowVersion = new Guid("00000001-0000-0000-0000-000000000001") });
            context.Ones.Add(new One { RequiredColumn = "Second", RowVersion = new Guid("00000001-0000-0000-0000-000000000001") });

            context.Twos.Add(new Two { Data = "First" });
            context.Twos.Add(new Two { Data = "Second" });

            context.Books.Add(new Book { Id = "Book1" });

            context.SaveChanges();
        }
    }
}
