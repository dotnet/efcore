// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    [Table("DefaultContainerStore.PasswordReset")]
    public class PasswordReset
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ResetNo { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        public string TempPassword { get; set; }

        [Required]
        public string EmailedTo { get; set; }

        public virtual Login Login { get; set; }
    }
}
