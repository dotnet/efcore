// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QueryExecution.Model
{
    [Table("DefaultContainerStore.OrderNote")]
    public class OrderNote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NoteId { get; set; }

        [Required]
        public string Note { get; set; }

        public int? OrderId { get; set; }

        public virtual Order Order { get; set; }
    }
}
