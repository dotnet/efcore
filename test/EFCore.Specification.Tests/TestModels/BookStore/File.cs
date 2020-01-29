// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.BookStore
{
    public class File
    {
        public int FileId { get; set; }

        public string Name { get; set; }

        [Required]
        public byte[] Data { get; set; }

        [StringLength(100)]
        public string FileExtension { get; set; }
    }
}
