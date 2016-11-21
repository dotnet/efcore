using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E2E.Sqlite
{
    public partial class OneToManyPrincipal
    {
        public OneToManyPrincipal()
        {
            OneToManyDependent = new HashSet<OneToManyDependent>();
        }

        [Column("OneToManyPrincipalID1", TypeName = "INT")]
        public long OneToManyPrincipalId1 { get; set; }
        [Column("OneToManyPrincipalID2", TypeName = "INT")]
        public long OneToManyPrincipalId2 { get; set; }
        [Required]
        public string Other { get; set; }

        [InverseProperty("OneToManyDependentFk")]
        public ICollection<OneToManyDependent> OneToManyDependent { get; set; }
    }
}
