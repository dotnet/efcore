// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DefaultContainerStore.RSAToken")]
    public partial class RSAToken
    {
        [Key]
        [StringLength(20)]
        public string Serial { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Issued { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        public virtual Login Login { get; set; }
    }
}
