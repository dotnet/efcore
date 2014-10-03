// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution.Model
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DefaultContainerStore.Barcode")]
    public partial class Barcode
    {
        public Barcode()
        {
            BadScans = new HashSet<IncorrectScan>();
        }

        [Key]
        [MaxLength(50)]
        public byte[] Code { get; set; }

        public int ProductId { get; set; }

        [Required]
        public string Text { get; set; }

        public virtual Product Product { get; set; }

        public virtual BarcodeDetail Detail { get; set; }

        public virtual ICollection<IncorrectScan> BadScans { get; set; }
    }
}
