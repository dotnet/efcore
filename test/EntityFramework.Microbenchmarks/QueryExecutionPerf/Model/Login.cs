// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFramework.Microbenchmarks.QueryExecutionPerf.Model;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    [Table("DefaultContainerStore.Login")]
    public class Login
    {
        public Login()
        {
            Orders = new HashSet<Order>();
            RSATokens = new HashSet<RSAToken>();
            SuspiciousActivities = new HashSet<SuspiciousActivity>();
            SentMessages = new HashSet<Message>();
            ReceivedMessages = new HashSet<Message>();
            PageViews = new HashSet<PageView>();
            PasswordResets = new HashSet<PasswordReset>();
        }

        [Key]
        [StringLength(50)]
        public string Username { get; set; }

        public int CustomerId { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual LastLogin LastLogin { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        public virtual ICollection<RSAToken> RSATokens { get; set; }

        public virtual ICollection<SuspiciousActivity> SuspiciousActivities { get; set; }

        public virtual ICollection<Message> SentMessages { get; set; }

        public virtual ICollection<Message> ReceivedMessages { get; set; }

        public virtual ICollection<PageView> PageViews { get; set; }

        public virtual ICollection<PasswordReset> PasswordResets { get; set; }
    }
}
