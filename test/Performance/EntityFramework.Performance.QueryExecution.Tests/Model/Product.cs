// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QueryExecution.Model
{
    [Table("DefaultContainerStore.Product")]
    public class Product
    {
        public Product()
        {
            BackOrderLines = new HashSet<BackOrderLine>();
            Barcodes = new HashSet<Barcode>();
            DiscontinuedProducts = new HashSet<DiscontinuedProduct>();
            OrderLines = new HashSet<OrderLine>();
            ProductPageViews = new HashSet<ProductPageView>();
            Photos = new HashSet<ProductPhoto>();
            Reviews = new HashSet<ProductReview>();
            Orders = new HashSet<Order>();
            Suppliers = new HashSet<Supplier>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ProductId { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public decimal Dimensions_Width { get; set; }

        public decimal Dimensions_Height { get; set; }

        public decimal Dimensions_Depth { get; set; }

        [Required]
        public string BaseConcurrency { get; set; }

        [Required]
        [StringLength(20)]
        public string ComplexConcurrency_Token { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ComplexConcurrency_QueriedDateTime { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime NestedComplexConcurrency_ModifiedDate { get; set; }

        [Required]
        [StringLength(50)]
        public string NestedComplexConcurrency_ModifiedBy { get; set; }

        [Required]
        [StringLength(20)]
        public string NestedComplexConcurrency_Concurrency_Token { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? NestedComplexConcurrency_Concurrency_QueriedDateTime { get; set; }

        public virtual ICollection<BackOrderLine> BackOrderLines { get; set; }

        public virtual ICollection<Barcode> Barcodes { get; set; }

        public virtual ICollection<DiscontinuedProduct> DiscontinuedProducts { get; set; }

        public virtual ICollection<OrderLine> OrderLines { get; set; }

        public virtual ProductDetail Detail { get; set; }

        public virtual ICollection<ProductPageView> ProductPageViews { get; set; }

        public virtual ICollection<ProductPhoto> Photos { get; set; }

        public virtual ICollection<ProductReview> Reviews { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        public virtual ICollection<Supplier> Suppliers { get; set; }
    }
}
