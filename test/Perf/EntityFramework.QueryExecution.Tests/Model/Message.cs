// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QueryExecution.Model
{
    [Table("DefaultContainerStore.Message")]
    public class Message
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MessageId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string FromUsername { get; set; }

        [Required]
        [StringLength(50)]
        public string ToUsername { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Sent { get; set; }

        [Required]
        public string Subject { get; set; }

        public string Body { get; set; }

        public bool IsRead { get; set; }

        public virtual Login Sender { get; set; }

        public virtual Login Recipient { get; set; }
    }
}
