// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    [Table("DefaultContainerStore.ProductPageView")]
    public class ProductPageView
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PageViewId { get; set; }

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }
    }
}
