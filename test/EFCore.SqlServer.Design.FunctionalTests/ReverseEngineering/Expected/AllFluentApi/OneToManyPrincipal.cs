using System;
using System.Collections.Generic;

namespace E2ETest.Namespace
{
    public partial class OneToManyPrincipal
    {
        public OneToManyPrincipal()
        {
            OneToManyDependent = new HashSet<OneToManyDependent>();
        }

        public int OneToManyPrincipalId1 { get; set; }
        public int OneToManyPrincipalId2 { get; set; }
        public string Other { get; set; }

        public ICollection<OneToManyDependent> OneToManyDependent { get; set; }
    }
}
