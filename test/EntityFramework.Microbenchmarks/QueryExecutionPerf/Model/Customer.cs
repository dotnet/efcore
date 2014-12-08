// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFramework.Microbenchmarks.QueryExecutionPerf.Model;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    [Table("DefaultContainerStore.Customer")]
    public class Customer
    {
        public Customer()
        {
            Complaints = new HashSet<Complaint>();
            Logins = new HashSet<Login>();
            Orders = new HashSet<Order>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(32)]
        public string ContactInfo_Email { get; set; }

        [Required]
        [StringLength(16)]
        public string ContactInfo_HomePhone_PhoneNumber { get; set; }

        [StringLength(16)]
        public string ContactInfo_HomePhone_Extension { get; set; }

        [Required]
        [StringLength(16)]
        public string ContactInfo_WorkPhone_PhoneNumber { get; set; }

        [StringLength(16)]
        public string ContactInfo_WorkPhone_Extension { get; set; }

        [Required]
        [StringLength(16)]
        public string ContactInfo_MobilePhone_PhoneNumber { get; set; }

        [StringLength(16)]
        public string ContactInfo_MobilePhone_Extension { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Auditing_ModifiedDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Auditing_ModifiedBy { get; set; }

        [Required]
        [StringLength(20)]
        public string Auditing_Concurrency_Token { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? Auditing_Concurrency_QueriedDateTime { get; set; }

        public int? HusbandId { get; set; }

        public virtual Customer Husband { get; set; }

        public virtual Customer Wife { get; set; }

        public virtual ICollection<Complaint> Complaints { get; set; }

        public virtual CustomerInfo Info { get; set; }

        public virtual ICollection<Login> Logins { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
