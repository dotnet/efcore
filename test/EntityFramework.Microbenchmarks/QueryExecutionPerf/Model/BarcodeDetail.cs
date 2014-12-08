// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    [Table("DefaultContainerStore.Detail")]
    public class BarcodeDetail
    {
        [Key]
        [MaxLength(50)]
        public byte[] Code { get; set; }

        [Required]
        public string RegisteredTo { get; set; }

        public virtual Barcode Barcode { get; set; }
    }
}
