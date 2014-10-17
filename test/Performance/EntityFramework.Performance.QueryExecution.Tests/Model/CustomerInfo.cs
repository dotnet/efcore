// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QueryExecution.Model
{
    [Table("DefaultContainerStore.Info")]
    public class CustomerInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerInfoId { get; set; }

        [Required]
        public string Information { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
