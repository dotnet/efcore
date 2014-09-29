// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DefaultContainerStore.Driver")]
    public partial class Driver
    {
        [Key]
        [StringLength(100)]
        public string Name { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime BirthDate { get; set; }

        public virtual License License { get; set; }
    }
}
