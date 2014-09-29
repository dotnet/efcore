// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DefaultContainerStore.ProductPageView")]
    public partial class ProductPageView
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PageViewId { get; set; }

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }
    }
}
