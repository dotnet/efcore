// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFramework.Microbenchmarks.QueryExecutionPerf.Model;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    [Table("DefaultContainerStore.Order")]
    public class Order
    {
        public Order()
        {
            BackOrderLines = new HashSet<BackOrderLine>();
            Notes = new HashSet<OrderNote>();
            OrderLines = new HashSet<OrderLine>();
            Products = new HashSet<Product>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OrderId { get; set; }

        public int? CustomerId { get; set; }

        [Required]
        [StringLength(20)]
        public string Concurrency_Token { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? Concurrency_QueriedDateTime { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        public virtual ICollection<BackOrderLine> BackOrderLines { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual Login Login { get; set; }

        public virtual ICollection<OrderNote> Notes { get; set; }

        public virtual ICollection<OrderLine> OrderLines { get; set; }

        public virtual OrderQualityCheck OrderQualityCheck { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
