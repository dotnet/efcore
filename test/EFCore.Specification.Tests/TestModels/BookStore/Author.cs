// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.BookStore
{
    public class Author
    {
        public int AuthorId { get; set; }

        [Required]
        [StringLength(160, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(160, MinimumLength = 2)]
        public string LastName { get; set; }

        public virtual List<Book> Books { get; set; }
    }
}
