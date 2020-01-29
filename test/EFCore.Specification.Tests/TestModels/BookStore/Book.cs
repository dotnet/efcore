// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.BookStore
{
    public class Book
    {
        public int BookId { get; set; }

        public int AuthorId { get; set; }

        public int FileId { get; set; }

        [Required]
        [StringLength(160, MinimumLength = 2)]
        public string Title { get; set; }

        public virtual Author Author { get; set; }

        public virtual File File { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public Book()
        {
            Created = DateTime.UtcNow;
        }
    }
}
