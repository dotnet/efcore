// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    [Table("DefaultContainerStore.PageView")]
    public class PageView
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PageViewId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Viewed { get; set; }

        [Required]
        [StringLength(500)]
        public string PageUrl { get; set; }

        public virtual Login Login { get; set; }
    }
}
