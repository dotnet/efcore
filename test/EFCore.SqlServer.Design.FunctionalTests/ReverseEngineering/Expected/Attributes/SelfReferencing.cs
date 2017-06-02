using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E2ETest.Namespace.SubDir
{
    public partial class SelfReferencing
    {
        public SelfReferencing()
        {
            InverseSelfReferenceFkNavigation = new HashSet<SelfReferencing>();
        }

        [Column("SelfReferencingID")]
        public int SelfReferencingId { get; set; }
        [Required]
        [StringLength(20)]
        public string Name { get; set; }
        [Required]
        [StringLength(100)]
        public string Description { get; set; }
        [Column("SelfReferenceFK")]
        public int? SelfReferenceFk { get; set; }

        [ForeignKey("SelfReferenceFk")]
        [InverseProperty("InverseSelfReferenceFkNavigation")]
        public SelfReferencing SelfReferenceFkNavigation { get; set; }
        [InverseProperty("SelfReferenceFkNavigation")]
        public ICollection<SelfReferencing> InverseSelfReferenceFkNavigation { get; set; }
    }
}
