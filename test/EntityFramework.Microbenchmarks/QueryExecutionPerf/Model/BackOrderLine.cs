// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFramework.Microbenchmarks.QueryExecutionPerf.Model;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    [Table("DefaultContainerStore.BackOrderLine")]
    public class BackOrderLine : OrderLine
    {
        [Column(TypeName = "datetime2")]
        public DateTime ETA { get; set; }

        public int? Supplier_SupplierId { get; set; }

        public virtual Supplier Supplier { get; set; }
    }
}
