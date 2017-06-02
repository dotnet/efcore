using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E2E.Sqlite
{
    public partial class Dependent
    {
        [Column(TypeName = "INT")]
        public long Id { get; set; }
        [Column(TypeName = "INT")]
        public long PrincipalId { get; set; }

        [ForeignKey("PrincipalId")]
        [InverseProperty("Dependent")]
        public Principal Principal { get; set; }
    }
}
