// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    [Table("DefaultContainerStore.ComputerDetail")]
    public class ComputerDetail
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ComputerDetailId { get; set; }

        [Required]
        public string Manufacturer { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        public string Serial { get; set; }

        [Required]
        public string Specifications { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime PurchaseDate { get; set; }

        public decimal Dimensions_Width { get; set; }

        public decimal Dimensions_Height { get; set; }

        public decimal Dimensions_Depth { get; set; }

        public virtual Computer Computer { get; set; }
    }
}
