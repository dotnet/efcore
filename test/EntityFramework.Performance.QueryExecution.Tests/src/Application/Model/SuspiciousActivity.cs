// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DefaultContainerStore.SuspiciousActivity")]
    public partial class SuspiciousActivity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SuspiciousActivityId { get; set; }

        [Required]
        public string Activity { get; set; }

        [StringLength(50)]
        public string Username { get; set; }

        public virtual Login Login { get; set; }
    }
}
