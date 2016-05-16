using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E2E.Sqlite
{
    public partial class Dependent
    {
        public string Id { get; set; }
        [Column(TypeName = "INT")]
        public long? PrincipalId { get; set; }
    }
}
