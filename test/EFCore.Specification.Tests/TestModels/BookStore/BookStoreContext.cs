// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.BookStore
{
    public class BookStoreContext : PoolableDbContext
    {
        public BookStoreContext(DbContextOptions<BookStoreContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<File> Files { get; set; }

        public static void Seed(BookStoreContext context)
        {
            var books = CreateTestBooks();
            context.Books.AddRange(books);

            context.SaveChanges();
        }

        private static Book[] CreateTestBooks()
        {
            return new[] {
                new Book
                {
                    Title = "Erlkönig (Ballade)",
                    Author = new Author { FirstName = "Johann Wolfgang", LastName = "Goethe" },
                    File = new File
                    {
                        Name = "erlkoenig",
                        FileExtension = ".txt",
                        Data = ReadFullyFromResource("Microsoft.EntityFrameworkCore.TestModels.BookStore.erlkoenig.txt")
                    }
                },
                new Book
                {
                    Title = "Ulysses",
                    Author = new Author { FirstName = "Alfred", LastName = "Tennyson" },
                    File = new File
                    {
                        Name = "ulysses",
                        FileExtension = ".txt",
                        Data = ReadFullyFromResource("Microsoft.EntityFrameworkCore.TestModels.BookStore.ulysses.txt")
                    }
                }};
        }

        private static byte[] ReadFullyFromResource(string resourceName)
        {
            static byte[] ReadFully(Stream input)
            {
                using var ms = new MemoryStream();
                input.CopyTo(ms);
                return ms.ToArray();
            }

            byte[] fileArray;

            var assembly = Assembly.GetAssembly(typeof(BookStoreTestBase<>));
            var names = assembly.GetManifestResourceNames();
            
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                fileArray = ReadFully(stream);
            }

            return fileArray;
        }
    }
}
