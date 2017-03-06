using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E2E.Sqlite
{
    public partial class OneToManyDependent
    {
        [Column("OneToManyDependentID1", TypeName = "INT")]
        public long OneToManyDependentId1 { get; set; }
        [Column("OneToManyDependentID2", TypeName = "INT")]
        public long OneToManyDependentId2 { get; set; }
        [Required]
        [Column(TypeName = "VARCHAR")]
        public string SomeDependentEndColumn { get; set; }
        [Column("OneToManyDependentFK1", TypeName = "INT")]
        public long? OneToManyDependentFk1 { get; set; }
        [Column("OneToManyDependentFK2", TypeName = "INT")]
        public long? OneToManyDependentFk2 { get; set; }

        [ForeignKey("OneToManyDependentFk1,OneToManyDependentFk2")]
        [InverseProperty("OneToManyDependent")]
        public OneToManyPrincipal OneToManyDependentFk { get; set; }
    }
}
