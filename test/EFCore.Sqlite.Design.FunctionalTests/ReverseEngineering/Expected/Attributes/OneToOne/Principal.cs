using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E2E.Sqlite
{
    public partial class Principal
    {
        public long Id { get; set; }

        [InverseProperty("Principal")]
        public Dependent Dependent { get; set; }
    }
}
