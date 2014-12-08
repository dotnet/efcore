// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    [Table("DefaultContainerStore.License")]
    public class License
    {
        [Key]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string LicenseNumber { get; set; }

        [Required]
        public string LicenseClass { get; set; }

        [Required]
        public string Restrictions { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime ExpirationDate { get; set; }

        public virtual Driver Driver { get; set; }
    }
}
