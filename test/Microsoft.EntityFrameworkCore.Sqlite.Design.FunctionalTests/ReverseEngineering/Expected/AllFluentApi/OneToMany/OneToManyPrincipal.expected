using System;
using System.Collections.Generic;

namespace E2E.Sqlite
{
    public partial class OneToManyPrincipal
    {
        public OneToManyPrincipal()
        {
            OneToManyDependent = new HashSet<OneToManyDependent>();
        }

        public long OneToManyPrincipalId1 { get; set; }
        public long OneToManyPrincipalId2 { get; set; }
        public string Other { get; set; }

        public ICollection<OneToManyDependent> OneToManyDependent { get; set; }
    }
}
