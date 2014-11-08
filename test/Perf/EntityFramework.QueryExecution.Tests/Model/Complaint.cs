// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QueryExecution.Model
{
    [Table("DefaultContainerStore.Complaint")]
    public class Complaint
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ComplaintId { get; set; }

        public int? CustomerId { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Logged { get; set; }

        [Required]
        public string Details { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual Resolution Resolution { get; set; }
    }
}
