// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DefaultContainerStore.Resolution")]
    public partial class Resolution
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ResolutionId { get; set; }

        [Required]
        public string Details { get; set; }

        public virtual Complaint Complaint { get; set; }
    }
}
