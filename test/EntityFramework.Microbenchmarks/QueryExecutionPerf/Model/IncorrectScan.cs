// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    [Table("DefaultContainerStore.IncorrectScan")]
    public class IncorrectScan
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IncorrectScanId { get; set; }

        [Required]
        [MaxLength(50)]
        public byte[] ExpectedCode { get; set; }

        [MaxLength(50)]
        public byte[] ActualCode { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime ScanDate { get; set; }

        [Required]
        public string Details { get; set; }

        public virtual Barcode ExpectedBarcode { get; set; }

        public virtual Barcode ActualBarcode { get; set; }
    }
}
