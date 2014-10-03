// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DefaultContainerStore.LastLogin")]
    public partial class LastLogin
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
