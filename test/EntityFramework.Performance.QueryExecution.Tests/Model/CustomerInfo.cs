// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DefaultContainerStore.Info")]
    public partial class CustomerInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerInfoId { get; set; }

        [Required]
        public string Information { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
