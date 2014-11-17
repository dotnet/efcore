// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFramework.Microbenchmarks.QueryExecutionPerf.Model;

namespace EntityFramework.Microbenchmarks.QueryExecutionPerf.Model
{
    [Table("DefaultContainerStore.LastLogin")]
    public class LastLogin
    {
        public LastLogin()
        {
            SmartCards = new HashSet<SmartCard>();
        }

        [Key]
        [StringLength(50)]
        public string Username { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime LoggedIn { get; set; }

        [StringLength(50)]
        public string SmartcardUsername { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? LoggedOut { get; set; }

        public virtual Login Login { get; set; }

        public virtual ICollection<SmartCard> SmartCards { get; set; }
    }
}
