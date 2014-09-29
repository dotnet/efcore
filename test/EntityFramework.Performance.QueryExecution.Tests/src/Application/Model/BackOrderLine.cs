// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DefaultContainerStore.BackOrderLine")]
    public partial class BackOrderLine : OrderLine
    {
        [Column(TypeName = "datetime2")]
        public DateTime ETA { get; set; }

        public int? Supplier_SupplierId { get; set; }

        public virtual Supplier Supplier { get; set; }
    }
}
